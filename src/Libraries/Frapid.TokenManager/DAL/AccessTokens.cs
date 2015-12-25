﻿using Frapid.Configuration;
using Frapid.DataAccess;

namespace Frapid.TokenManager.DAL
{
    public class AccessTokens
    {
        public static bool IsValid(string clientToken, string ipAddress, string userAgent)
        {
            const string sql = "SELECT * FROM account.is_valid_client_token(@0, @1, @2);";
            return Factory.Scalar<bool>(DbConvention.GetCatalog(), sql, clientToken, ipAddress, userAgent);
        }
    }
}