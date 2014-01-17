CREATE PROCEDURE [dbo].[MaybeAudit]
@UserName nvarchar(50), @ChangeId INT, @Action NVARCHAR (50), @Description NVARCHAR (MAX)=NULL
AS
BEGIN
    DECLARE @ClUserName nvarchar(50) = (SELECT UserName FROM dbo.ChangeList WHERE Id = @ChangeId)

    IF @UserName != @ClUserName
    BEGIN
        INSERT INTO dbo.AuditRecord (TimeStamp, UserName, ChangeListId, Action, Description)
            VALUES(GETUTCDATE(), @UserName, @ChangeId, @Action, @Description)
    END
END
