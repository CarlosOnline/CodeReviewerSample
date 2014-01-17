CREATE PROCEDURE [dbo].[DeleteCommentGroup]
@GroupId INT
AS
BEGIN
    DELETE FROM dbo.Comment WHERE GroupId = @GroupId
    DELETE FROM dbo.CommentGroup WHERE Id = @GroupId
END
