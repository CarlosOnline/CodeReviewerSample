CREATE PROCEDURE [dbo].[AddAttachment]
@ChangeId INT, @Description NVARCHAR (128), @Link NVARCHAR (MAX), @result INT OUTPUT
AS
BEGIN
    DECLARE @AttachmentId int
    SET @AttachmentId = (SELECT Id FROM dbo.Attachment WHERE ChangeListId = @ChangeId AND Link = @Link)
    IF @AttachmentId IS NOT NULL
    BEGIN
        SET @result = @AttachmentId
        UPDATE dbo.Attachment SET TimeStamp = GETUTCDATE(), Description = @Description WHERE Id = @AttachmentId
        SELECT @result
        RETURN
    END
    INSERT INTO dbo.Attachment (ChangeListId, TimeStamp, Description, Link)
        VALUES(@ChangeId, GETUTCDATE(), @Description, @Link)
    SET @result = @@IDENTITY
    SELECT @result
END
