﻿-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/00.db core/casts.sql --<--<--
DROP FUNCTION IF EXISTS text_to_bigint(text) CASCADE;
CREATE FUNCTION text_to_bigint(text) RETURNS bigint AS 'SELECT int8in(textout($1));' LANGUAGE SQL STRICT IMMUTABLE;
CREATE CAST (text AS bigint) WITH FUNCTION text_to_bigint(text) AS IMPLICIT;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/00.db core/extensions.sql --<--<--
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/00.db core/postgresql-roles.sql --<--<--
DO
$$
BEGIN
    IF NOT EXISTS (SELECT * FROM pg_catalog.pg_roles WHERE rolname = 'frapid_db_user') THEN
        CREATE ROLE frapid_db_user WITH LOGIN PASSWORD 'change-on-deployment@123';
    END IF;

    COMMENT ON ROLE frapid_db_user IS 'The default user for frapid databases.';

    EXECUTE 'ALTER DATABASE ' || current_database() || ' OWNER TO frapid_db_user;';
END
$$
LANGUAGE plpgsql;

DO
$$
BEGIN
    IF NOT EXISTS (SELECT * FROM pg_catalog.pg_roles WHERE rolname = 'report_user') THEN
        CREATE ROLE report_user WITH LOGIN PASSWORD 'change-on-deployment@123';
    END IF;

    COMMENT ON ROLE report_user IS 'This user account is used by the Reporting Engine to run ad-hoc queries. It is strictly advised for this user to only have a read-only access to the database.';
END
$$
LANGUAGE plpgsql;


-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/01.poco.sql --<--<--
DROP FUNCTION IF EXISTS public.text_to_int_array
(
    _input                              text,
    _remove_nulls                       boolean
);

CREATE FUNCTION public.text_to_int_array
(
    _input                              text,
    _remove_nulls                       boolean = true
)
RETURNS integer[]
STABLE
AS
$$
    DECLARE _int_array                  integer[];
BEGIN
    WITH items
    AS
    (
        SELECT
            item, 
            item ~ '^[0-9]+$' AS is_number
        FROM 
            unnest(regexp_split_to_array(_input, ',')) AS item
    )
    SELECT
        array_agg(item)
    INTO
        _int_array
    FROM items
    WHERE is_number;

    RETURN _int_array;   
END
$$
LANGUAGE plpgsql;


DROP FUNCTION IF EXISTS public.poco_get_table_function_annotation
(
    _schema_name            text,
    _table_name             text
);

CREATE FUNCTION public.poco_get_table_function_annotation
(
    _schema_name            text,
    _table_name             text
)
RETURNS TABLE
(
    id                      integer, 
    column_name             text, 
    nullable                text, 
    udt_name                text, 
    column_default          text, 
    max_length              integer, 
    primary_key             text
) AS
$$
    DECLARE _args           text;
BEGIN
    DROP TABLE IF EXISTS temp_annonation;

    CREATE TEMPORARY TABLE temp_annonation
    (
        id                      SERIAL,
        column_name             text,
        is_nullable             text DEFAULT('NO'),
        udt_name                text,
        column_default          text,
        max_length              integer DEFAULT(0),
        is_primary_key          text DEFAULT('NO')
    ) ON COMMIT DROP;


    SELECT
        pg_catalog.pg_get_function_arguments(pg_proc.oid) AS arguments
    INTO
        _args
    FROM pg_proc
    INNER JOIN pg_namespace
    ON pg_proc.pronamespace = pg_namespace.oid
    INNER JOIN pg_type
    ON pg_proc.prorettype = pg_type.oid
    INNER JOIN pg_namespace type_namespace
    ON pg_type.typnamespace = type_namespace.oid
    WHERE typname != ANY(ARRAY['trigger'])
    AND pg_namespace.nspname = _schema_name
    AND proname::text = _table_name;

    INSERT INTO temp_annonation(column_name, udt_name)
    SELECT split_part(trim(unnest(regexp_split_to_array(_args, ','))), ' ', 1), trim(unnest(regexp_split_to_array(_args, ',')));

    UPDATE temp_annonation
    SET udt_name = TRIM(REPLACE(temp_annonation.udt_name, temp_annonation.column_name, ''));

    
    RETURN QUERY
    SELECT * FROM temp_annonation;
END
$$
LANGUAGE plpgsql;

  
DROP FUNCTION IF EXISTS get_app_data_type(_db_data_type text);

CREATE FUNCTION get_app_data_type(_db_data_type text)
RETURNS text
STABLE
AS
$$
BEGIN
    IF(_db_data_type IN('int4', 'int', 'integer')) THEN
        RETURN 'int';
    END IF;

    IF(_db_data_type IN('varchar', 'character varying', 'text')) THEN
        RETURN 'string';
    END IF;
    
    IF(_db_data_type IN('date', 'time', 'timestamp', 'timestamptz')) THEN
        RETURN 'System.DateTime';
    END IF;
    
    IF(_db_data_type IN('bool', 'boolean')) THEN
        RETURN 'bool';
    END IF;

    RETURN $1;
END
$$
LANGUAGE plpgsql;


DROP FUNCTION IF EXISTS public.poco_get_table_function_definition(_schema text, _name text);

CREATE FUNCTION public.poco_get_table_function_definition(_schema text, _name text)
RETURNS TABLE
(
    id                      bigint,
    column_name             text,
    nullable                text,
    udt_name                text,
    column_default          text,
    max_length              integer,
    primary_key             text,
    data_type               text
)
AS
$$
    DECLARE _oid            oid;
    DECLARE _typoid         oid;
BEGIN
    CREATE TEMPORARY TABLE temp_poco
    (
        id                      bigint,
        column_name             text,
        is_nullable             text,
        udt_name                text,
        column_default          text,
        max_length              integer default(0),
        is_primary_key          text,
        data_type               text
    ) ON COMMIT DROP;

    SELECT        
        pg_proc.oid,
        pg_proc.prorettype
    INTO 
        _oid,
        _typoid
    FROM pg_proc
    INNER JOIN pg_namespace
    ON pg_proc.pronamespace = pg_namespace.oid
    WHERE pg_proc.proname=_name
    AND pg_namespace.nspname=_schema
    LIMIT 1;


    IF EXISTS
    (
        SELECT 1
        FROM information_schema.columns 
        WHERE table_schema=_schema 
        AND table_name=_name
    ) THEN
        INSERT INTO temp_poco
        SELECT
            row_number() OVER(ORDER BY attnum),
            attname AS column_name,
            CASE WHEN attnotnull THEN 'NO' ELSE 'YES' END,
            pg_type.typname,
            public.parse_default(adsrc),
            CASE WHEN atttypmod <> -1 
            THEN atttypmod - 4
            ELSE 0 END,
            CASE WHEN indisprimary THEN 'YES' ELSE 'NO' END
        FROM   pg_attribute
        LEFT JOIN pg_index
        ON pg_attribute.ATTRELID = pg_index.indrelid
        AND pg_attribute.attnum = ANY(pg_index.indkey)
        AND pg_index.indisprimary
        INNER JOIN pg_type
        ON pg_attribute.atttypid = pg_type.oid
        LEFT   JOIN pg_catalog.pg_attrdef
        ON (pg_attribute.attrelid, pg_attribute.attnum) = (pg_attrdef.adrelid,  pg_attrdef.adnum)
        WHERE  attrelid = (_schema || '.' || _name)::regclass
        AND    attnum > 0
        AND    NOT attisdropped
        ORDER  BY attnum;

        UPDATE temp_poco
        SET data_type = public.get_app_data_type(temp_poco.udt_name);
        
        RETURN QUERY
        SELECT * FROM temp_poco;
        RETURN;
    END IF;

    IF EXISTS(SELECT * FROM pg_type WHERE oid = _typoid AND typtype='c') THEN
        --Composite Type
        INSERT INTO temp_poco
        SELECT
            row_number() OVER(ORDER BY attnum),
            attname::text               AS column_name,
            'NO'::text                  AS is_nullable, 
            format_type(t.oid,NULL)     AS udt_name,
            ''::text                    AS column_default
        FROM pg_attribute att
        JOIN pg_type t ON t.oid=atttypid
        JOIN pg_namespace nsp ON t.typnamespace=nsp.oid
        LEFT OUTER JOIN pg_type b ON t.typelem=b.oid
        LEFT OUTER JOIN pg_collation c ON att.attcollation=c.oid
        LEFT OUTER JOIN pg_namespace nspc ON c.collnamespace=nspc.oid
        WHERE att.attrelid=(SELECT typrelid FROM pg_type WHERE pg_type.oid = _typoid)
        AND att.attnum > 0
        ORDER by attnum;

        UPDATE temp_poco
        SET data_type = public.get_app_data_type(temp_poco.udt_name);

        RETURN QUERY
        SELECT * FROM temp_poco;
        RETURN;
    END IF;

    IF(_oid IS NOT NULL) THEN
        INSERT INTO temp_poco
        WITH procs
        AS
        (
            SELECT 
            row_number() OVER(ORDER BY proallargtypes),
            explode_array(proargnames) as column_name,
            explode_array(proargmodes) as column_mode,
            explode_array(proallargtypes) as argument_type
            FROM pg_proc
            WHERE oid = _oid
        )
        SELECT
            row_number() OVER(ORDER BY 1),
            procs.column_name::text,
            'NO'::text AS is_nullable, 
            format_type(procs.argument_type, null) as udt_name,
            ''::text AS column_default
        FROM procs
        WHERE column_mode=ANY(ARRAY['t', 'o']);

        UPDATE temp_poco
        SET data_type = public.get_app_data_type(temp_poco.udt_name);

        RETURN QUERY
        SELECT * FROM temp_poco;
        RETURN;
    END IF;

    INSERT INTO temp_poco
    SELECT 
        row_number() OVER(ORDER BY attnum),
        attname::text               AS column_name,
        'NO'::text                  AS is_nullable, 
        format_type(t.oid,NULL)     AS udt_name,
        ''::text                    AS column_default
    FROM pg_attribute att
    JOIN pg_type t ON t.oid=atttypid
    JOIN pg_namespace nsp ON t.typnamespace=nsp.oid
    LEFT OUTER JOIN pg_type b ON t.typelem=b.oid
    LEFT OUTER JOIN pg_collation c ON att.attcollation=c.oid
    LEFT OUTER JOIN pg_namespace nspc ON c.collnamespace=nspc.oid
    WHERE att.attrelid=
    (
        SELECT typrelid 
        FROM pg_type
        INNER JOIN pg_namespace
        ON pg_type.typnamespace = pg_namespace.oid
        WHERE typname=_name
        AND pg_namespace.nspname=_schema
    )
    AND att.attnum > 0
    ORDER by attnum;

    UPDATE temp_poco
    SET data_type = public.get_app_data_type(temp_poco.udt_name);

    RETURN QUERY
    SELECT * FROM temp_poco;
END;
$$
LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS public.poco_get_tables(_schema text);

CREATE FUNCTION public.poco_get_tables(_schema text)
RETURNS TABLE
(
    table_schema                name, 
    table_name                  name, 
    table_type                  text, 
    has_duplicate               boolean
) AS
$$
BEGIN
    CREATE TEMPORARY TABLE _t
    (
        table_schema            name,
        table_name              name,
        table_type              text,
        has_duplicate           boolean DEFAULT(false)
    ) ON COMMIT DROP;

    INSERT INTO _t
    SELECT 
        information_schema.tables.table_schema, 
        information_schema.tables.table_name, 
        information_schema.tables.table_type
    FROM information_schema.tables 
    WHERE (information_schema.tables.table_type='BASE TABLE' OR information_schema.tables.table_type='VIEW')
    AND information_schema.tables.table_schema = _schema
    UNION ALL
    SELECT DISTINCT 
        pg_namespace.nspname::text, 
        pg_proc.proname::text, 
        'FUNCTION' AS table_type
    FROM pg_proc
    INNER JOIN pg_namespace
    ON pg_proc.pronamespace = pg_namespace.oid
    INNER JOIN pg_language 
    ON pg_proc.prolang = pg_language .oid
    INNER JOIN pg_type
    ON pg_proc.prorettype=pg_type.oid
    WHERE ('t' = ANY(pg_proc.proargmodes) OR 'o' = ANY(pg_proc.proargmodes) OR pg_type.typtype = 'c')
    AND lanname NOT IN ('c','internal')
    AND nspname=_schema;


    UPDATE _t
    SET has_duplicate = TRUE
    FROM
    (
        SELECT
            information_schema.tables.table_name,
            COUNT(information_schema.tables.table_name) AS table_count
        FROM information_schema.tables
        GROUP BY information_schema.tables.table_name
        
    ) subquery
    WHERE subquery.table_name = _t.table_name
    AND subquery.table_count > 1;

    

    RETURN QUERY
    SELECT * FROM _t;
END
$$
LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS public.parse_default(text);

CREATE OR REPLACE FUNCTION public.parse_default(text)
RETURNS text 
AS
$$
    DECLARE _sql text;
    DECLARE _val text;
BEGIN
    IF($1 NOT LIKE 'nextval%') THEN
        _sql := 'SELECT ' || $1;
        EXECUTE _sql INTO _val;
        RAISE NOTICE '%', _sql;
        RETURN _val;
    END IF;

    IF($1 = 'now()') THEN
        RETURN '';
    END IF;

    RETURN $1;
END
$$
LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS public.explode_array(in_array anyarray);

CREATE FUNCTION public.explode_array(in_array anyarray)
RETURNS SETOF anyelement
IMMUTABLE
AS
$$
    SELECT ($1)[s] FROM generate_series(1,array_upper($1, 1)) AS s;
$$
LANGUAGE sql;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/01.types-domains-tables-and-constraints/domains.sql --<--<--
DROP DOMAIN IF EXISTS public.money_strict CASCADE;
CREATE DOMAIN public.money_strict
AS DECIMAL(24, 4)
CHECK
(
    VALUE > 0
);


DROP DOMAIN IF EXISTS public.money_strict2 CASCADE;
CREATE DOMAIN public.money_strict2
AS DECIMAL(24, 4)
CHECK
(
    VALUE >= 0
);

DROP DOMAIN IF EXISTS public.integer_strict CASCADE;
CREATE DOMAIN public.integer_strict
AS integer
CHECK
(
    VALUE > 0
);

DROP DOMAIN IF EXISTS public.integer_strict2 CASCADE;
CREATE DOMAIN public.integer_strict2
AS integer
CHECK
(
    VALUE >= 0
);

DROP DOMAIN IF EXISTS public.smallint_strict CASCADE;
CREATE DOMAIN public.smallint_strict
AS smallint
CHECK
(
    VALUE > 0
);

DROP DOMAIN IF EXISTS public.smallint_strict2 CASCADE;
CREATE DOMAIN public.smallint_strict2
AS smallint
CHECK
(
    VALUE >= 0
);

DROP DOMAIN IF EXISTS public.decimal_strict CASCADE;
CREATE DOMAIN public.decimal_strict
AS decimal
CHECK
(
    VALUE > 0
);

DROP DOMAIN IF EXISTS public.decimal_strict2 CASCADE;
CREATE DOMAIN public.decimal_strict2
AS decimal
CHECK
(
    VALUE >= 0
);

DROP DOMAIN IF EXISTS public.color CASCADE;
CREATE DOMAIN public.color
AS text;

DROP DOMAIN IF EXISTS public.photo CASCADE;
CREATE DOMAIN public.photo
AS text;


DROP DOMAIN IF EXISTS public.html CASCADE;
CREATE DOMAIN public.html
AS text;

DROP DOMAIN IF EXISTS public.password CASCADE;
CREATE DOMAIN public.password
AS text;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/01.types-domains-tables-and-constraints/tables-and-constraints.sql --<--<--
DROP SCHEMA IF EXISTS core CASCADE;
CREATE SCHEMA core;

CREATE TABLE core.countries
(
    country_code                            	national character varying(12) PRIMARY KEY,
    country_name                            	national character varying(100) NOT NULL,
    audit_user_id                           	integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);

CREATE TABLE core.apps
(
	app_id										SERIAL,
    app_name                                    national character varying(100) PRIMARY KEY,
    name                                        national character varying(100),
    version_number                              national character varying(100),
    publisher                                   national character varying(100),
    published_on                                date,
    icon                                        national character varying(100),
    landing_url                                 text,
    audit_user_id                           	integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);

CREATE UNIQUE INDEX apps_app_name_uix
ON core.apps(UPPER(app_name))
WHERE NOT deleted;

CREATE TABLE core.app_dependencies
(
    app_dependency_id                           SERIAL PRIMARY KEY,
    app_name                                    national character varying(100) REFERENCES core.apps,
    depends_on                                  national character varying(100) REFERENCES core.apps,
    audit_user_id                           	integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);


CREATE TABLE core.menus
(
    menu_id                                     SERIAL PRIMARY KEY,
    sort                                        integer,
    app_name                                    national character varying(100) NOT NULL REFERENCES core.apps,
    menu_name                                   national character varying(100) NOT NULL,
    url                                         text,
    icon                                        national character varying(100),
    parent_menu_id                              integer REFERENCES core.menus,
    audit_user_id                           	integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);

CREATE UNIQUE INDEX menus_app_name_menu_name_uix
ON core.menus(UPPER(app_name), UPPER(menu_name))
WHERE NOT deleted;

CREATE TABLE core.menu_locale
(
    menu_locale_id                              SERIAL PRIMARY KEY,
    menu_id                                     integer NOT NULL REFERENCES core.menus,
    culture                                     national character varying(12) NOT NULL,
    menu_text                                   national character varying(250) NOT NULL,
    audit_user_id                           	integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);

CREATE TABLE core.currencies
(
	currency_id									SERIAL,
    currency_code                           	national character varying(12) PRIMARY KEY,
    currency_symbol                         	national character varying(12) NOT NULL,
    currency_name                           	national character varying(48) NOT NULL UNIQUE,
    hundredth_name                          	national character varying(48) NOT NULL,
    audit_user_id                           	integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);

CREATE TABLE core.offices
(
    office_id                                   SERIAL PRIMARY KEY,
    office_code                                 national character varying(12) NOT NULL,
    office_name                                 national character varying(150) NOT NULL,
    nick_name                                   national character varying(50),
    registration_date                           date,
	currency_code								national character varying(12),
    po_box                                      national character varying(128),
    address_line_1                              national character varying(128),   
    address_line_2                              national character varying(128),
    street                                      national character varying(50),
    city                                        national character varying(50),
    state                                       national character varying(50),
    zip_code                                    national character varying(24),
    country                                     national character varying(50),
    phone                                       national character varying(24),
    fax                                         national character varying(24),
    email                                       national character varying(128),
    url                                         national character varying(50),
    logo                                        public.photo,
    parent_office_id                            integer NULL REFERENCES core.offices,
	registration_number							national character varying(100),
	pan_number									national character varying(50),
	has_vat										boolean NOT NULL DEFAULT(false),
	allow_transaction_posting					boolean NOT NULL DEFAULT(false),
    audit_user_id                               integer,
    audit_ts                                	TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted										boolean DEFAULT(false)
);

CREATE TABLE core.frequencies
(
    frequency_id                            SERIAL PRIMARY KEY,
    frequency_code                          national character varying(12) NOT NULL,
    frequency_name                          national character varying(50) NOT NULL,
    audit_user_id                           integer,
    audit_ts                                TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted									boolean DEFAULT(false)
);


CREATE UNIQUE INDEX frequencies_frequency_code_uix
ON core.frequencies(UPPER(frequency_code))
WHERE NOT deleted;

CREATE UNIQUE INDEX frequencies_frequency_name_uix
ON core.frequencies(UPPER(frequency_name))
WHERE NOT deleted;


CREATE TABLE core.verification_statuses
(
    verification_status_id                  smallint PRIMARY KEY,
    verification_status_name                national character varying(128) NOT NULL,
    audit_user_id                           integer,
    audit_ts                                TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted									boolean DEFAULT(false)
);

COMMENT ON TABLE core.verification_statuses IS 
'Verification statuses are integer values used to represent the state of a transaction.
For example, a verification status of value "0" would mean that the transaction has not yet been verified.
A negative value indicates that the transaction was rejected, whereas a positive value means approved.

Remember:
1. Only approved transactions appear on ledgers and final reports.
2. Cash repository balance is maintained on the basis of LIFO principle. 

   This means that cash balance is affected (reduced) on your repository as soon as a credit transaction is posted,
   without the transaction being approved on the first place. If you reject the transaction, the cash balance then increases.
   This also means that the cash balance is not affected (increased) on your repository as soon as a debit transaction is posted.
   You will need to approve the transaction.

   It should however be noted that the cash repository balance might be less than the total cash shown on your balance sheet,
   if you have pending transactions to verify. You cannot perform EOD operation if you have pending verifications.
';

CREATE TABLE core.week_days
(
	week_day_id                 			integer NOT NULL CHECK(week_day_id >=1 AND week_day_id <=7) PRIMARY KEY,
	week_day_code               			national character varying(12) NOT NULL UNIQUE,
	week_day_name               			national character varying(50) NOT NULL UNIQUE,
    audit_user_id                           integer,
    audit_ts                                TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted									boolean DEFAULT(false)
);

CREATE TABLE core.genders
(
	gender_code                             national character varying(4) NOT NULL PRIMARY KEY,
	gender_name                             national character varying(50) NOT NULL UNIQUE,
	audit_user_id                           integer,
	audit_ts                                TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted									boolean DEFAULT(false)    
);

CREATE TABLE core.marital_statuses
(
	marital_status_id                       SERIAL NOT NULL PRIMARY KEY,
	marital_status_code                     national character varying(12) NOT NULL UNIQUE,
	marital_status_name                     national character varying(128) NOT NULL,
	is_legally_recognized_marriage          boolean NOT NULL DEFAULT(false),
	audit_user_id                           integer,    
	audit_ts                                TIMESTAMP WITH TIME ZONE DEFAULT(NOW()),
	deleted									boolean DEFAULT(false)
);



-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/04.default-values/01.default-values.sql --<--<--
INSERT INTO core.offices(office_code, office_name, currency_code, nick_name, po_box, address_line_1, address_line_2, street, city, state, country, phone, fax, email, url)
SELECT 'DEF', 'Default', 'USD', 'MixERP', '3415', 'Lobortis. Avenue', '', '', 'Rocky Mount', 'WA', 'United States', '(213) 3640-6139', '', 'info@mixerp.com', 'http://mixerp.com';

INSERT INTO core.genders(gender_code, gender_name)
SELECT 'M', 'Male' UNION ALL
SELECT 'F', 'Female';

INSERT INTO core.marital_statuses(marital_status_code, marital_status_name, is_legally_recognized_marriage)
SELECT 'NEM', 'Never Married',          false UNION ALL
SELECT 'SEP', 'Separated',              false UNION ALL
SELECT 'MAR', 'Married',                true UNION ALL
SELECT 'LIV', 'Living Relationship',    false UNION ALL
SELECT 'DIV', 'Divorced',               false UNION ALL
SELECT 'WID', 'Widower',                false UNION ALL
SELECT 'CIV', 'Civil Union',            true;

INSERT INTO core.currencies(currency_code, currency_symbol, currency_name, hundredth_name)
SELECT 'NPR', 'रू.',       'Nepali Rupees',        'paisa'     UNION ALL
SELECT 'USD', '$',      'United States Dollar', 'cents'     UNION ALL
SELECT 'GBP', '£',      'Pound Sterling',       'penny'     UNION ALL
SELECT 'EUR', '€',      'Euro',                 'cents'     UNION ALL
SELECT 'JPY', '¥',      'Japanese Yen',         'sen'       UNION ALL
SELECT 'CHF', 'CHF',    'Swiss Franc',          'centime'   UNION ALL
SELECT 'CAD', '¢',      'Canadian Dollar',      'cent'      UNION ALL
SELECT 'AUD', 'AU$',    'Australian Dollar',    'cent'      UNION ALL
SELECT 'HKD', 'HK$',    'Hong Kong Dollar',     'cent'      UNION ALL
SELECT 'INR', '₹',      'Indian Rupees',        'paise'     UNION ALL
SELECT 'SEK', 'kr',     'Swedish Krona',        'öre'       UNION ALL
SELECT 'NZD', 'NZ$',    'New Zealand Dollar',   'cent';


-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/05.scrud-views/core.office_scrud_view.sql --<--<--
DROP VIEW IF EXISTS core.office_scrud_view;

CREATE VIEW core.office_scrud_view
AS
SELECT
	core.offices.office_id,
	core.offices.office_code,
	core.offices.office_name,
	core.offices.currency_code,
	parent_office.office_code || ' (' || parent_office.office_name || ')' AS parent_office
FROM core.offices
LEFT JOIN core.offices AS parent_office
ON parent_office.office_id = core.offices.parent_office_id
WHERE NOT core.offices.deleted;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.create_app.sql --<--<--
DROP FUNCTION IF EXISTS core.create_app
(
    _app_name                                   text,
    _name                                       text,
    _version_number                             text,
    _publisher                                  text,
    _published_on                               date,
    _icon                                       text,
    _landing_url                                text,
    _dependencies                               text[]
);

CREATE FUNCTION core.create_app
(
    _app_name                                   text,
    _name                                       text,
    _version_number                             text,
    _publisher                                  text,
    _published_on                               date,
    _icon                                       text,
    _landing_url                                text,
    _dependencies                               text[]
)
RETURNS void
AS
$$
BEGIN
    IF EXISTS
    (
        SELECT 1
        FROM core.apps
        WHERE LOWER(core.apps.app_name) = LOWER(_app_name)
    ) THEN
        UPDATE core.apps
        SET
            name = _name,
            version_number = _version_number,
            publisher = _publisher,
            published_on = _published_on,
            icon = _icon,
            landing_url = _landing_url
        WHERE
            app_name = _app_name;
    ELSE
        INSERT INTO core.apps(app_name, name, version_number, publisher, published_on, icon, landing_url)
        SELECT _app_name, _name, _version_number, _publisher, _published_on, _icon, _landing_url;
    END IF;

    DELETE FROM core.app_dependencies
    WHERE app_name = _app_name;

    INSERT INTO core.app_dependencies(app_name, depends_on)
    SELECT _app_name, UNNEST(_dependencies);
END
$$
LANGUAGE plpgsql;


-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.create_menu.sql --<--<--
DROP FUNCTION IF EXISTS core.create_menu
(
    _sort                                       integer,
    _app_name                                   text,
    _menu_name                                  text,
    _url                                        text,
    _icon                                       text,
    _parent_menu_id                             integer    
);

DROP FUNCTION IF EXISTS core.create_menu
(
    _sort                                       integer,
    _app_name                                   text,
    _menu_name                                  text,
    _url                                        text,
    _icon                                       text,
    _parent_menu_name                           text
);

CREATE FUNCTION core.create_menu
(
    _sort                                       integer,
    _app_name                                   text,
    _menu_name                                  text,
    _url                                        text,
    _icon                                       text,
    _parent_menu_id                             integer
    
)
RETURNS integer
AS
$$
    DECLARE _menu_id                            integer;
BEGIN
    IF EXISTS
    (
       SELECT 1
       FROM core.menus
       WHERE LOWER(app_name) = LOWER(_app_name)
       AND LOWER(menu_name) = LOWER(_menu_name)
    ) THEN
        UPDATE core.menus
        SET
            sort = _sort,
            url = _url,
            icon = _icon,
            parent_menu_id = _parent_menu_id
       WHERE LOWER(app_name) = LOWER(_app_name)
       AND LOWER(menu_name) = LOWER(_menu_name)
       RETURNING menu_id INTO _menu_id;        
    ELSE
        INSERT INTO core.menus(sort, app_name, menu_name, url, icon, parent_menu_id)
        SELECT _sort, _app_name, _menu_name, _url, _icon, _parent_menu_id
        RETURNING menu_id INTO _menu_id;        
    END IF;

    RETURN _menu_id;
END
$$
LANGUAGE plpgsql;


CREATE FUNCTION core.create_menu
(
    _sort                                       integer,
    _app_name                                   text,
    _menu_name                                  text,
    _url                                        text,
    _icon                                       text,
    _parent_menu_name                           text    
)
RETURNS integer
AS
$$
    DECLARE _parent_menu_id                     integer;
BEGIN
    SELECT menu_id INTO _parent_menu_id
    FROM core.menus
    WHERE LOWER(menu_name) = LOWER(_parent_menu_name)
    AND LOWER(app_name) = LOWER(_app_name)
	AND NOT core.menus.deleted;

    RETURN core.create_menu(_sort, _app_name, _menu_name, _url, _icon, _parent_menu_id);
END
$$
LANGUAGE plpgsql;


DROP FUNCTION IF EXISTS core.create_menu
(
    _app_name                                   text,
    _menu_name                                  text,
    _url                                        text,
    _icon                                       text,
    _parent_menu_name                           text    
);

CREATE FUNCTION core.create_menu
(
    _app_name                                   text,
    _menu_name                                  text,
    _url                                        text,
    _icon                                       text,
    _parent_menu_name                           text    
)
RETURNS integer
AS
$$
BEGIN
    RETURN core.create_menu(0, _app_name, _menu_name, _url, _icon, _parent_menu_name);
END
$$
LANGUAGE plpgsql;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.get_currency_code_by_office_id.sql --<--<--
DROP FUNCTION IF EXISTS core.get_currency_code_by_office_id(_office_id integer);

CREATE FUNCTION core.get_currency_code_by_office_id(_office_id integer)
RETURNS national character varying(50)
AS
$$
BEGIN
    RETURN currency_code 
    FROM core.offices
    WHERE core.offices.office_id = _office_id
	AND NOT core.offices.deleted;
END
$$
LANGUAGE plpgsql;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.get_office_code_by_office_id.sql --<--<--
DROP FUNCTION IF EXISTS core.get_office_code_by_office_id(_office_id integer);

CREATE FUNCTION core.get_office_code_by_office_id(_office_id integer)
RETURNS national character varying(12)
AS
$$
BEGIN
    RETURN core.offices.office_code
    FROM core.offices
    WHERE core.offices.office_id = _office_id
	AND NOT core.offices.deleted;
END
$$
LANGUAGE plpgsql;


-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.get_office_id_by_office_name.sql --<--<--
DROP FUNCTION IF EXISTS core.get_office_id_by_office_name(_office_name text);

CREATE FUNCTION core.get_office_id_by_office_name(_office_name text)
RETURNS integer
AS
$$
BEGIN
    RETURN core.offices.office_id
    FROM core.offices
    WHERE core.offices.office_name = _office_name
	AND NOT core.offices.deleted;
END
$$
LANGUAGE plpgsql;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.get_office_ids.sql --<--<--
DROP FUNCTION IF EXISTS core.get_office_ids(root_office_id integer);

CREATE FUNCTION core.get_office_ids(root_office_id integer)
RETURNS SETOF integer
AS
$$
BEGIN
    RETURN QUERY 
    (
        WITH RECURSIVE office_cte(office_id, path) AS (
         SELECT
            tn.office_id,  tn.office_id::TEXT AS path
            FROM core.offices AS tn 
			WHERE tn.office_id =$1
			AND NOT core.offices.deleted
        UNION ALL
         SELECT
            c.office_id, (p.path || '->' || c.office_id::TEXT)
            FROM office_cte AS p, core.offices AS c 
			WHERE parent_office_id = p.office_id
        )

        SELECT office_id 
		FROM office_cte
    );
END
$$LANGUAGE plpgsql;


-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.get_office_name_by_office_id.sql --<--<--
DROP FUNCTION IF EXISTS core.get_office_name_by_office_id(_office_id integer);

CREATE FUNCTION core.get_office_name_by_office_id(_office_id integer)
RETURNS text
AS
$$
BEGIN
    RETURN core.offices.office_name
    FROM core.offices
    WHERE core.offices.office_id = _office_id
	AND NOT core.offices.deleted;
END
$$
LANGUAGE plpgsql;

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/06.functions-and-logic/core.is_valid_office_id.sql --<--<--
DROP FUNCTION IF EXISTS core.is_valid_office_id(integer);

CREATE FUNCTION core.is_valid_office_id(integer)
RETURNS boolean
AS
$$
BEGIN
    IF EXISTS(SELECT 1 FROM core.offices WHERE office_id=$1) THEN
        RETURN true;
    END IF;

    RETURN false;
END
$$
LANGUAGE plpgsql;

SELECT core.is_valid_office_id(1);

-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/10.policy/access_policy.sql --<--<--


-->-->-- src/Frapid.Web/Areas/Frapid.Core/db/PostgreSQL/1.x/1.0/src/99.ownership.sql --<--<--
DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'frapid_db_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT * FROM pg_tables 
    WHERE NOT schemaname = ANY(ARRAY['pg_catalog', 'information_schema'])
    AND tableowner <> 'frapid_db_user'
    LOOP
        EXECUTE 'ALTER TABLE '|| this.schemaname || '.' || this.tablename ||' OWNER TO frapid_db_user;';
    END LOOP;
END
$$
LANGUAGE plpgsql;

DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'frapid_db_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT oid::regclass::text as mat_view
    FROM   pg_class
    WHERE  relkind = 'm'
    LOOP
        EXECUTE 'ALTER TABLE '|| this.mat_view ||' OWNER TO frapid_db_user;';
    END LOOP;
END
$$
LANGUAGE plpgsql;

DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'frapid_db_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT 'ALTER '
        || CASE WHEN p.proisagg THEN 'AGGREGATE ' ELSE 'FUNCTION ' END
        || quote_ident(n.nspname) || '.' || quote_ident(p.proname) || '(' 
        || pg_catalog.pg_get_function_identity_arguments(p.oid) || ') OWNER TO frapid_db_user;' AS sql
    FROM   pg_catalog.pg_proc p
    JOIN   pg_catalog.pg_namespace n ON n.oid = p.pronamespace
    WHERE  NOT n.nspname = ANY(ARRAY['pg_catalog', 'information_schema'])
    LOOP        
        EXECUTE this.sql;
    END LOOP;
END
$$
LANGUAGE plpgsql;


DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'frapid_db_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT * FROM pg_views
    WHERE NOT schemaname = ANY(ARRAY['pg_catalog', 'information_schema'])
    AND viewowner <> 'frapid_db_user'
    LOOP
        EXECUTE 'ALTER VIEW '|| this.schemaname || '.' || this.viewname ||' OWNER TO frapid_db_user;';
    END LOOP;
END
$$
LANGUAGE plpgsql;


DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'frapid_db_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT 'ALTER SCHEMA ' || nspname || ' OWNER TO frapid_db_user;' AS sql FROM pg_namespace
    WHERE nspname NOT LIKE 'pg_%'
    AND nspname <> 'information_schema'
    LOOP
        EXECUTE this.sql;
    END LOOP;
END
$$
LANGUAGE plpgsql;



DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'frapid_db_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT      'ALTER TYPE ' || n.nspname || '.' || t.typname || ' OWNER TO frapid_db_user;' AS sql
    FROM        pg_type t 
    LEFT JOIN   pg_catalog.pg_namespace n ON n.oid = t.typnamespace 
    WHERE       (t.typrelid = 0 OR (SELECT c.relkind = 'c' FROM pg_catalog.pg_class c WHERE c.oid = t.typrelid)) 
    AND         NOT EXISTS(SELECT 1 FROM pg_catalog.pg_type el WHERE el.oid = t.typelem AND el.typarray = t.oid)
    AND         typtype NOT IN ('b')
    AND         n.nspname NOT IN ('pg_catalog', 'information_schema')
    LOOP
        EXECUTE this.sql;
    END LOOP;
END
$$
LANGUAGE plpgsql;


DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'report_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT * FROM pg_tables 
    WHERE NOT schemaname = ANY(ARRAY['pg_catalog', 'information_schema'])
    AND tableowner <> 'report_user'
    LOOP
        EXECUTE 'GRANT SELECT ON TABLE '|| this.schemaname || '.' || this.tablename ||' TO report_user;';
    END LOOP;
END
$$
LANGUAGE plpgsql;

DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'report_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT 'GRANT EXECUTE ON '
        || CASE WHEN p.proisagg THEN 'AGGREGATE ' ELSE 'FUNCTION ' END
        || quote_ident(n.nspname) || '.' || quote_ident(p.proname) || '(' 
        || pg_catalog.pg_get_function_identity_arguments(p.oid) || ') TO report_user;' AS sql
    FROM   pg_catalog.pg_proc p
    JOIN   pg_catalog.pg_namespace n ON n.oid = p.pronamespace
    WHERE  NOT n.nspname = ANY(ARRAY['pg_catalog', 'information_schema'])
    LOOP        
        EXECUTE this.sql;
    END LOOP;
END
$$
LANGUAGE plpgsql;


DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'report_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT * FROM pg_views
    WHERE NOT schemaname = ANY(ARRAY['pg_catalog', 'information_schema'])
    AND viewowner <> 'report_user'
    LOOP
        EXECUTE 'GRANT SELECT ON '|| this.schemaname || '.' || this.viewname ||' TO report_user;';
    END LOOP;
END
$$
LANGUAGE plpgsql;


DO
$$
    DECLARE this record;
BEGIN
    IF(CURRENT_USER = 'report_user') THEN
        RETURN;
    END IF;

    FOR this IN 
    SELECT 'GRANT USAGE ON SCHEMA ' || nspname || ' TO report_user;' AS sql FROM pg_namespace
    WHERE nspname NOT LIKE 'pg_%'
    AND nspname <> 'information_schema'
    LOOP
        EXECUTE this.sql;
    END LOOP;
END
$$
LANGUAGE plpgsql;


