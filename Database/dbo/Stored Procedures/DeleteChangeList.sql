CREATE PROCEDURE [dbo].[DeleteChangeList]
@UserName nvarchar(50), @ChangeId INT
AS
BEGIN
    EXEC dbo.MaybeAudit @UserName, @ChangeId, "DELETE"
    UPDATE dbo.ChangeList SET Stage = 3 WHERE Id = @ChangeId
END
