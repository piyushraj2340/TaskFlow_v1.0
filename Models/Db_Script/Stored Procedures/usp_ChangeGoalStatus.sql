CREATE OR ALTER PROCEDURE [dbo].[usp_ChangeGoalStatus](
@UserId NVARCHAR(450),
@GoalId INT,
@StatusToUpdate INT -- The status of the goal. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
)
AS 
BEGIN 
	   -- Declare status constants
    DECLARE @NotStarted INT = 0,
            @Running INT = 1,
            @Completed INT = 2,
            @Ended INT = 3;

	DECLARE @CurrentDateTime DATETIME = GETDATE();

	DECLARE @CurrentGoalStatus INT;

	SELECT @CurrentGoalStatus = GoalStatus 
	FROM Goals 
	WHERE UserId = @UserId
		AND Id = @GoalId
		AND IsDeleted = 0
		AND EndDate > @CurrentDateTime;

	IF @CurrentGoalStatus IS NULL
	BEGIN 
		RAISERROR ('Goal Not Found! Goal Must be in Active State', 16, 1);
		RETURN;
	END 


	BEGIN TRANSACTION;

	BEGIN TRY
		IF @StatusToUpdate = @NotStarted -- Change to NotStarted
			BEGIN 
				IF @CurrentGoalStatus = @Running -- if Goal is in running state then only change allowed... 
					BEGIN 
						UPDATE Goals
						SET GoalStatus = @NotStarted, UpdatedOn = @CurrentDateTime
						WHERE Id = @GoalId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Goal must be in running state!', 16, 1);
						RETURN;
					END 
			END
		ELSE IF @StatusToUpdate = @Running -- Change to running
			BEGIN 
				IF @CurrentGoalStatus in (@NotStarted, @Completed, @Ended) -- either in complete, notstarted amd Ended state
					BEGIN 
						UPDATE g
						SET g.GoalStatus = @Running,
							g.UpdatedOn = @CurrentDateTime,
							g.IsStarted = CASE WHEN @CurrentGoalStatus = @NotStarted AND g.IsStarted = 0 THEN 1 ELSE g.IsStarted END,
							g.StartedOn = CASE WHEN @CurrentGoalStatus = @NotStarted AND g.IsStarted = 0 THEN @CurrentDateTime ELSE g.StartedOn END
						FROM Goals g
						WHERE Id = @GoalId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Goal must be in Active State!', 16, 1);
						RETURN;
					END 
			END
		ELSE IF @StatusToUpdate = @Completed -- Change to completed
			BEGIN 
				IF @CurrentGoalStatus = @Running -- if Goal is in running state then only change allowed... 
					BEGIN 
						UPDATE Goals
						SET GoalStatus = @Completed, UpdatedOn = @CurrentDateTime, CompletedOn = @CurrentDateTime
						WHERE Id = @GoalId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Goal must be in running state!', 16, 1);
						RETURN;
					END 
			END

		ELSE IF @StatusToUpdate = @Ended -- Change to Ended
			BEGIN 
				IF @CurrentGoalStatus = @Running -- if Goal is in running state then only change allowed... 
					BEGIN 
						UPDATE Goals
						SET GoalStatus = @Ended, UpdatedOn = @CurrentDateTime, EndedOn = @CurrentDateTime
						WHERE Id = @GoalId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Goal must be in running state!', 16, 1);
						RETURN;
					END 
			END
		ELSE 
			BEGIN
				RAISERROR ('Invalid Goal Status!', 16, 1);
				RETURN;
			END 

		COMMIT;
		
		-- Return success message
        PRINT 'Goal status updated successfully!';
	END TRY
	BEGIN CATCH 
		ROLLBACK;
		-- Retrieve error details
        DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        -- Raise the error
        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
	END catch
END 
GO