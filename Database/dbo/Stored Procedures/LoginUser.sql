CREATE PROCEDURE [dbo].[LoginUser]
	@Id INT = 0,
	@Username NVARCHAR(500) = NULL,
	@Email NVARCHAR(500) = NULL,
	@Password NVARCHAR(MAX) = NULL,
	@Token UNIQUEIDENTIFIER = NULL,
	@Result INT OUTPUT
AS
	SET @Result = 0
	IF (@Id <= 0) set @Id = NULL
	IF (@Username = '') set @Username = NULL
	IF (@Email = '') set @Email = NULL
	IF (@Password = '') set @Password = NULL

	IF (@Password IS NULL AND @Token IS NULL) OR (@Username IS NULL AND @Email IS NULL) BEGIN
		-- invalid credentials
		RETURN 0
	END

	DECLARE @LoginPassword NVARCHAR(MAX)
	DECLARE @LoginToken UNIQUEIDENTIFIER
	DECLARE @LoginTokenDate DATETIME

	DECLARE @LoginId INT = NULL
	SELECT @LoginId = Id,
		@LoginPassword = [Password],
		@LoginToken = [Token],
		@LoginTokenDate = [TokenDate]
	FROM dbo.[User]
	WHERE 
		(@Id IS NOT NULL AND Id = @Id)
		OR (@Username IS NOT NULL AND UserName = @Username)
		OR (@Email IS NOT NULL AND Email = @Email)

	IF (@LoginId IS NULL) BEGIN
		SET @Result = -1 -- NOT FOUND
		RETURN @Result
	END

	DECLARE @Found INT = 0
	IF (@Found = 0 AND @Password IS NOT NULL AND @Password = @LoginPassword) BEGIN
		SET @Found = 1
	END

	IF (@Found = 0 AND @Token IS NOT NULL AND @Token = @LoginToken) BEGIN
		IF (DATEDIFF(day, @LoginTokenDate, GETDATE()) > 14) BEGIN
			SET @Result = -2 -- Expired token
			RETURN @Result
		END

		SET @Found = 1
	END

	IF (@Found <= 0) BEGIN
		UPDATE dbo.[User]
		SET Attempts = Attempts + 1,
			LastAttempt = GETDATE()
		WHERE Id = @Id
		RETURN @Result
	END ELSE BEGIN
		UPDATE dbo.[User]
		SET Attempts = 0,
			LastAttempt = GETDATE()
		WHERE Id = @LoginId
		SET @Result = @LoginId
	END

RETURN @Result
