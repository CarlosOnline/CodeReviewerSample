CREATE PROCEDURE [dbo].[SetUserContext]
@Key NVARCHAR (50), @Value NVARCHAR (MAX), @UserName nvarchar(200), @ReviewerAlias nvarchar(200), @Version INT, @result INT OUTPUT
AS
BEGIN
    DECLARE @Id int
    SET @Id = (SELECT Id FROM dbo.UserContext WHERE UserName = @UserName AND KeyName = @Key)
    IF @Id IS NOT NULL
    BEGIN
        UPDATE dbo.UserContext SET Value = @Value, [Version] = @Version WHERE Id = @Id
        IF @ReviewerAlias IS NOT NULL AND @ReviewerAlias != '' BEGIN
            UPDATE dbo.UserContext SET ReviewerAlias = @ReviewerAlias WHERE Id = @Id
        END
        SET @result = @Id
        SELECT @result
        RETURN
    END

    INSERT INTO dbo.UserContext (UserName, ReviewerAlias, KeyName, Value, [Version]) VALUES(@UserName, @ReviewerAlias, @Key, @Value, @Version)
    SET @result = @@IDENTITY
    SELECT @result
END
