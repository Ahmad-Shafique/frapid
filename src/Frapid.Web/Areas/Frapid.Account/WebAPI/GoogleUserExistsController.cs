// ReSharper disable All
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Frapid.ApplicationState.Cache;
using Frapid.ApplicationState.Models;
using Frapid.DataAccess;
using Frapid.DataAccess.Models;
using Frapid.Framework;
using Frapid.Framework.Extensions;
using Frapid.WebApi;
using Frapid.Account.Entities;
using Frapid.Account.DataAccess;
namespace Frapid.Account.Api
{
    /// <summary>
    /// Provides a direct HTTP access to execute the function GoogleUserExists.
    /// </summary>
    [RoutePrefix("api/v1.0/account/procedures/google-user-exists")]
    public class GoogleUserExistsController : FrapidApiController
    {
        /// <summary>
        ///     The GoogleUserExists repository.
        /// </summary>
        private IGoogleUserExistsRepository repository;

        public class Annotation
        {
            public int UserId { get; set; }
        }


        public GoogleUserExistsController()
        {
        }

        public GoogleUserExistsController(IGoogleUserExistsRepository repository)
        {
            this.repository = repository;
        }

        protected override void Initialize(HttpControllerContext context)
        {
            base.Initialize(context);
            if (this.repository == null)
            {
                this.repository = new GoogleUserExistsProcedure
                {
                    _Catalog = this.MetaUser.Catalog,
                    _LoginId = this.MetaUser.LoginId,
                    _UserId = this.MetaUser.UserId
                };
            }
        }
        /// <summary>
        ///     Creates meta information of "google user exists" annotation.
        /// </summary>
        /// <returns>Returns the "google user exists" annotation meta information to perform CRUD operation.</returns>
        [AcceptVerbs("GET", "HEAD")]
        [Route("annotation")]
        [Route("~/api/account/procedures/google-user-exists/annotation")]
        [RestAuthorize]
        public EntityView GetAnnotation()
        {
            return new EntityView
            {
                Columns = new List<EntityColumn>()
                {
                        new EntityColumn
                        {
                                ColumnName = "_user_id",
                                PropertyName = "UserId",
                                DataType = "int",
                                DbDataType = "integer",
                                IsNullable = false,
                                IsPrimaryKey = false,
                                IsSerial = false,
                                Value = "",
                                MaxLength = 0
                        }
                }
            };
        }




        [AcceptVerbs("POST")]
        [Route("execute")]
        [Route("~/api/account/procedures/google-user-exists/execute")]
        [RestAuthorize]
        public bool Execute([FromBody] Annotation annotation)
        {
            try
            {
                this.repository.UserId = annotation.UserId;


                return this.repository.Execute();
            }
            catch (UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (DataAccessException ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    Content = new StringContent(ex.Message),
                    StatusCode = HttpStatusCode.InternalServerError
                });
            }
#if !DEBUG
            catch
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }
#endif
        }
    }
}