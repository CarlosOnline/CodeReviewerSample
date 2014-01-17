CREATE PROCEDURE [dbo].[RenameChangeList]
@UserName nvarchar(50), @ChangeId INT, @NewCL NVARCHAR (128)
AS
BEGIN
    EXEC dbo.MaybeAudit @UserName, @ChangeId, "RENAME"
    UPDATE dbo.ChangeList SET CL = @NewCL WHERE Id = @ChangeId
END
