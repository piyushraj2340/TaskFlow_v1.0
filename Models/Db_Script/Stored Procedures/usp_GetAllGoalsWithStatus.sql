CREATE OR ALTER PROCEDURE [dbo].[usp_GetAllGoalsWithStatus]
(
    @UserId NVARCHAR(450), -- The unique identifier of the user.
    @Status INT,           -- The status of the goal. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
						   -- 4 = All
	@Mode INT              -- The mode 
					       -- 0 = Goals - Model
						   -- 1 = GoalDTO - Model
						   -- 2 = GoalNameDTO - Model
)
AS 
BEGIN 
	SET NOCOUNT ON;

	BEGIN TRANSACTION;

	BEGIN TRY

	-- Change the status of the ended goals as Date has passed but goalStatus has not changed....
	EXEC usp_UpdateEndedGoal @UserId;
	EXEC usp_UpdateAutoStartedGoal @UserId;


	if(@Mode = 0)
		BEGIN 
			SELECT g.*
			FROM Goals g
			WHERE g.UserId = @UserId 
			AND (@Status = 4 OR g.GoalStatus = @Status)
			AND IsDeleted = 0;
		END
	ELSE IF(@Mode = 1)
		BEGIN
			SELECT g.Id, g.Name, g.EndDate, g.Priority, g.Description,  g.GoalStatus, g.UserId, g.IsScheduled, g.StartDate, g.IsStarted, g.StartOptionType
			FROM Goals g
			WHERE g.UserId = @UserId 
			AND (@Status = 4 OR g.GoalStatus = @Status)
			AND IsDeleted = 0;
		END
	ELSE IF(@Mode = 2)
		BEGIN 
			SELECT g.Id, g.Name, g.UserId
			FROM Goals g
			WHERE g.UserId = @UserId 
			AND (@Status = 4 OR g.GoalStatus = @Status)
			AND IsDeleted = 0;
		END
	ELSE 
		BEGIN 
			RAISERROR ('Invalid @Mode To Access Data From usp_GetAllTasksWithStatusByGoalId', 16, 1);
			RETURN;
		END

		COMMIT;
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
		RETURN;
	END CATCH

END
GO