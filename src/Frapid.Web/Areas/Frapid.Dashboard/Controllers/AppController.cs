﻿using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Frapid.Areas;
using Frapid.Areas.Authorization;
using Frapid.Dashboard.DAL;
using Frapid.i18n;

namespace Frapid.Dashboard.Controllers
{
    public class AppController : FrapidController
    {
        [Route("dashboard/my/apps")]
        [RestrictAnonymous]
        public async Task<ActionResult> GetAppsAsync()
        {
            int userId = this.AppUser.UserId;
            int officeId = this.AppUser.OfficeId;

            string culture = CultureManager.GetCurrent().TwoLetterISOLanguageName;
            var awaiter = await App.GetAsync(this.Tenant, userId, officeId, culture).ConfigureAwait(false);
            var apps = awaiter.OrderBy(x => x.AppId);

            return this.Ok(apps);
        }
    }
}