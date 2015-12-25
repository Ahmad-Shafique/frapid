﻿using System;
using System.Collections.Generic;
using System.Text;
using Frapid.Framework.Extensions;
using Frapid.TokenManager.DTO;
using Jose;
using Newtonsoft.Json;

namespace Frapid.TokenManager
{
    public class Provider
    {
        public Provider(string catalog) : this(catalog, null, long.MinValue)
        {
        }

        public Provider(string catalog, Guid? applicationId, long loginId)
        {
            this.ApplicationId = applicationId;
            this.LoginId = loginId;
            this.Catalog = catalog;
            this.TokenIssuerName = TokenConfig.TokenIssuerName;
            this.TokenValidHours = TokenConfig.TokenValidHours;
            this.Algorithm = TokenConfig.Algorithm;
            this.Key = Encoding.UTF8.GetBytes(TokenConfig.PrivateKey);
        }

        public Guid? ApplicationId { get; set; }
        public string Catalog { get; set; }
        public long LoginId { get; set; }
        public string TokenIssuerName { get; set; }
        public int TokenValidHours { get; set; }
        public JwsAlgorithm Algorithm { get; set; }
        public byte[] Key { get; set; }

        public Token GetToken()
        {
            var token = new Token();

            token.AddHeader("typ", "JWT");

            token.IssuedBy = this.TokenIssuerName;
            token.Audience = this.Catalog;
            token.CreatedOn = DateTime.UtcNow;
            token.ExpiresOn = DateTime.UtcNow.AddHours(this.TokenValidHours);
            token.Subject = this.Catalog;
            token.TokenId = this.Catalog + this.LoginId;
            token.LoginId = this.LoginId;
            token.ApplicationId = this.ApplicationId;
            token.ClientToken = this.Encode(token);

            return token;
        }

        public Token GetToken(string clientToken)
        {
            if (string.IsNullOrWhiteSpace(clientToken))
            {
                return null;
            }

            var token = this.Decode(clientToken);
            return token;
        }

        private string Encode(Token token)
        {
            return JWT.Encode(token.Claims, this.Key, this.Algorithm, token.Header);
        }

        public Token Decode(string clientToken)
        {
            var token = new Token();
            string decoded = JWT.Decode(clientToken, this.Key);
            var dto = JsonConvert.DeserializeObject<List<Claim>>(decoded);
            token.ClientToken = clientToken;

            foreach (var c in dto)
            {
                switch (c.Type)
                {
                    case "aud":
                        token.Audience = c.Value;
                        break;
                    case "iat":
                        token.CreatedOn = new DateTime(c.Value.To<long>(), DateTimeKind.Utc);
                        break;
                    case "exp":
                        token.ExpiresOn = new DateTime(c.Value.To<long>(), DateTimeKind.Utc);
                        break;
                    case "sub":
                        token.Subject = c.Value;
                        break;
                    case "jti":
                        token.TokenId = c.Value;
                        break;
                    case "iss":
                        token.IssuedBy = c.Value;
                        break;
                    case "loginid":
                        token.LoginId = c.Value.To<long>();
                        break;
                    case "userid":
                        token.UserId = c.Value.To<int>();
                        break;
                    case "officeid":
                        token.OfficeId = c.Value.To<int>();
                        break;
                }
            }

            return token;
        }
    }
}