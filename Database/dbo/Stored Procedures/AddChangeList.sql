CREATE PROCEDURE [dbo].[AddChangeList]
@SourceControl INT, @UserName NVARCHAR(50), @ReviewerAlias NVARCHAR (256), @UserClient NVARCHAR (50), @CL NVARCHAR (128), @Url NVARCHAR (2048), @Title NVARCHAR (128), @Description NVARCHAR (MAX), @result INT OUTPUT
AS
BEGIN
    DECLARE @ReviewRevision int = 0
    DECLARE @ChangeId int
    SET @ChangeId = (SELECT Id FROM dbo.ChangeList
        WHERE SourceControlId = @SourceControl AND (UserName = @UserName OR ReviewerAlias = @ReviewerAlias) AND UserClient = @UserClient AND CL = @CL)
    IF @ChangeId IS NOT NULL
    BEGIN
        SELECT @ReviewRevision = (SELECT CurrentReviewRevision FROM dbo.ChangeList
            WHERE SourceControlId = @SourceControl AND (UserName = @UserName OR ReviewerAlias = @ReviewerAlias) AND UserClient = @UserClient AND CL = @CL)
        -- Increment Current Review Revision
        SET @ReviewRevision = @ReviewRevision + 1

        SET @result = @ChangeId
        UPDATE dbo.ChangeList SET Stage = 0, CurrentReviewRevision=@ReviewRevision, Title=@Title, Description = @Description, TimeStamp = GETDATE() WHERE Id = @ChangeId
        IF @ReviewerAlias IS NOT NULL AND @ReviewerAlias != '' BEGIN
            UPDATE dbo.ChangeList SET ReviewerAlias=@ReviewerAlias WHERE Id = @ChangeId
        END
        SELECT @result
        RETURN
    END
    INSERT INTO dbo.ChangeList (SourceControlId, UserName, ReviewerAlias, UserClient, CL, Url, Title, Description, TimeStamp, Stage, CurrentReviewRevision)
        VALUES(@SourceControl, @UserName, @ReviewerAlias, @UserClient, @CL, @Url, @Title, @Description, GETDATE(), 0, @ReviewRevision)
    SET @result = @@IDENTITY
    SELECT @result
END
