﻿using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Frapid.Account.DAL;
using Frapid.Account.DTO;
using Frapid.Account.InputModels;
using Frapid.ApplicationState.Cache;
using Frapid.Areas;
using Frapid.Configuration;
using Frapid.TokenManager;

namespace Frapid.Account.Controllers
{
    public class BaseAuthenticationController : FrapidController
    {
        protected ActionResult OnAuthenticated(LoginResult result, SignInInfo model = null)
        {
            if (!result.Status)
            {
                Thread.Sleep(new Random().Next(1, 5)*1000);
                return Json(result);
            }

            Guid? applicationId = null;

            if (model != null)
            {
                applicationId = model.ApplicationId;
            }

            var manager = new Provider(AppUsers.GetCatalog(), applicationId, result.LoginId);
            var token = manager.GetToken();

            AccessTokens.Save(token, this.RemoteUser.IpAddress, this.RemoteUser.UserAgent);

            var cookie = new HttpCookie("access_token")
            {
                Domain = DbConvention.GetDomain(),
                Value = token.ClientToken,
                HttpOnly = true,
                Secure = true
            };

            Response.Cookies.Add(cookie);
            return Json(token.ClientToken);
        }
    }
}