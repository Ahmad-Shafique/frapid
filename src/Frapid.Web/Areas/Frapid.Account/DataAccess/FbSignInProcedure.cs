// ReSharper disable All
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Frapid.Account.Entities;
using Frapid.DataAccess;
using Frapid.DataAccess.Models;
using Frapid.DbPolicy;
using Frapid.Framework.Extensions;
namespace Frapid.Account.DataAccess
{
    /// <summary>
    /// Prepares, validates, and executes the function "account.fb_sign_in(_fb_user_id text, _email text, _office_id integer, _name text, _token text, _browser text, _ip_address text, _culture text)" on the database.
    /// </summary>
    public class FbSignInProcedure : DbAccess, IFbSignInRepository
    {
        /// <summary>
        /// The schema of this PostgreSQL function.
        /// </summary>
        public override string _ObjectNamespace => "account";
        /// <summary>
        /// The schema unqualified name of this PostgreSQL function.
        /// </summary>
        public override string _ObjectName => "fb_sign_in";
        /// <summary>
        /// Login id of application user accessing this PostgreSQL function.
        /// </summary>
        public long _LoginId { get; set; }
        /// <summary>
        /// User id of application user accessing this table.
        /// </summary>
        public int _UserId { get; set; }
        /// <summary>
        /// The name of the database on which queries are being executed to.
        /// </summary>
        public string _Catalog { get; set; }

        /// <summary>
        /// Maps to "_fb_user_id" argument of the function "account.fb_sign_in".
        /// </summary>
        public string FbUserId { get; set; }
        /// <summary>
        /// Maps to "_email" argument of the function "account.fb_sign_in".
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Maps to "_office_id" argument of the function "account.fb_sign_in".
        /// </summary>
        public int OfficeId { get; set; }
        /// <summary>
        /// Maps to "_name" argument of the function "account.fb_sign_in".
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Maps to "_token" argument of the function "account.fb_sign_in".
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Maps to "_browser" argument of the function "account.fb_sign_in".
        /// </summary>
        public string Browser { get; set; }
        /// <summary>
        /// Maps to "_ip_address" argument of the function "account.fb_sign_in".
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// Maps to "_culture" argument of the function "account.fb_sign_in".
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Prepares, validates, and executes the function "account.fb_sign_in(_fb_user_id text, _email text, _office_id integer, _name text, _token text, _browser text, _ip_address text, _culture text)" on the database.
        /// </summary>
        public FbSignInProcedure()
        {
        }

        /// <summary>
        /// Prepares, validates, and executes the function "account.fb_sign_in(_fb_user_id text, _email text, _office_id integer, _name text, _token text, _browser text, _ip_address text, _culture text)" on the database.
        /// </summary>
        /// <param name="fbUserId">Enter argument value for "_fb_user_id" parameter of the function "account.fb_sign_in".</param>
        /// <param name="email">Enter argument value for "_email" parameter of the function "account.fb_sign_in".</param>
        /// <param name="officeId">Enter argument value for "_office_id" parameter of the function "account.fb_sign_in".</param>
        /// <param name="name">Enter argument value for "_name" parameter of the function "account.fb_sign_in".</param>
        /// <param name="token">Enter argument value for "_token" parameter of the function "account.fb_sign_in".</param>
        /// <param name="browser">Enter argument value for "_browser" parameter of the function "account.fb_sign_in".</param>
        /// <param name="ipAddress">Enter argument value for "_ip_address" parameter of the function "account.fb_sign_in".</param>
        /// <param name="culture">Enter argument value for "_culture" parameter of the function "account.fb_sign_in".</param>
        public FbSignInProcedure(string fbUserId, string email, int officeId, string name, string token, string browser, string ipAddress, string culture)
        {
            this.FbUserId = fbUserId;
            this.Email = email;
            this.OfficeId = officeId;
            this.Name = name;
            this.Token = token;
            this.Browser = browser;
            this.IpAddress = ipAddress;
            this.Culture = culture;
        }
        /// <summary>
        /// Prepares and executes the function "account.fb_sign_in".
        /// </summary>
        /// <exception cref="UnauthorizedException">Thown when the application user does not have sufficient privilege to perform this action.</exception>
        public IEnumerable<DbFbSignInResult> Execute()
        {
            if (!this.SkipValidation)
            {
                if (!this.Validated)
                {
                    this.Validate(AccessTypeEnum.Execute, this._LoginId, this._Catalog, false);
                }
                if (!this.HasAccess)
                {
                    Log.Information("Access to the function \"FbSignInProcedure\" was denied to the user with Login ID {LoginId}.", this._LoginId);
                    throw new UnauthorizedException("Access is denied.");
                }
            }
            string query = "SELECT * FROM account.fb_sign_in(@FbUserId, @Email, @OfficeId, @Name, @Token, @Browser, @IpAddress, @Culture);";

            query = query.ReplaceWholeWord("@FbUserId", "@0::text");
            query = query.ReplaceWholeWord("@Email", "@1::text");
            query = query.ReplaceWholeWord("@OfficeId", "@2::integer");
            query = query.ReplaceWholeWord("@Name", "@3::text");
            query = query.ReplaceWholeWord("@Token", "@4::text");
            query = query.ReplaceWholeWord("@Browser", "@5::text");
            query = query.ReplaceWholeWord("@IpAddress", "@6::text");
            query = query.ReplaceWholeWord("@Culture", "@7::text");


            List<object> parameters = new List<object>();
            parameters.Add(this.FbUserId);
            parameters.Add(this.Email);
            parameters.Add(this.OfficeId);
            parameters.Add(this.Name);
            parameters.Add(this.Token);
            parameters.Add(this.Browser);
            parameters.Add(this.IpAddress);
            parameters.Add(this.Culture);

            return Factory.Get<DbFbSignInResult>(this._Catalog, query, parameters.ToArray());
        }


    }
}