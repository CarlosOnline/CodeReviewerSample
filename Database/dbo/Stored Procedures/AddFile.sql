CREATE PROCEDURE [dbo].[AddFile]
    @ChangeId INT, 
    @LocalFile NVARCHAR (512), 
    @ServerFile NVARCHAR (512), 
    @ReviewRevision INT, 
    @result INT OUTPUT
AS
BEGIN
    DECLARE @FileId int
    SET @FileId = (SELECT Id FROM dbo.ChangeFile
        WHERE ChangeListId = @ChangeId AND ServerFileName = @ServerFile AND IsActive = 1)
    IF @FileId IS NOT NULL
    BEGIN
        SET @result = @FileId
        SELECT @result AS [Result]
        RETURN
    END
    INSERT INTO dbo.ChangeFile (ChangeListId, LocalFileName, ServerFileName, IsActive, ReviewRevision)
        VALUES(@ChangeId, @LocalFile, @ServerFile, 1, @ReviewRevision)
    SET @result = @@IDENTITY
    SELECT @result AS [Result]
END
