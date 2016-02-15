﻿using System.Web.Mvc;
using Frapid.Areas.Authorization;
using Frapid.Dashboard.Controllers;

namespace Frapid.Config.Controllers
{
    public class CurrencyController : DashboardController
    {
        [Route("dashboard/config/currencies")]
        [RestrictAnonymous]
        public ActionResult Index()
        {
            return this.FrapidView(this.GetRazorView<AreaRegistration>("Currency/Index.cshtml"));
        }
    }
}