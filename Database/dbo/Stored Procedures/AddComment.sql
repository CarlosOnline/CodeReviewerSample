CREATE PROCEDURE [dbo].[AddComment]
@CommentId INT, @UserName NVARCHAR(200), @ReviewerAlias NVARCHAR(200), @CommentText NVARCHAR (2048), @ReviewRevision INT, @FileVersionId INT, @GroupId INT, @result INT OUTPUT
AS
BEGIN
    DECLARE @FileId INT = 0

    -- UPDATE EXISTING COMMENT
    IF @CommentId IS NOT NULL AND @CommentId > 0 BEGIN
        UPDATE dbo.Comment SET CommentText = @CommentText, ReviewRevision=@ReviewRevision
        WHERE Id = @CommentId
        SET @result = @CommentId
    END
    ELSE BEGIN
        INSERT INTO dbo.Comment (CommentText, ReviewRevision, FileVersionId, GroupId, UserName, ReviewerAlias)
            VALUES(@CommentText, @ReviewRevision, @FileVersionId, @GroupId, @UserName, @ReviewerAlias)
        SET @result = @@IDENTITY
    END
    SELECT @result

END
