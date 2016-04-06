﻿-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/01.types-domains-tables-and-constraints/tables-and-constraints.sql --<--<--
DROP SCHEMA IF EXISTS auth CASCADE;
CREATE SCHEMA auth;

CREATE TABLE auth.access_types
(
    access_type_id                              integer PRIMARY KEY,
    access_type_name                            national character varying(48) NOT NULL
);

CREATE UNIQUE INDEX access_types_uix
ON auth.access_types(UPPER(access_type_name));


CREATE TABLE auth.group_entity_access_policy
(
    group_entity_access_policy_id           SERIAL NOT NULL PRIMARY KEY,
    entity_name                             national character varying(128) NULL,
    office_id                               integer NOT NULL REFERENCES core.offices,
    role_id                                 integer NOT NULL REFERENCES account.roles,
    access_type_id                          integer NULL REFERENCES auth.access_types,
    allow_access                            boolean NOT NULL,
    audit_user_id                           integer NULL REFERENCES account.users,
    audit_ts                                TIMESTAMP WITH TIME ZONE NULL 
                                            DEFAULT(NOW())
);

CREATE TABLE auth.entity_access_policy
(
    entity_access_policy_id                 SERIAL NOT NULL PRIMARY KEY,
    entity_name                             national character varying(128) NULL,
    office_id                               integer NOT NULL REFERENCES core.offices,
    user_id                                 integer NOT NULL REFERENCES account.users,
    access_type_id                          integer NULL REFERENCES auth.access_types,
    allow_access                            boolean NOT NULL,
    audit_user_id                           integer NULL REFERENCES account.users,
    audit_ts                                TIMESTAMP WITH TIME ZONE NULL 
                                            DEFAULT(NOW())
);

CREATE TABLE auth.group_menu_access_policy
(
    group_menu_access_policy_id             BIGSERIAL PRIMARY KEY,
    office_id                               integer NOT NULL REFERENCES core.offices,
    menu_id                                 integer NOT NULL REFERENCES core.menus,
    role_id                                 integer REFERENCES account.roles,
    audit_user_id                           integer REFERENCES account.users,
    audit_ts                                TIMESTAMP WITH TIME ZONE NULL 
                                            DEFAULT(NOW())
);

CREATE UNIQUE INDEX menu_access_uix
ON auth.group_menu_access_policy(office_id, menu_id, role_id);

CREATE TABLE auth.menu_access_policy
(
    menu_access_policy_id                   BIGSERIAL PRIMARY KEY,
    office_id                               integer NOT NULL REFERENCES core.offices,
    menu_id                                 integer NOT NULL REFERENCES core.menus,
    user_id                                 integer NULL REFERENCES account.users,
    allow_access                            boolean,
    disallow_access                         boolean
                                            CHECK(NOT(allow_access is true AND disallow_access is true)),
    audit_user_id                           integer REFERENCES account.users,
    audit_ts                                TIMESTAMP WITH TIME ZONE NULL 
                                            DEFAULT(NOW())
);

CREATE UNIQUE INDEX menu_access_policy_uix
ON auth.menu_access_policy(office_id, menu_id, user_id);


-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.create_api_access_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.create_api_access_policy
(
    _role_names                     text[],
    _office_id                      integer,
    _entity_name                    text,
    _access_types                   text[],
    _allow_access                   boolean
);

CREATE FUNCTION auth.create_api_access_policy
(
    _role_names                     text[],
    _office_id                      integer,
    _entity_name                    text,
    _access_types                   text[],
    _allow_access                   boolean
)
RETURNS void
AS
$$
    DECLARE _role_id                integer;
    DECLARE _role_ids               integer[];
    DECLARE _access_type_ids        int[];
BEGIN
    IF(_role_names = '{*}'::text[]) THEN
        SELECT
            array_agg(role_id)
        INTO
            _role_ids
        FROM account.roles;
    ELSE
        SELECT
            array_agg(role_id)
        INTO
            _role_ids
        FROM account.roles
        WHERE role_name = ANY(_role_names);
    END IF;

    IF(_access_types = '{*}'::text[]) THEN
        SELECT
            array_agg(access_type_id)
        INTO
            _access_type_ids
        FROM auth.access_types;
    ELSE
        SELECT
            array_agg(access_type_id)
        INTO
            _access_type_ids
        FROM auth.access_types
        WHERE access_type_name = ANY(_access_types);
    END IF;

    IF(_role_ids IS NOT NULL) THEN
        FOREACH _role_id IN ARRAY _role_ids
        LOOP
            PERFORM auth.save_api_group_policy(_role_id, _entity_name, _office_id, _access_type_ids, _allow_access);
        END LOOP;
    END IF;
END
$$
LANGUAGE plpgsql;

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.create_app_menu_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.create_app_menu_policy
(
    _role_name                      text,
    _office_id                      integer,
    _app_name                       text,
    _menu_names                     text[]
);

CREATE FUNCTION auth.create_app_menu_policy
(
    _role_name                      text,
    _office_id                      integer,
    _app_name                       text,
    _menu_names                     text[]
)
RETURNS void
AS
$$
    DECLARE _role_id                integer;
    DECLARE _menu_ids               int[];
BEGIN
    SELECT
        role_id
    INTO
        _role_id
    FROM account.roles
    WHERE role_name = _role_name;

    IF(_menu_names = '{*}'::text[]) THEN
        SELECT
            array_agg(menu_id)
        INTO
            _menu_ids
        FROM core.menus
        WHERE app_name = _app_name;
    ELSE
        SELECT
            array_agg(menu_id)
        INTO
            _menu_ids
        FROM core.menus
        WHERE app_name = _app_name
        AND menu_name = ANY(_menu_names);
    END IF;
    
    PERFORM auth.save_group_menu_policy(_role_id, _office_id, _menu_ids, _app_name);    
END
$$
LANGUAGE plpgsql;

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.get_apps.sql --<--<--
DROP FUNCTION IF EXISTS auth.get_apps(_user_id integer, _office_id integer, _culture text);

CREATE FUNCTION auth.get_apps(_user_id integer, _office_id integer, _culture text)
RETURNS TABLE
(
    app_name                            text,
    name                                text,
    version_number                      text,
    publisher                           text,
    published_on                        date,
    icon                                text,
    landing_url                         text
)
AS
$$
BEGIN
    RETURN QUERY
    SELECT
        core.apps.app_name::text,
        core.apps.name::text,
        core.apps.version_number::text,
        core.apps.publisher::text,
        core.apps.published_on::date,
        core.apps.icon::text,
        core.apps.landing_url::text
    FROM core.apps
    WHERE core.apps.app_name IN
    (
        SELECT DISTINCT menus.app_name
        FROM auth.get_menu(_user_id, _office_id, _culture)
        AS menus
    );
END
$$
LANGUAGE plpgsql;


-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.get_group_menu_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.get_group_menu_policy
(
    _role_id        integer,
    _office_id      integer,
    _culture        text
);

CREATE FUNCTION auth.get_group_menu_policy
(
    _role_id        integer,
    _office_id      integer,
    _culture        text
)
RETURNS TABLE
(
    row_number                      integer,
    menu_id                         integer,
    app_name                        text,
    menu_name                       text,
    allowed                         boolean,
    url                             text,
    sort                            integer,
    icon                            character varying,
    parent_menu_id                  integer
)
AS
$$
BEGIN
    DROP TABLE IF EXISTS _temp_menu;
    CREATE TEMPORARY TABLE _temp_menu
    (
        row_number                      SERIAL,
        menu_id                         integer,
        app_name                        text,
        menu_name                       text,
        allowed                         boolean,
        url                             text,
        sort                            integer,
        icon                            character varying,
        parent_menu_id                  integer
    ) ON COMMIT DROP;

    INSERT INTO _temp_menu(menu_id)
    SELECT core.menus.menu_id
    FROM core.menus
    ORDER BY core.menus.app_name, core.menus.sort, core.menus.menu_id;

    --GROUP POLICY
    UPDATE _temp_menu
    SET allowed = true
    FROM  auth.group_menu_access_policy
    WHERE auth.group_menu_access_policy.menu_id = _temp_menu.menu_id
    AND office_id = _office_id
    AND role_id = _role_id;
   
    
    UPDATE _temp_menu
    SET
        app_name        = core.menus.app_name,
        menu_name       = core.menus.menu_name,
        url             = core.menus.url,
        sort            = core.menus.sort,
        icon            = core.menus.icon,
        parent_menu_id  = core.menus.parent_menu_id
    FROM core.menus
    WHERE core.menus.menu_id = _temp_menu.menu_id;

    UPDATE _temp_menu
    SET
        menu_name       = core.menu_locale.menu_text
    FROM core.menu_locale
    WHERE core.menu_locale.menu_id = _temp_menu.menu_id
    AND core.menu_locale.culture = _culture;
    

    RETURN QUERY
    SELECT * FROM _temp_menu
    ORDER BY app_name, sort, menu_id;
END
$$
LANGUAGE plpgsql;

--SELECT * FROM auth.get_group_menu_policy(1, 1, '');

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.get_menu.sql --<--<--
DROP FUNCTION IF EXISTS auth.get_menu
(
    _user_id                            integer, 
    _office_id                          integer, 
    _culture                            text
);

CREATE FUNCTION auth.get_menu
(
    _user_id                            integer, 
    _office_id                          integer, 
    _culture                            text
)
RETURNS TABLE
(
    menu_id                             integer,
    app_name                            character varying,
    menu_name                           character varying,
    url                                 text,
    sort                                integer,
    icon                                character varying,
    parent_menu_id                      integer
)
AS
$$
    DECLARE _role_id                    integer;
BEGIN
    SELECT
        role_id
    INTO
        _role_id
    FROM account.users
    WHERE user_id = _user_id;

    DROP TABLE IF EXISTS _temp_menu;
    CREATE TEMPORARY TABLE _temp_menu
    (
        menu_id                         integer,
        app_name                        character varying,
        menu_name                       character varying,
        url                             text,
        sort                            integer,
        icon                            character varying,
        parent_menu_id                  integer
    ) ON COMMIT DROP;


    --GROUP POLICY
    INSERT INTO _temp_menu(menu_id)
    SELECT auth.group_menu_access_policy.menu_id
    FROM auth.group_menu_access_policy
    WHERE office_id = _office_id
    AND role_id = _role_id;

    --USER POLICY : ALLOWED MENUS
    INSERT INTO _temp_menu(menu_id)
    SELECT auth.menu_access_policy.menu_id
    FROM auth.menu_access_policy
    WHERE office_id = _office_id
    AND user_id = _user_id
    AND allow_access
    AND auth.menu_access_policy.menu_id NOT IN
    (
        SELECT _temp_menu.menu_id
        FROM _temp_menu
    );

    --USER POLICY : DISALLOWED MENUS
    DELETE FROM _temp_menu
    WHERE _temp_menu.menu_id
    IN
    (
        SELECT auth.menu_access_policy.menu_id
        FROM auth.menu_access_policy
        WHERE office_id = _office_id
        AND user_id = _user_id
        AND disallow_access
    );

    
    UPDATE _temp_menu
    SET
        app_name        = core.menus.app_name,
        menu_name       = core.menus.menu_name,
        url             = core.menus.url,
        sort            = core.menus.sort,
        icon            = core.menus.icon,
        parent_menu_id  = core.menus.parent_menu_id
    FROM core.menus
    WHERE core.menus.menu_id = _temp_menu.menu_id;

    UPDATE _temp_menu
    SET
        menu_name       = core.menu_locale.menu_text
    FROM core.menu_locale
    WHERE core.menu_locale.menu_id = _temp_menu.menu_id
    AND core.menu_locale.culture = _culture;
    

    RETURN QUERY
    SELECT * FROM _temp_menu;
END
$$
LANGUAGE plpgsql;

--SELECT * FROM auth.get_menu(1, 1, '');

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.get_user_menu_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.get_user_menu_policy
(
    _user_id        integer,
    _office_id      integer,
    _culture        text
);

CREATE FUNCTION auth.get_user_menu_policy
(
    _user_id        integer,
    _office_id      integer,
    _culture        text
)
RETURNS TABLE
(
    row_number                      integer,
    menu_id                         integer,
    app_name                        text,
    menu_name                       text,
    allowed                         boolean,
    disallowed                      boolean,
    url                             text,
    sort                            integer,
    icon                            character varying,
    parent_menu_id                  integer
)
AS
$$
    DECLARE _role_id                    integer;
BEGIN
    SELECT
        role_id
    INTO
        _role_id
    FROM account.users
    WHERE user_id = _user_id;

    DROP TABLE IF EXISTS _temp_menu;
    CREATE TEMPORARY TABLE _temp_menu
    (
        row_number                      SERIAL,
        menu_id                         integer,
        app_name                        text,
        menu_name                       text,
        allowed                         boolean,
        disallowed                      boolean,
        url                             text,
        sort                            integer,
        icon                            character varying,
        parent_menu_id                  integer
    ) ON COMMIT DROP;

    INSERT INTO _temp_menu(menu_id)
    SELECT core.menus.menu_id
    FROM core.menus
    ORDER BY core.menus.app_name, core.menus.sort, core.menus.menu_id;

    --GROUP POLICY
    UPDATE _temp_menu
    SET allowed = true
    FROM  auth.group_menu_access_policy
    WHERE auth.group_menu_access_policy.menu_id = _temp_menu.menu_id
    AND office_id = _office_id
    AND role_id = _role_id;
    
    --USER POLICY : ALLOWED MENUS
    UPDATE _temp_menu
    SET allowed = true
    FROM  auth.menu_access_policy
    WHERE auth.menu_access_policy.menu_id = _temp_menu.menu_id
    AND office_id = _office_id
    AND user_id = _user_id
    AND allow_access;


    --USER POLICY : DISALLOWED MENUS
    UPDATE _temp_menu
    SET disallowed = true
    FROM auth.menu_access_policy
    WHERE _temp_menu.menu_id = auth.menu_access_policy.menu_id 
    AND office_id = _office_id
    AND user_id = _user_id
    AND disallow_access;
   
    
    UPDATE _temp_menu
    SET
        app_name        = core.menus.app_name,
        menu_name       = core.menus.menu_name,
        url             = core.menus.url,
        sort            = core.menus.sort,
        icon            = core.menus.icon,
        parent_menu_id  = core.menus.parent_menu_id
    FROM core.menus
    WHERE core.menus.menu_id = _temp_menu.menu_id;

    UPDATE _temp_menu
    SET
        menu_name       = core.menu_locale.menu_text
    FROM core.menu_locale
    WHERE core.menu_locale.menu_id = _temp_menu.menu_id
    AND core.menu_locale.culture = _culture;
    

    RETURN QUERY
    SELECT * FROM _temp_menu
    ORDER BY app_name, sort, menu_id;
END
$$
LANGUAGE plpgsql;

--SELECT * FROM auth.get_user_menu_policy(1, 1, '');

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.has_access.sql --<--<--
DROP FUNCTION IF EXISTS auth.has_access(_user_id integer, _entity text, _access_type_id integer);

CREATE FUNCTION auth.has_access(_user_id integer, _entity text, _access_type_id integer)
RETURNS boolean
AS
$$
    DECLARE _role_id                                    integer;
    DECLARE _group_all_policy                           boolean = false;
    DECLARE _group_all_entity_specific_access_type      boolean = false;
    DECLARE _group_specific_entity_all_access_type      boolean = false;
    DECLARE _group_explicit_policy                      boolean = false;
    DECLARE _effective_group_policy                     boolean = false;
    DECLARE _user_all_policy                            boolean = false;
    DECLARE _user_all_entity_specific_access_type       boolean = false;
    DECLARE _user_specific_entity_all_access_type       boolean = false;
    DECLARE _user_explicit_policy                       boolean = false;
    DECLARE _effective_user_policy                      boolean = false;
BEGIN    
    --USER AUTHORIZATION BASED ON ALL ENTITIES AND ALL ACCESS TYPES
    SELECT 
        allow_access 
    INTO 
        _user_all_policy
    FROM auth.entity_access_policy
    WHERE user_id = _user_id
    AND access_type_id IS NULL
    AND COALESCE(entity_name, '') = '';

    --USER AUTHORIZATION BASED ON ALL ENTITIES AND SPECIFIED ACCESS TYPE
    SELECT 
        allow_access
    INTO
        _user_all_entity_specific_access_type
    FROM auth.entity_access_policy
    WHERE user_id = _user_id
    AND access_type_id = _access_type_id
    AND COALESCE(entity_name, '') = '';

    --USER AUTHORIZATION BASED ON SPECIFIED ENTITY AND ALL ACCESS TYPES
    SELECT
        allow_access
    INTO
        _user_specific_entity_all_access_type
    FROM auth.entity_access_policy
    WHERE user_id = _user_id
    AND access_type_id IS NULL
    AND entity_name = _entity;

    --USER AUTHORIZATION BASED ON SPECIFIED ENTITY AND SPECIFIED ACCESS TYPE
    SELECT 
        allow_access
    INTO
        _user_explicit_policy
    FROM auth.entity_access_policy
    WHERE user_id = _user_id
    AND access_type_id = _access_type_id
    AND entity_name = _entity;

    --EFFECTIVE USER POLICY BASED ON PRECEDENCE.
    _effective_user_policy := COALESCE(_user_explicit_policy, _user_specific_entity_all_access_type, _user_all_entity_specific_access_type, _user_all_policy);

    IF(_effective_user_policy IS NOT NULL) THEN
        RETURN _effective_user_policy;
    END IF;

    SELECT role_id INTO _role_id FROM account.users WHERE user_id = _user_id;

    --GROUP AUTHORIZATION BASED ON ALL ENTITIES AND ALL ACCESS TYPES
    SELECT 
        allow_access 
    INTO 
        _group_all_policy
    FROM auth.group_entity_access_policy
    WHERE role_id = _role_id
    AND access_type_id IS NULL
    AND COALESCE(entity_name, '') = '';

    --GROUP AUTHORIZATION BASED ON ALL ENTITIES AND SPECIFIED ACCESS TYPE
    SELECT 
        allow_access
    INTO
        _group_all_entity_specific_access_type
    FROM auth.group_entity_access_policy
    WHERE role_id = _role_id
    AND access_type_id = _access_type_id
    AND COALESCE(entity_name, '') = '';

    --GROUP AUTHORIZATION BASED ON SPECIFIED ENTITY AND ALL ACCESS TYPES
    SELECT
        allow_access
    INTO
        _group_specific_entity_all_access_type
    FROM auth.group_entity_access_policy
    WHERE role_id = _role_id
    AND access_type_id IS NULL
    AND entity_name = _entity;

    --GROUP AUTHORIZATION BASED ON SPECIFIED ENTITY AND SPECIFIED ACCESS TYPE
    SELECT 
        allow_access
    INTO
        _group_explicit_policy
    FROM auth.group_entity_access_policy
    WHERE role_id = _role_id
    AND access_type_id = _access_type_id
    AND entity_name = _entity;

    --EFFECTIVE GROUP POLICY BASED ON PRECEDENCE.
    _effective_group_policy := COALESCE(_group_explicit_policy, _group_specific_entity_all_access_type, _group_all_entity_specific_access_type, _group_all_policy);

    RETURN COALESCE(_effective_group_policy, false);    
END
$$
LANGUAGE plpgsql;


-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.save_api_group_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.save_api_group_policy
(
    _role_id            integer,
    _entity_name        national character varying(128),
    _office_id          integer,
    _access_type_ids    int[],
    _allow_access       boolean
);

CREATE FUNCTION auth.save_api_group_policy
(
    _role_id            integer,
    _entity_name        national character varying(128),
    _office_id          integer,
    _access_type_ids    int[],
    _allow_access       boolean
)
RETURNS void
AS
$$
BEGIN
    IF(_role_id IS NULL OR _office_id IS NULL) THEN
        RETURN;
    END IF;
    
    DELETE FROM auth.group_entity_access_policy
    WHERE auth.group_entity_access_policy.access_type_id 
    NOT IN
    (
        SELECT * from unnest(_access_type_ids)
    )
    AND role_id = _role_id
    AND office_id = _office_id
    AND entity_name = _entity_name
    AND access_type_id IN
    (
        SELECT access_type_id
        FROM auth.access_types
    );

    WITH access_types
    AS
    (
        SELECT unnest(_access_type_ids) AS _access_type_id
    )
    
    INSERT INTO auth.group_entity_access_policy(role_id, office_id, entity_name, access_type_id, allow_access)
    SELECT _role_id, _office_id, _entity_name, _access_type_id, _allow_access
    FROM access_types
    WHERE _access_type_id NOT IN
    (
        SELECT access_type_id
        FROM auth.group_entity_access_policy
        WHERE auth.group_entity_access_policy.role_id = _role_id
        AND auth.group_entity_access_policy.office_id = _office_id
    );

    RETURN;
END
$$
LANGUAGE plpgsql;



-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.save_group_menu_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.save_group_menu_policy
(
    _role_id        integer,
    _office_id      integer,
    _menu_ids       int[],
    _app_name       text
);

CREATE FUNCTION auth.save_group_menu_policy
(
    _role_id        integer,
    _office_id      integer,
    _menu_ids       int[],
    _app_name       text
)
RETURNS void
AS
$$
BEGIN
    IF(_role_id IS NULL OR _office_id IS NULL) THEN
        RETURN;
    END IF;
    
    DELETE FROM auth.group_menu_access_policy
    WHERE auth.group_menu_access_policy.menu_id NOT IN(SELECT * from unnest(_menu_ids))
    AND role_id = _role_id
    AND office_id = _office_id
    AND menu_id IN
    (
        SELECT menu_id
        FROM core.menus
        WHERE app_name = _app_name
    );

    WITH menus
    AS
    (
        SELECT unnest(_menu_ids) AS _menu_id
    )
    
    INSERT INTO auth.group_menu_access_policy(role_id, office_id, menu_id)
    SELECT _role_id, _office_id, _menu_id
    FROM menus
    WHERE _menu_id NOT IN
    (
        SELECT menu_id
        FROM auth.group_menu_access_policy
        WHERE auth.group_menu_access_policy.role_id = _role_id
        AND auth.group_menu_access_policy.office_id = _office_id
    );

    RETURN;
END
$$
LANGUAGE plpgsql;

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/02.functions-and-logic/auth.save_user_menu_policy.sql --<--<--
DROP FUNCTION IF EXISTS auth.save_user_menu_policy
(
    _user_id                        integer,
    _office_id                      integer,
    _allowed_menu_ids               int[],
    _disallowed_menu_ids            int[]
);

CREATE FUNCTION auth.save_user_menu_policy
(
    _user_id                        integer,
    _office_id                      integer,
    _allowed_menu_ids               int[],
    _disallowed_menu_ids            int[]
)
RETURNS void
VOLATILE AS
$$
BEGIN
    INSERT INTO auth.menu_access_policy(office_id, user_id, menu_id)
    SELECT _office_id, _user_id, core.menus.menu_id
    FROM core.menus
    WHERE core.menus.menu_id NOT IN
    (
        SELECT auth.menu_access_policy.menu_id
        FROM auth.menu_access_policy
        WHERE user_id = _user_id
        AND office_id = _office_id
    );

    UPDATE auth.menu_access_policy
    SET allow_access = NULL, disallow_access = NULL
    WHERE user_id = _user_id
    AND office_id = _office_id;

    UPDATE auth.menu_access_policy
    SET allow_access = true
    WHERE user_id = _user_id
    AND office_id = _office_id
    AND menu_id IN(SELECT * from unnest(_allowed_menu_ids));

    UPDATE auth.menu_access_policy
    SET disallow_access = true
    WHERE user_id = _user_id
    AND office_id = _office_id
    AND menu_id IN(SELECT * from unnest(_disallowed_menu_ids));

    
    RETURN;
END
$$
LANGUAGE plpgsql;

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/03.menus/0.menus.sql --<--<--
SELECT * FROM core.create_app('Frapid.Authorization', 'Authorization', '1.0', 'MixERP Inc.', 'December 1, 2015', 'purple privacy', '/dashboard/authorization/menu-access/group-policy', '{Frapid.Account}'::text[]);

SELECT * FROM core.create_menu('Frapid.Authorization', 'Entity Access Policy', '', 'lock', '');
SELECT * FROM core.create_menu('Frapid.Authorization', 'Group Policy', '/dashboard/authorization/entity-access/group-policy', 'users', 'Entity Access Policy');
SELECT * FROM core.create_menu('Frapid.Authorization', 'User Policy', '/dashboard/authorization/entity-access/user-policy', 'user', 'Entity Access Policy');
SELECT * FROM core.create_menu('Frapid.Authorization', 'Menu Access Policy', '', 'toggle on', '');
SELECT * FROM core.create_menu('Frapid.Authorization', 'Group Policy', '/dashboard/authorization/menu-access/group-policy', 'users', 'Menu Access Policy');
SELECT * FROM core.create_menu('Frapid.Authorization', 'User Policy', '/dashboard/authorization/menu-access/user-policy', 'user', 'Menu Access Policy');


SELECT * FROM auth.create_app_menu_policy
(
    'Admin', 
    core.get_office_id_by_office_name('Default'), 
    'Frapid.Authorization',
    '{*}'::text[]
);

SELECT * FROM auth.create_app_menu_policy
(
    'Admin', 
    core.get_office_id_by_office_name('Default'), 
    'Frapid.Account',
    '{*}'::text[]
);


-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/03.menus/1.menu-policy.sql --<--<--
SELECT * FROM auth.create_app_menu_policy
(
    'Admin', 
    core.get_office_id_by_office_name('Default'), 
    'Frapid.Authorization',
    '{*}'::text[]
);


-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/03.menus/2.menu-policy-account.sql --<--<--
SELECT * FROM auth.create_app_menu_policy
(
    'User', 
    core.get_office_id_by_office_name('Default'), 
    'Frapid.Account',
    '{Configuration Profile, Email Templates, Account Verification, Password Reset, Welcome Email, Welcome Email (3rd Party)}'::text[]
);

SELECT * FROM auth.create_app_menu_policy
(
    'Admin', 
    core.get_office_id_by_office_name('Default'), 
    'Frapid.Account',
    '{*}'::text[]
);

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/04.default-values/01.default-values.sql --<--<--
INSERT INTO auth.access_types(access_type_id, access_type_name)
SELECT 1, 'Read'            UNION ALL
SELECT 2, 'Create'          UNION ALL
SELECT 3, 'Edit'            UNION ALL
SELECT 4, 'Delete'          UNION ALL
SELECT 5, 'CreateFilter'    UNION ALL
SELECT 6, 'DeleteFilter'    UNION ALL
SELECT 7, 'Export'          UNION ALL
SELECT 8, 'ExportData'      UNION ALL
SELECT 9, 'ImportData'      UNION ALL
SELECT 10, 'Execute'        UNION ALL
SELECT 11, 'Verify';

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/10.policy/access_policy.sql --<--<--
SELECT * FROM auth.create_api_access_policy('{Admin}', core.get_office_id_by_office_name('Default'), '', '{*}', true);

-->-->-- C:/Users/nirvan/Desktop/mixerp/frapid/src/Frapid.Web/Areas/Frapid.Authorization/db/1.x/1.0/src/99.ownership.sql --<--<--
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
