CREATE PROCEDURE [dbo].[SubmitChangeList]
@UserName nvarchar(50), @ChangeId INT
AS
BEGIN
    EXEC dbo.MaybeAudit @UserName, @ChangeId, "CLOSE"
    UPDATE dbo.ChangeList SET Stage = 2 WHERE Id = @ChangeId
END
