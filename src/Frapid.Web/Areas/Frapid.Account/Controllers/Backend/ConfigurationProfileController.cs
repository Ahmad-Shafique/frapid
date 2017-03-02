﻿using System.Web.Mvc;
using Frapid.Areas.Authorization;
using Frapid.Dashboard;
using Frapid.Dashboard.Controllers;

namespace Frapid.Account.Controllers.Backend
{
    public class ConfigurationProfileController : DashboardController
    {
        [Route("dashboard/account/configuration-profile")]
        [MenuPolicy]
        public ActionResult Index()
        {
            return this.FrapidView(this.GetRazorView<AreaRegistration>("ConfigurationProfile/Index.cshtml", this.Tenant));
        }
    }
}