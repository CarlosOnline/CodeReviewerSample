CREATE PROCEDURE [dbo].[AddMailRequest]
@Id INT, @ReviewerAlias NVARCHAR (256), @ChangeListId INT, @RequestType INT, @result INT OUTPUT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @ReviewerId int
        SET @ReviewerId = (SELECT Id FROM dbo.Reviewer WHERE Id = @Id)
        IF @ReviewerId IS NULL OR @ReviewerId = 0 BEGIN
            SET @ReviewerId = (SELECT Id FROM dbo.Reviewer
                WHERE ChangeListId = @ChangeListId AND ReviewerAlias = @ReviewerAlias)
        END
        IF @ReviewerId IS NULL OR @ReviewerId = 0 BEGIN
            SET @result = 0
            SELECT @result
            ROLLBACK TRANSACTION;
            RETURN
        END
        INSERT INTO dbo.MailChangeList (ChangeListId, ReviewerId, [RequestType])
        VALUES(@ChangeListId, @ReviewerId, @RequestType)

        SET @result = @@IDENTITY
        COMMIT TRANSACTION;
        SELECT @result
    END TRY
    BEGIN CATCH
        declare @error int, @message varchar(4000), @xstate int;
        select @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
        ROLLBACK TRANSACTION;
        raiserror ('AddMailRequest: %d: %s', 16, 1, @error, @message)
    END CATCH;
END
