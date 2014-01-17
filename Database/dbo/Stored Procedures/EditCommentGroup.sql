CREATE PROCEDURE [dbo].[EditCommentGroup]
@GroupId INT, @Status INT, @result INT OUTPUT
AS
BEGIN
    UPDATE dbo.CommentGroup SET Status = @Status WHERE Id = @GroupId
    SET @result = @@ROWCOUNT
END
/****** Object:  StoredProcedure [dbo].[DeleteCommentGroup]    ******/
SET ANSI_NULLS ON
