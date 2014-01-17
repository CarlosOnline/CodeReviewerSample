CREATE FUNCTION [dbo].[MailRequestTypeFromReviewerStatus]
(
    @Status INT
)
RETURNS INT
AS
    BEGIN
        IF @Status = 0 RETURN -1 -- NotLookedAtYet
        IF @Status = 1 RETURN -1 -- Looking
        IF @Status = 2 RETURN  4 -- SignedOff
        IF @Status = 3 RETURN  5 -- SignedOffWithComments
        IF @Status = 4 RETURN  6 -- WaitingOnAuthor
        IF @Status = 5 RETURN -1 -- Complete
        IF @Status = 6 RETURN -1 -- Deleted
        RETURN -1
    END
