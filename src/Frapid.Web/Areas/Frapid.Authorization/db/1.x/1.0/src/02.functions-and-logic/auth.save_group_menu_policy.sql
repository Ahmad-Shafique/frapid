DROP FUNCTION IF EXISTS auth.save_group_menu_policy
(
    _role_id        integer,
    _office_id      integer,
    _menu_ids       int[]
);

CREATE FUNCTION auth.save_group_menu_policy
(
    _role_id        integer,
    _office_id      integer,
    _menu_ids       int[]
)
RETURNS void
AS
$$
BEGIN
    IF(_role_id IS NULL OR _office_id IS NULL) THEN
        RETURN;
    END IF;
    
    DELETE FROM auth.group_menu_access_policy
    WHERE auth.group_menu_access_policy.menu_id NOT IN(SELECT * from explode_array(_menu_ids))
    AND role_id = _role_id
    AND office_id = _office_id;

    WITH menus
    AS
    (
        SELECT explode_array(_menu_ids) AS _menu_id
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