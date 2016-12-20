﻿IF OBJECT_ID('account.sign_in') IS NOT NULL
DROP PROCEDURE account.sign_in;

GO


CREATE PROCEDURE account.sign_in
(
    @email                                  national character varying(500),
    @office_id                              integer,
    @browser                                national character varying(500),
    @ip_address                             national character varying(500),
    @culture                                national character varying(500)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @login_id                       bigint;
    DECLARE @user_id                        integer;

	DECLARE @result TABLE
	(
		login_id                                bigint,
		status                                  bit,
		message                                 national character varying(100)
	);


    IF(COALESCE(@office_id, 0) = 0)
    BEGIN
        IF(SELECT COUNT(*) FROM core.offices) = 1
        BEGIN
            SELECT @office_id = office_id
            FROM core.offices;
        END;
    END;

    IF account.is_restricted_user(@email) = 1
    BEGIN
		INSERT INTO @result
        SELECT CAST(NULL AS bigint), 0, 'Access is denied';

		SELECT * FROM @result;
        RETURN;
    END;

    SELECT @user_id = user_id 
    FROM account.users
    WHERE email = @email;

	UPDATE account.logins 
	SET is_active = 0 
	WHERE user_id=@user_id 
	AND office_id = @office_id 
	AND browser = @browser
	AND ip_address = @ip_address;

    INSERT INTO account.logins(user_id, office_id, browser, ip_address, login_timestamp, culture)
    SELECT @user_id, @office_id, @browser, @ip_address, getutcdate(), COALESCE(@culture, '');

    SET @login_id = SCOPE_IDENTITY();
    
	INSERT INTO @result
    SELECT @login_id, 1, 'Welcome';

	SELECT * FROM @result;
    RETURN;    
END;


GO