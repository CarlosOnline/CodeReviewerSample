CREATE PROCEDURE [dbo].[AddCommentGroup]
@GroupId INT, @FileVersionId INT, @Line INT, @LineStamp nvarchar(200), @Status INT, @result INT OUTPUT
AS
BEGIN
    DECLARE @FileId int
    SET @FileId = (SELECT FileId FROM dbo.FileVersion WHERE Id = @FileVersionId)

    DECLARE @ChangeListId int
    SET @ChangeListId = (SELECT ChangeListId FROM dbo.ChangeFile WHERE Id = @FileId)

    DECLARE @ReviewId int
    SET @ReviewId = (SELECT Id FROM dbo.Review WHERE ChangeListId = @ChangeListId)

    IF @GroupId = 0 OR @GroupId IS NULL
    BEGIN
        SET @GroupId = (SELECT Id FROM dbo.CommentGroup
        WHERE [ReviewId] = @ReviewId AND FileVersionId = @FileVersionId AND Line = @Line AND LineStamp = @LineStamp)
    END

    -- UPDATE EXISTING COMMENT GROUP
    IF @GroupId IS NOT NULL
    BEGIN
        UPDATE dbo.CommentGroup SET Status = @Status WHERE Id = @GroupId
        SET @result = @GroupId
    END
    ELSE
    BEGIN
        INSERT INTO dbo.CommentGroup (ReviewId, ChangeListId, FileId, FileVersionId, Line, LineStamp, Status)
            VALUES(@ReviewId, @ChangeListId, @FileId, @FileVersionId, @Line, @LineStamp, @Status)
        SET @result = @@IDENTITY
    END

    SELECT @result
END

/****** Object:  StoredProcedure [dbo].[EditCommentGroup]    ******/
SET ANSI_NULLS ON
