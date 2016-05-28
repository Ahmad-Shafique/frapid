﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Frapid.DataAccess;
using Frapid.DataAccess.Models;
using Frapid.Framework;
using Frapid.WebApi.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Frapid.WebApi.Service
{
    public class FormApiController: FrapidApiController
    {
        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/meta")]
        [RestAuthorize]
        public async Task<EntityView> GetEntityViewAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetMetaAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/count")]
        [RestAuthorize]
        public async Task<long> CountAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.CountAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/all")]
        [Route("~/api/forms/{schemaName}/{tableName}/export")]
        [RestAuthorize]
        public async Task<IEnumerable<dynamic>> GetAllAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetAllAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/{primaryKey}")]
        [RestAuthorize]
        public async Task<dynamic> GetAsync(string schemaName, string tableName, string primaryKey)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetAsync(primaryKey);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/get")]
        [RestAuthorize]
        public async Task<IEnumerable<dynamic>> GetAsync(string schemaName, string tableName, [FromUri] object[] primaryKeys)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetAsync(primaryKeys);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/first")]
        [RestAuthorize]
        public async Task<dynamic> GetFirstAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetFirstAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/previous/{primaryKey}")]
        [RestAuthorize]
        public async Task<dynamic> GetPreviousAsync(string schemaName, string tableName, string primaryKey)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetPreviousAsync(primaryKey);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/next/{primaryKey}")]
        [RestAuthorize]
        public async Task<dynamic> GetNextAsync(string schemaName, string tableName, string primaryKey)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetNextAsync(primaryKey);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/last")]
        [RestAuthorize]
        public async Task<dynamic> GetLastAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetLastAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}")]
        [RestAuthorize]
        public async Task<IEnumerable<dynamic>> GetPaginatedResultAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetPaginatedResultAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/page/{pageNumber}")]
        [RestAuthorize]
        public async Task<IEnumerable<dynamic>> GetPaginatedResultAsync(string schemaName, string tableName, long pageNumber)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetPaginatedResultAsync(pageNumber);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("POST")]
        [Route("~/api/forms/{schemaName}/{tableName}/count-where")]
        [RestAuthorize]
        public async Task<long> CountWhereAsync(string schemaName, string tableName, [FromBody] JArray filters)
        {
            try
            {
                var f = Filter.FromJArray(filters);
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.CountWhereAsync(f);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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


        [AcceptVerbs("POST")]
        [Route("~/api/forms/{schemaName}/{tableName}/get-where/{pageNumber}")]
        [RestAuthorize]
        public async Task<IEnumerable<dynamic>> GetWhereAsync(string schemaName, string tableName, long pageNumber, [FromBody] JArray filters)
        {
            try
            {
                var f = Filter.FromJArray(filters);
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetWhereAsync(pageNumber, f);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/count-filtered/{filterName}")]
        [RestAuthorize]
        public async Task<long> CountFilteredAsync(string schemaName, string tableName, string filterName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.CountFilteredAsync(filterName);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/get-filtered/{pageNumber}/{filterName}")]
        [RestAuthorize]
        public async Task<IEnumerable<dynamic>> GetFilteredAsync(string schemaName, string tableName, long pageNumber, string filterName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetFilteredAsync(pageNumber, filterName);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/display-fields")]
        [RestAuthorize]
        public async Task<IEnumerable<DisplayField>> GetDisplayFieldsAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetDisplayFieldsAsync();
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/custom-fields")]
        [RestAuthorize]
        public async Task<IEnumerable<CustomField>> GetCustomFieldsAsync(string schemaName, string tableName)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetCustomFieldsAsync(null);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("GET", "HEAD")]
        [Route("~/api/forms/{schemaName}/{tableName}/custom-fields/{resourceId}")]
        [RestAuthorize]
        public async Task<IEnumerable<CustomField>> GetCustomFieldsAsync(string schemaName, string tableName, string resourceId)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.GetCustomFieldsAsync(resourceId);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("POST")]
        [Route("~/api/forms/{schemaName}/{tableName}/add-or-edit")]
        [RestAuthorize]
        public async Task<object> AddOrEditAsync(string schemaName, string tableName, [FromBody] JArray form)
        {
            var item = form[0].ToObject<Dictionary<string, object>>();
            var customFields = form[1].ToObject<List<CustomField>>(JsonHelper.GetJsonSerializer());

            if(item == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
            }

            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.AddOrEditAsync(item, customFields);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("POST")]
        [Route("~/api/forms/{schemaName}/{tableName}/add")]
        [Route("~/api/forms/{schemaName}/{tableName}/add/{skipPrimaryKey:bool}")]
        [RestAuthorize]
        public async Task AddAsync(string schemaName, string tableName, [FromBody] JArray form, bool skipPrimaryKey = true)
        {
            var item = form[0].ToObject<Dictionary<string, object>>();
            var customFields = form[1].ToObject<List<CustomField>>(JsonHelper.GetJsonSerializer());

            if(item == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
            }

            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                await repository.AddAsync(item, customFields, skipPrimaryKey);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("PUT")]
        [Route("~/api/forms/{schemaName}/{tableName}/edit/{primaryKey}")]
        [RestAuthorize]
        public async Task EditAsync(string schemaName, string tableName, string primaryKey, [FromBody] JArray form)
        {
            var item = form[0].ToObject<Dictionary<string, object>>();
            var customFields = form[1].ToObject<List<CustomField>>(JsonHelper.GetJsonSerializer());

            if(item == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
            }

            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                await repository.UpdateAsync(item, primaryKey, customFields);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        private List<Dictionary<string, object>> ParseCollection(JArray collection)
        {
            return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(collection.ToString(), JsonHelper.GetJsonSerializerSettings());
        }

        [AcceptVerbs("POST")]
        [Route("~/api/forms/{schemaName}/{tableName}/bulk-import")]
        [RestAuthorize]
        public async Task<List<object>> BulkImportAsync(string schemaName, string tableName, [FromBody] JArray collection)
        {
            var items = this.ParseCollection(collection);

            if(items == null ||
               items.Count.Equals(0))
            {
                return null;
            }

            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                return await repository.BulkImportAsync(items);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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

        [AcceptVerbs("DELETE")]
        [Route("~/api/forms/{schemaName}/{tableName}/delete/{primaryKey}")]
        [RestAuthorize]
        public async Task DeleteAsync(string schemaName, string tableName, string primaryKey)
        {
            try
            {
                var repository = new FormRepository(schemaName, tableName, this.MetaUser.Tenant, this.MetaUser.LoginId, this.MetaUser.UserId);
                await repository.DeleteAsync(primaryKey);
            }
            catch(UnauthorizedException)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch(DataAccessException ex)
            {
                throw new HttpResponseException
                    (
                    new HttpResponseMessage
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