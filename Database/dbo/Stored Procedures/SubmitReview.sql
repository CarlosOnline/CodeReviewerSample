CREATE PROCEDURE [dbo].[SubmitReview]
@ReviewId INT
AS
BEGIN
    UPDATE dbo.Review SET [IsSubmitted] = 1, [TimeStamp] = GETUTCDATE() WHERE Id = @ReviewId
    INSERT INTO dbo.MailReview (ReviewId) VALUES(@ReviewId)
END
