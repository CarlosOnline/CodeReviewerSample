CREATE PROCEDURE [dbo].[ReopenChangeList]
@UserName nvarchar(50), @ChangeId INT
AS
BEGIN
    EXEC dbo.MaybeAudit @UserName, @ChangeId, "REOPEN"
    UPDATE dbo.ChangeList SET Stage = 0 WHERE Id = @ChangeId
END
