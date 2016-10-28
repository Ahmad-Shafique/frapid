﻿using System;
using System.Linq;
using System.Web;
using Frapid.Areas;
using Frapid.Configuration;
using Frapid.Framework;
using Serilog;

namespace Frapid.Web
{
    public sealed class FrapidApplication : IHttpModule
    {
        public void Init(HttpApplication app)
        {
            app.BeginRequest += this.App_BeginRequest;
            app.EndRequest += this.App_EndRequest;
            app.PostAuthenticateRequest += this.App_PostAuthenticateRequest;
            app.Error += this.App_Error;
        }


        public void Dispose()
        {
        }

        private void SetCorsHeaders()
        {
            var context = FrapidHttpContext.GetCurrent();

            if (context == null)
            {
                return;
            }

            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "HEAD,GET,POST,PUT,DELETE,OPTIONS");
            context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        }

        private void App_PostAuthenticateRequest(object sender, EventArgs eventArgs)
        {
            string tenant = TenantConvention.GetTenant();
            string file = TenantStaticContentHelper.GetFile(tenant, FrapidHttpContext.GetCurrent());

            if (!string.IsNullOrWhiteSpace(file))
            {
                //We found the requested file on the tenant's "wwwroot" directory.
                FrapidHttpContext.GetCurrent().RewritePath(file);
            }
        }

        private void App_Error(object sender, EventArgs e)
        {
            var context = FrapidHttpContext.GetCurrent();
            var exception = context.Server.GetLastError();

            if (exception != null)
            {
                Log.Error("Exception. {exception}", exception);
            }
        }

        private void Handle404Error()
        {
            var context = FrapidHttpContext.GetCurrent();
            int statusCode = context.Response.StatusCode;

            if (statusCode != 404)
            {
                return;
            }

            context.Server.ClearError();
            context.Response.TrySkipIisCustomErrors = true;
            string path = context.Request.Url.AbsolutePath;

            var ignoredPaths = new[]
            {
                "/api",
                "/dashboard",
                "/content-not-found"
            };

            if (!ignoredPaths.Any(x => path.StartsWith(x)))
            {
                context.Server.TransferRequest("/content-not-found?path=" + path, true);
            }
        }

        public void App_EndRequest(object sender, EventArgs e)
        {
            this.SetCorsHeaders();
            this.Handle404Error();
        }

        public void App_BeginRequest(object sender, EventArgs e)
        {
            var context = FrapidHttpContext.GetCurrent();

            if (context == null)
            {
                return;
            }

            string domain = TenantConvention.GetDomain();
            Log.Verbose(
                $"Got a {context.Request.HttpMethod} request {context.Request.AppRelativeCurrentExecutionFilePath} on domain {domain}.");

            bool enforceSsl = TenantConvention.EnforceSsl(domain);

            if (!enforceSsl)
            {
                Log.Verbose($"SSL was not enforced on domain {domain}.");
                return;
            }

            if (context.Request.Url.Scheme == "https")
            {
                context.Response.AddHeader("Strict-Transport-Security", "max-age=31536000");
            }
            else if (context.Request.Url.Scheme == "http")
            {
                string path = "https://" + context.Request.Url.Host + context.Request.Url.PathAndQuery;
                context.Response.Status = "301 Moved Permanently";
                context.Response.AddHeader("Location", path);
            }
        }
    }
}