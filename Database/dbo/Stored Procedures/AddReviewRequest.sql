CREATE PROCEDURE [dbo].[AddReviewRequest]
@ChangeListId INT, @ReviewerAlias NVARCHAR (50)
AS
BEGIN
    INSERT INTO dbo.MailReviewRequest (ChangeListId, ReviewerAlias) VALUES(@ChangeListId, @ReviewerAlias)
END
