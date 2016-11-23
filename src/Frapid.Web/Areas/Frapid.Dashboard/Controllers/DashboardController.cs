﻿using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Frapid.Framework.Extensions;

namespace Frapid.Dashboard.Controllers
{
    public class DashboardController : BackendController
    {
        private static readonly string LandingPage = "~/Areas/Frapid.Dashboard/Views/Default/LandingPage.cshtml";

        private string GetLayoutFile()
        {
            string theme = Configuration.GetDefaultTheme(this.Tenant);
            return ThemeConfiguration.GetLayout(this.Tenant, theme);
        }

        private string GetLayoutPath()
        {
            string layout = Configuration.GetCurrentThemePath(this.Tenant);
            string layoutDirectory = HostingEnvironment.MapPath(layout);

            if (layoutDirectory != null && Directory.Exists(layoutDirectory))
            {
                return layout;
            }

            return null;
        }

        private bool IsAjax(HttpContextBase context)
        {
            if (context.Request.IsAjaxRequest())
            {
                return true;
            }

            string query = context.Request.QueryString["IsAjaxRequest"];

            if (!string.IsNullOrWhiteSpace(query))
            {
                if (query.ToUpperInvariant().StartsWith("T"))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            this.ViewBag.LayoutPath = this.GetLayoutPath();
            this.ViewBag.LayoutFile = this.GetLayoutFile();

            bool isAjax = this.IsAjax(filterContext.HttpContext);

            if (!isAjax)
            {
                this.ViewBag.Layout = this.ViewBag.LayoutPath + this.ViewBag.LayoutFile;
            }
        }

        protected ContentResult FrapidView(string path, object model = null)
        {
            bool isAjax = this.IsAjax(this.HttpContext);
            return this.View(isAjax ? path : LandingPage, model);
        }
    }
}