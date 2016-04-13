﻿using System.Collections.Generic;
using Frapid.Configuration;
using Frapid.Configuration.Db;
using Frapid.NPoco;

namespace Frapid.DataAccess
{
    public static class Factory
    {
        public static string GetProviderName(string tenant)
        {
            return DbProvider.GetProviderName(tenant);
        }

        public static string GetMetaDatabase(string tenant)
        {
            return DbProvider.GetMetaDatabase(tenant);
        }

        public static T Single<T>(string database, string sql, params object[] args)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                return db.Single<T>(sql, args);
            }
        }

        public static IEnumerable<T> Get<T>(string database, string sql, params object[] args)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                return db.Query<T>(sql, args);
            }
        }

        public static IEnumerable<T> Get<T>(string database, string sql)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                return db.Query<T>(sql);
            }
        }

        public static IEnumerable<T> Get<T>(string database, Sql sql)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                var retVal = db.Query<T>(sql);
                return retVal;
            }
        }

        public static object Insert(string database, object poco, string tableName = "", string primaryKeyName = "", bool autoIncrement = true)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                if (!string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrWhiteSpace(primaryKeyName))
                {
                    return db.Insert(tableName, primaryKeyName, autoIncrement, poco);
                }

                return db.Insert(poco);
            }
        }

        public static object Update(string database, object poco, object primaryKeyValue, string tableName = "",
            string primaryKeyName = "")
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                if (!string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrWhiteSpace(primaryKeyName))
                {
                    return db.Update(tableName, primaryKeyName, poco, primaryKeyValue);
                }

                return db.Update(poco, primaryKeyValue);
            }
        }

        public static T Scalar<T>(string database, string sql, params object[] args)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                return db.ExecuteScalar<T>(sql, args);
            }
        }

        public static T Scalar<T>(string database, Sql sql)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                return db.ExecuteScalar<T>(sql);
            }
        }

        public static void NonQuery(string database, string sql, params object[] args)
        {
            using (var db = DbProvider.Get(FrapidDbServer.GetConnectionString(database), database).GetDatabase())
            {
                db.Execute(sql, args);
            }
        }

        public static void Execute(string connectionString, string tenant, string sql, params object[] args)
        {
            using (var db = DbProvider.Get(connectionString, tenant).GetDatabase())
            {
                db.Execute(sql, args);
            }
        }

        public static T ExecuteScalar<T>(string connectionString, string tenant, Sql sql)
        {
            using (var db = DbProvider.Get(connectionString, tenant).GetDatabase())
            {
                return db.ExecuteScalar<T>(sql);
            }
        }
    }
}