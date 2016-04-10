IF OBJECT_ID('account.reset_account') IS NOT NULL
DROP PROCEDURE account.reset_account;

GO


CREATE PROCEDURE account.reset_account
(
    @email                                  national character varying(500),
    @browser                                national character varying(500),
    @ip_address                             national character varying(500)
)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @request_table_variable			TABLE(request_id uniqueidentifier);
    DECLARE @user_id                        integer;
    DECLARE @name                           national character varying(500);
    DECLARE @expires_on                     datetimeoffset = dateadd(d, 1, getutcdate());

    IF(account.user_exists(@email) = 0 OR account.is_restricted_user(@email) = 1)
    BEGIN
        RETURN;
    END;

    SELECT
        @user_id = user_id,
        @name = name
    FROM account.users
    WHERE email = @email;

    IF account.has_active_reset_request(@email) = 1
    BEGIN
        SELECT 
        TOP 1
        * FROM account.reset_requests
        WHERE email = @email
        AND expires_on <= @expires_on;
        
        RETURN;
    END;

    INSERT INTO account.reset_requests(user_id, email, name, browser, ip_address, expires_on)
    OUTPUT INSERTED.request_id INTO @request_table_variable
    SELECT @user_id, @email, @name, @browser, @ip_address, @expires_on


    SELECT *
    FROM account.reset_requests
    WHERE request_id = 
    (
		SELECT request_id 
		FROM @request_table_variable
	);

    RETURN;
END;


GO
