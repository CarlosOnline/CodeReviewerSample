CREATE PROCEDURE [dbo].[AddCommentEx]
    @CommentId INT,
    @UserName NVARCHAR(200),
    @ReviewerAlias NVARCHAR(200),
    @CommentText NVARCHAR (MAX),
    @ReviewRevision INT,
    @ReviewerId INT,
    @FileVersionId INT,
    @GroupId INT,
    @ChangeListId INT,
    @LineStamp nvarchar(200),
    @Status INT,
    @result INT OUTPUT
AS
BEGIN
    IF @ReviewerId IS NULL OR @ReviewerId = 0 BEGIN
        SET @ReviewerId = (SELECT Id from dbo.Reviewer WHERE UserName = @UserName OR ReviewerAlias = @ReviewerAlias)
        IF @ReviewerId IS NULL OR @ReviewerId = 0 BEGIN
            EXEC AddReviewer @ReviewerId, @UserName, @ReviewerAlias, @ChangeListId, @Status, 0, @ReviewerId OUTPUT
        END
    END

    IF @GroupId = 0 BEGIN
        EXEC AddCommentGroup @GroupId, @FileVersionId, 1, @LineStamp, @Status, @result OUTPUT
        SET @GroupId = @result
    END
    IF @GroupId = 0 BEGIN
        SET @result = 0
    END
    -- UPDATE EXISTING COMMENT
    ELSE IF @CommentId IS NOT NULL AND @CommentId > 0 BEGIN
        UPDATE dbo.Comment SET CommentText = @CommentText WHERE Id = @CommentId
        SET @result = @CommentId
    END
    ELSE BEGIN
        INSERT INTO dbo.Comment (CommentText, ReviewRevision, ReviewerId, FileVersionId, GroupId, UserName, ReviewerAlias)
            VALUES(@CommentText, @ReviewRevision, @ReviewerId, @FileVersionId, @GroupId, @UserName, @ReviewerAlias)
        SET @result = @@IDENTITY
    END
    SELECT @result AS [Result]
END
