CREATE PROCEDURE [dbo].[DeleteComment]
@CommentId INT, @result INT OUTPUT
AS
BEGIN
    DECLARE @GroupId INT
    SELECT @GroupId = GroupId FROM dbo.Comment WHERE Id = @CommentId

    DELETE FROM dbo.Comment WHERE Id = @CommentId

    IF @GroupId IS NOT NULL AND @GroupId > 0
    BEGIN
        DECLARE @count int = 0
        SELECT @count = COUNT(*) FROM dbo.Comment WHERE GroupId=@GroupId
        IF @count = 0
        BEGIN
            DELETE FROM dbo.CommentGroup WHERE Id = @GroupId
        END
    END
    SELECT @GroupId
    SET @result = @GroupId
END
