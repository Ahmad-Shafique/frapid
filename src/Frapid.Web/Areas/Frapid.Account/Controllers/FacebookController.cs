using System.Threading.Tasks;
using System.Web.Mvc;
using Frapid.Account.InputModels;
using Frapid.Account.RemoteAuthentication;
using Frapid.Areas;
using Npgsql;

namespace Frapid.Account.Controllers
{
    [AntiForgery]
    public class FacebookController : BaseAuthenticationController
    {
        [Route("account/facebook/sign-in")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> FacebookSignInAsync(FacebookAccount account)
        {
            var auth = new FacebookAuthentication();
            try
            {
                var result =
                    await auth.AuthenticateAsync(account, this.RemoteUser);
                return this.OnAuthenticated(result);
            }
            catch (NpgsqlException)
            {
                return this.Json("Access is denied.");
            }
        }
    }
}