CREATE PROCEDURE [dbo].[SetChangeListStatus]
@Id INT, @Status INT
AS
BEGIN
    DECLARE @ChangeId int
    SET @ChangeId = (SELECT Id FROM dbo.ChangeList WHERE Id=@Id)
    IF @ChangeId IS NOT NULL BEGIN
        UPDATE dbo.ChangeList SET Stage = @Status WHERE Id = @ChangeId
    END
    ELSE BEGIN
        raiserror ('SetChangeListStatus: invalid change list Id [%d]', 16, 1, @Id)
    END
END
