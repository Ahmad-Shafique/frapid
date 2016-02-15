using System.Web.Mvc;
using Frapid.Areas.Authorization;
using Frapid.Dashboard.Controllers;

namespace Frapid.WebsiteBuilder.Controllers.Backend
{
    public class ContactController : DashboardController
    {
        [Route("dashboard/website/contacts")]
        [RestrictAnonymous]
        public ActionResult Index()
        {
            return this.FrapidView(this.GetRazorView<AreaRegistration>("Contact/Index.cshtml"));
        }
    }
}