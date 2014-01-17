CREATE PROCEDURE [dbo].[DeleteReviewer]
@Id INT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
            DECLARE @ReviewerId INT = 0
            SELECT @ReviewerId = Id from dbo.[Reviewer] WHERE Id=@Id
            
            IF @ReviewerId != 0 BEGIN
                DECLARE @Comments INT = 0
                SELECT @Comments = COUNT(*) FROM dbo.[Comment] WHERE ReviewerId=@Id
                
                DELETE FROM dbo.[MailChangeList] Where ReviewerId = @Id
                IF @Comments = 0 BEGIN
                    DELETE FROM dbo.[Reviewer] WHERE Id = @Id
                END
                ELSE BEGIN
                    UPDATE dbo.[Reviewer] SET Status=4 WHERE Id = @Id
                END
            END
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        declare @error int, @message varchar(4000), @xstate int;
        select @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
        ROLLBACK TRANSACTION;
        raiserror ('DeleteReviewer: %d: %s', 16, 1, @error, @message)
    END CATCH;
END
