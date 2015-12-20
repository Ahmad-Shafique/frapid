﻿using System.Linq;
using Frapid.ApplicationState.Models;
using Frapid.DataAccess;

namespace Frapid.ApplicationState.Cache
{
    public static class MetaLoginHelper
    {
        /// <summary>
        ///     Creates meta login table if not already present in the meta database.
        /// </summary>
        public static void CreateTable()
        {
            const string sql = @"DO
                                $$
                                BEGIN
                                    IF NOT EXISTS (
                                        SELECT 1 
                                        FROM   pg_catalog.pg_class c
                                        JOIN   pg_catalog.pg_namespace n ON n.oid = c.relnamespace
                                        WHERE  n.nspname = 'public'
                                        AND    c.relname = 'frapid_logins'
                                        AND    c.relkind = 'r'
                                    ) THEN
                                        CREATE TABLE public.frapid_logins
                                        (
                                            global_login_id         BIGSERIAL NOT NULL PRIMARY KEY,
                                            catalog                 text NOT NULL,
                                            login_id                bigint NOT NULL
                                        );
                                    END IF;
                                END
                                $$
                                LANGUAGE plpgsql;";

            Factory.NonQuery(Factory.MetaDatabase, sql);
        }

        /// <summary>
        ///     Returns meta login table against the supplied global login identifier.
        /// </summary>
        /// <param name="globalLoginId">The unique global login identifier set for each login.</param>
        /// <returns></returns>
        public static MetaLogin Get(long globalLoginId)
        {
            const string sql = "SELECT * FROM public.frapid_logins WHERE global_login_id=@0;";
            return Factory.Get<MetaLogin>(Factory.MetaDatabase, sql, globalLoginId).FirstOrDefault();
        }
    }
}