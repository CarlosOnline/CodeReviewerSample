CREATE PROCEDURE [dbo].[AddVersion]
@FileId INT, @Revision INT, @ReviewRevision INT, @Action INT, @TimeStamp DATETIME, @IsText BIT, @IsFullText BIT, @IsRevisionBase BIT, @Text VARCHAR (MAX), @result INT OUTPUT
AS
BEGIN
    DECLARE @VersionId int
    SET @VersionId = (SELECT Id FROM dbo.FileVersion
        WHERE FileId = @FileId AND Revision = @Revision AND ReviewRevision = @ReviewRevision AND Action = @Action
            AND IsText = @IsText AND IsFullText = @IsFullText AND IsRevisionBase = @IsRevisionBase
            AND (@TimeStamp IS NULL AND TimeStamp IS NULL OR TimeStamp = @TimeStamp)
            AND (@Text IS NULL AND Text IS NULL OR Text = @Text))

    IF @VersionId IS NOT NULL
    BEGIN
       SET @result = @VersionId
       SELECT @result
       RETURN
    END
    INSERT INTO dbo.FileVersion (FileId, Revision, ReviewRevision, [Action], [TimeStamp], IsText, IsFullText, IsRevisionBase, [Text])
        VALUES(@FileId, @Revision, @ReviewRevision, @Action, @TimeStamp, @IsText, @IsFullText, @IsRevisionBase, @Text)

    -- RESET WaitingOnAuthor(s)
    DECLARE @ChangeListId int
    SET @ChangeListId = (SELECT ChangeListId FROM dbo.ChangeFile WHERE Id = @FileId)
        
    UPDATE dbo.Reviewer 
    SET Status = 0 
    WHERE ChangeListId = @ChangeListId 
    AND Status = 4
    
    SET @result = @@IDENTITY
    SELECT @result
END
