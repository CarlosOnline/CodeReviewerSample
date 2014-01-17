CREATE PROCEDURE [dbo].[AddReviewer]
@Id INT, @UserName NVARCHAR (200), @ReviewerAlias NVARCHAR (200), @ChangeListId INT, @Status INT, @RequestType INT, @result INT OUTPUT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @ReviewerId int
        SET @ReviewerId = (SELECT Id FROM dbo.Reviewer WHERE Id = @Id)
        IF @ReviewerId IS NULL OR @ReviewerId = 0 BEGIN
            SET @ReviewerId = (SELECT Id FROM dbo.Reviewer
                WHERE ChangeListId = @ChangeListId AND (UserName = @UserName OR ReviewerAlias = @ReviewerAlias))
        END
        IF @ReviewerId IS NOT NULL
        BEGIN
            -- Handle SignedOffWithComments case
            IF @Status IN (2, 3) BEGIN
                SET @Status = 2
                
                DECLARE @ActiveComments INT = 0
                SELECT @ActiveComments = COUNT(*) 
                FROM [dbo].Comment 
                WHERE ReviewerId = @ReviewerId
                
                IF @ActiveComments > 0
                    SET @Status = 3 -- SignedOffWithComments
            END

            -- Update Reviewer Status
            UPDATE dbo.Reviewer
            SET [Status]=@Status
            WHERE Id = @ReviewerId
        
            -- Add mail request
            IF @RequestType = -1 AND @Status IN (2, 3, 4)
                SET @RequestType = dbo.MailRequestTypeFromReviewerStatus(@Status)
            
            IF @RequestType != -1 BEGIN
                INSERT INTO dbo.MailChangeList (ChangeListId, ReviewerId, [RequestType])
                VALUES(@ChangeListId, @ReviewerId, @RequestType)
            END
            
            SET @result = @ReviewerId
            SELECT @result
            COMMIT TRANSACTION;
            RETURN
        END
        INSERT INTO dbo.Reviewer (UserName, ReviewerAlias, ChangeListId, [Status])
            VALUES(@UserName, @ReviewerAlias, @ChangeListId, @Status)
        SET @result = @@IDENTITY
        SET @ReviewerId = @result

        -- Add mail request
        INSERT INTO dbo.MailChangeList (ChangeListId, ReviewerId, [RequestType])
        VALUES(@ChangeListId, @ReviewerId, @RequestType)

        COMMIT TRANSACTION;
        SELECT @result
    END TRY
    BEGIN CATCH
        declare @error int, @message varchar(4000), @xstate int;
        select @error = ERROR_NUMBER(), @message = ERROR_MESSAGE(), @xstate = XACT_STATE();
        ROLLBACK TRANSACTION;
        raiserror ('AddReviewer: %d: %s', 16, 1, @error, @message)
    END CATCH;
END
