CREATE OR ALTER PROCEDURE [dbo].[usp_GetGoalById] (
	@UserId NVARCHAR(450),
	@GoalId INT,
	@Mode INT			-- The mode 
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

		IF(@Mode = 0)
			BEGIN
				SELECT g.*
				FROM Goals g
				WHERE g.UserId = @UserId AND g.Id = @GoalId AND IsDeleted = 0;
			END
		ELSE IF(@Mode = 1)
			BEGIN
				SELECT g.Id, g.Name, g.EndDate, g.Priority, g.Description,  g.GoalStatus, g.UserId, g.IsScheduled, g.StartDate, g.IsStarted, g.StartOptionType
				FROM Goals g
				WHERE g.UserId = @UserId AND g.Id = @GoalId AND IsDeleted = 0;
			END
		ELSE IF(@Mode = 2)
			BEGIN 
				SELECT g.Id, g.Name, g.UserId, g.GoalStatus
				FROM Goals g
				WHERE g.UserId = @UserId AND g.Id = @GoalId AND IsDeleted = 0;
			END
		ELSE 
			BEGIN 
				RAISERROR ('Invalid @Mode To Access Data From usp_GetGoalById', 16, 1);
				RETURN;
			END
		COMMIT; -- save all changes...
	END TRY
	BEGIN CATCH 
		ROLLBACK; -- withdraw all the changes...

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
END;
GO