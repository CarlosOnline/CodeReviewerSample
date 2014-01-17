CREATE PROCEDURE [dbo].[AddUser]
	@Id INT = 0,
	@Username NVARCHAR(500),
	@Password NVARCHAR(MAX),
	@Email NVARCHAR(500),
	@RegDate DATETIME = NULL,
	@Token UNIQUEIDENTIFIER = NULL,
	@Result INT OUTPUT
AS
	if (@RegDate IS NULL)
		set @RegDate = GETDATE()

	if (@Token IS NULL)
		set @Token = NEWID()

	if (@Id > 0) BEGIN
		SELECT @Id=Id
		FROM dbo.[User]
		WHERE Id = @Id

		IF (@Id IS NOT NULL) BEGIN
			UPDATE dbo.[User]
			SET UserName = @Username,
				Email = @Email,
				[Password] = @Password,
				RegDate = @RegDate,
				Token = @Token
			WHERE Id=@Id

			set @Result=@Id
			RETURN @Result
		END
	END

	INSERT INTO dbo.[User] (UserName, [Password], Email, RegDate, Token)
	VALUES (@Username, @Password, @Email, @RegDate, @Token)

	set @Result = @@IDENTITY
RETURN @Result
