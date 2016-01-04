﻿using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Frapid.Areas;
using Frapid.Configuration;
using Frapid.WebsiteBuilder.Models.Themes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Frapid.WebsiteBuilder.Controllers.Backend
{
    [AntiForgery]
    public class ThemeController : FrapidController
    {
        [Route("dashboard/my/website/themes")]
        [RestrictAnonymous]
        public ActionResult GetThemes()
        {
            var discoverer = new ThemeDiscoverer();
            var templates = discoverer.Discover();

            return this.Json(templates, JsonRequestBehavior.AllowGet);
        }

        [Route("dashboard/my/website/themes/create")]
        [RestrictAnonymous]
        [HttpPost]
        public ActionResult Create(ThemeInfo model)
        {
            if (!ModelState.IsValid)
            {
                return this.AjaxFail("Invalid or incomplete theme information provided.", HttpStatusCode.BadRequest);
            }

            try
            {
                var creator = new ThemeCreator(model);
                creator.Create();
            }
            catch (ThemeCreateException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [Route("dashboard/my/website/themes/delete")]
        [RestrictAnonymous]
        [HttpDelete]
        public ActionResult Delete(string themeName)
        {
            try
            {
                var remover = new ThemeRemover(themeName);
                remover.Remove();
            }
            catch (ThemeRemoveException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [Route("dashboard/my/website/themes/resources")]
        [RestrictAnonymous]
        public ActionResult GetResources(string themeName)
        {
            if (string.IsNullOrWhiteSpace(themeName))
            {
                return this.AjaxFail("Invalid theme name", HttpStatusCode.BadRequest);
            }

            string catalog = DbConvention.GetCatalog();
            string path = $"~/Catalogs/{catalog}/Areas/Frapid.WebsiteBuilder/Themes/{themeName}/";
            path = HostingEnvironment.MapPath(path);

            if (path == null || !Directory.Exists(path))
            {
                return this.AjaxFail("Path not found", HttpStatusCode.NotFound);
            }

            var resource = ThemeResource.Get(path);
            resource = ThemeResource.NormalizePath(path, resource);
            string json = JsonConvert.SerializeObject(resource, Formatting.None,
                new JsonSerializerSettings {ContractResolver = new CamelCasePropertyNamesContractResolver()});

            return Content(json);
        }

        [Route("dashboard/my/website/themes/blob")]
        [RestrictAnonymous]
        public ActionResult GetBinary(string themeName, string file)
        {
            if (string.IsNullOrWhiteSpace(themeName) || string.IsNullOrWhiteSpace(file))
            {
                return this.AjaxFail("Access is denied", HttpStatusCode.BadRequest);
            }

            string path = new ThemeFileLocator(themeName, file).Locate();
            string mimeType = GetMimeType(path);
            return File(path, mimeType);
        }

        private string GetMimeType(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName);
        }

        [Route("dashboard/my/website/themes/resources/create/file")]
        [RestrictAnonymous]
        [HttpPut]
        public ActionResult CreateFile(string themeName, string container, string file, string contents)
        {
            return this.CreateResource(themeName, container, file, false, contents);
        }

        [Route("dashboard/my/website/themes/resources/edit/file")]
        [RestrictAnonymous]
        [HttpPut]
        public ActionResult EditFile(string themeName, string container, string file, string contents)
        {
            return this.CreateResource(themeName, container, file, false, contents, true);
        }

        [Route("dashboard/my/website/themes/resources/create/folder")]
        [RestrictAnonymous]
        [HttpPut]
        public ActionResult CreateFolder(string themeName, string container, string folder)
        {
            return this.CreateResource(themeName, container, folder, true, null);
        }

        private ActionResult CreateResource(string themeName, string container, string file, bool isDirectory,
            string contents, bool rewriteFile = false)
        {
            try
            {
                var resource = new ResourceCreator
                {
                    ThemeName = themeName,
                    Container = container,
                    File = file,
                    IsDirectory = isDirectory,
                    Contents = contents,
                    Rewrite = rewriteFile
                };

                resource.Create();
            }
            catch (ResourceCreateException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }

            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        [Route("dashboard/my/website/themes/resources/delete")]
        [RestrictAnonymous]
        [HttpDelete]
        public ActionResult DeleteResource(string themeName, string resource)
        {
            if (string.IsNullOrWhiteSpace(themeName) || string.IsNullOrWhiteSpace(resource))
            {
                return this.AjaxFail("Access is denied", HttpStatusCode.BadRequest);
            }

            try
            {
                var remover = new ResourceRemover(themeName, resource);
                remover.Delete();
            }
            catch (ResourceRemoveException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }

            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        [Route("dashboard/my/website/themes/resources/upload")]
        [RestrictAnonymous]
        [HttpPost]
        public ActionResult UploadResource(string themeName, string container)
        {
            if (this.Request.Files.Count > 1)
            {
                return this.AjaxFail("Only single file may be uploaded", HttpStatusCode.BadRequest);
            }

            var file = this.Request.Files[0];
            if (file == null)
            {
                return this.AjaxFail("No file was uploaded", HttpStatusCode.BadRequest);
            }

            try
            {
                var uploader = new ResourceUploader(file, themeName, container);
                uploader.Upload();
            }
            catch (ResourceUploadException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }

            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }

        [Route("dashboard/my/website/themes/upload")]
        [RestrictAnonymous]
        [HttpPost]
        public ActionResult UploadTheme()
        {
            if (this.Request.Files.Count > 1)
            {
                return this.AjaxFail("Only single file may be uploaded", HttpStatusCode.BadRequest);
            }

            var file = this.Request.Files[0];
            if (file == null)
            {
                return this.AjaxFail("No file was uploaded", HttpStatusCode.BadRequest);
            }

            try
            {
                var uploader = new ThemeUploader(file);
                return Upload(uploader);
            }
            catch (ThemeUploadException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        [Route("dashboard/my/website/themes/upload/remote")]
        [RestrictAnonymous]
        [HttpPost]
        public ActionResult UploadTheme(string url)
        {
            Uri uri;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uri) &&
                          (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

            if (result.Equals(false))
            {
                return this.AjaxFail("Invalid URL", HttpStatusCode.BadRequest);
            }

            try
            {
                var uploader = new ThemeUploader(uri);
                return this.Upload(uploader);
            }
            catch (ThemeUploadException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        private ActionResult Upload(ThemeUploader uploader)
        {
            try
            {
                uploader.Install();
            }
            catch (ThemeUploadException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.BadRequest);
            }
            catch (ThemeInstallException ex)
            {
                return this.AjaxFail(ex.Message, HttpStatusCode.BadRequest);
            }

            return Json(new {success = true}, JsonRequestBehavior.AllowGet);
        }
    }
}