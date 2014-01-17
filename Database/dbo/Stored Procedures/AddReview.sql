CREATE PROCEDURE [dbo].[AddReview]
@UserName nvarchar(200), @ReviewerAlias nvarchar(200), @ChangeId INT, @Text NVARCHAR (2048)=NULL, @Status TINYINT=NULL, @Result INT OUTPUT
AS
BEGIN
    DECLARE @ReviewId int
    SET @ReviewId = (SELECT Id FROM dbo.Review
                     WHERE ChangeListId = @ChangeId AND (UserName = @UserName OR ReviewerAlias = @ReviewerAlias) AND IsSubmitted = 0)
    IF @ReviewId IS NOT NULL
    BEGIN
        IF @Text IS NOT NULL
            UPDATE dbo.Review SET CommentText = @Text WHERE Id = @ReviewId

        IF @Status IS NOT NULL
            UPDATE dbo.Review SET OverallStatus = @Status WHERE Id = @ReviewId

        SET @Result = @ReviewId
        SELECT @result
        RETURN
    END

    DECLARE @Status2 int
    SET @Status2 = 0
    IF @Status IS NOT NULL
        SET @Status2 = @Status

    INSERT INTO dbo.Review (ChangeListId, UserName, ReviewerAlias, TimeStamp, IsSubmitted, OverallStatus, CommentText)
        VALUES(@ChangeId, @UserName, @ReviewerAlias, GETUTCDATE(), 0, @Status2, @Text)
    SET @Result = @@IDENTITY
    SELECT @result
END
