CREATE PROCEDURE [dbo].[RemoveFile]
@FileId INT
AS
BEGIN
    UPDATE dbo.ChangeFile SET IsActive = 0 WHERE Id = @FileId
END
