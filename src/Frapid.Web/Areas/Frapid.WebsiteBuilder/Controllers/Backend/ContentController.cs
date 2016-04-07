﻿using System.Web.Mvc;
using Frapid.Areas.Authorization;
using Frapid.Dashboard.Controllers;
using Frapid.WebsiteBuilder.DAL;
using Frapid.WebsiteBuilder.DTO;

namespace Frapid.WebsiteBuilder.Controllers.Backend
{
    public class ContentController : DashboardController
    {
        [Route("dashboard/website/contents")]
        [RestrictAnonymous]
        [MenuPolicy]
        public ActionResult Index()
        {
            return this.FrapidView(this.GetRazorView<AreaRegistration>("Content/Index.cshtml"));
        }

        [Route("dashboard/website/contents/manage")]
        [Route("dashboard/website/contents/new")]
        [RestrictAnonymous]
        [MenuPolicy]
        public ActionResult Manage(int contentId = 0)
        {
            var model = Contents.Get(contentId) ?? new Content();
            return this.FrapidView(this.GetRazorView<AreaRegistration>("Content/Manage.cshtml"), model);
        }
    }
}