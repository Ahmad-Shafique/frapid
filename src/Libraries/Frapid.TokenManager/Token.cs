﻿using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Frapid.TokenManager
{
    public class Token
    {
        private string _audience;
        private DateTime _createdOn;
        private DateTime _expiresOn;
        private string _issuedBy;
        private long _loginId;
        private int _officeId;
        private string _subject;
        private string _tokenId;
        private int _userId;

        public Token()
        {
            this.Header = new Dictionary<string, object>();
            this.Claims = new List<Claim>();
        }

        public void AddHeader(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            this.Header.Add(key, value);
        }

        public void AddClaim(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var claim = new Claim(key, value.ToString());

            this.Claims.Add(claim);
        }

        public void AddClaim(Claim claim)
        {
            this.Claims.Add(claim);
        }

        #region Properties

        public Dictionary<string, object> Header { get; }
        public List<Claim> Claims { get; }
        public string ClientToken { get; set; }

        public DateTime CreatedOn
        {
            get { return this._createdOn; }
            set
            {
                this._createdOn = value;
                this.AddClaim("iat", value.Ticks);
            }
        }

        public DateTime ExpiresOn
        {
            get { return this._expiresOn; }
            set
            {
                this._expiresOn = value;
                this.AddClaim("exp", value.Ticks);
            }
        }

        public string Audience
        {
            get { return this._audience; }
            set
            {
                this._audience = value;
                this.AddClaim("aud", value);
            }
        }

        public string Subject
        {
            get { return this._subject; }
            set
            {
                this._subject = value;
                this.AddClaim("sub", value);
            }
        }

        public string IssuedBy
        {
            get { return this._issuedBy; }
            set
            {
                this._issuedBy = value;
                this.AddClaim("iss", value);
            }
        }

        public string TokenId
        {
            get { return this._tokenId; }
            set
            {
                this._tokenId = value;
                this.AddClaim("jti", value);
            }
        }

        public long LoginId
        {
            get { return this._loginId; }
            set
            {
                this._loginId = value;
                this.AddClaim("loginid", value);
            }
        }

        public Guid? ApplicationId { get; set; }

        public int UserId
        {
            get { return this._userId; }
            set
            {
                this._userId = value;
                this.AddClaim("userid", value);
            }
        }

        public int OfficeId
        {
            get { return this._officeId; }
            set
            {
                this._officeId = value;
                this.AddClaim("officeid", value);
            }
        }

        #endregion
    }
}