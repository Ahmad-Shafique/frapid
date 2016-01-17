﻿using System.Web.Mvc;
using Frapid.ApplicationState.Cache;
using Frapid.Areas;
using Frapid.Dashboard.DAL;
using Frapid.i18n;

namespace Frapid.Dashboard.Controllers
{
    public class AppController : FrapidController
    {
        [Route("dashboard/my/apps")]
        [RestrictAnonymous]
        public ActionResult GetApps()
        {
            int userId = AppUsers.GetCurrent().UserId;
            int officeId = AppUsers.GetCurrent().OfficeId;
            string culture = CultureManager.GetCurrent().TwoLetterISOLanguageName;

            return Json(App.Get(userId, officeId, culture), JsonRequestBehavior.AllowGet);
        }
    }
}