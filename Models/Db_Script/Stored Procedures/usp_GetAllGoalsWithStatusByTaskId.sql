CREATE OR ALTER PROCEDURE [dbo].[usp_GetAllGoalsWithStatusByTaskId] 
(
    @UserId NVARCHAR(450), -- The unique identifier of the user.
    @Status INT,            -- The status of the task. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
	@TaskId INT,
	@Mode INT              -- The mode 
					       -- 0 = Tasks - Model
						   -- 1 = TaskDTO - Model
						   -- 2 = TaskNameDTO - Model
)
AS 
BEGIN
SET NOCOUNT ON;

	DECLARE @currentDateTime DATETIME = GETDATE()
	BEGIN TRANSACTION;

	BEGIN TRY

	-- Change the status of the ENDed tasks as Date has passed but taskStatus has not changed....
	EXEC usp_UpdateEndedGoal @UserId;
	EXEC usp_UpdateAutoStartedGoal @UserId;

	if(@Mode = 0) 
		BEGIN 
			SELECT g.* 
			FROM Goals g
			RIGHT JOIN GoalTasks gt ON gt.GoalId = g.id
			WHERE gt.TaskId = @TaskId
			AND (@Status = 4 OR g.GoalStatus = @Status)
			AND gt.UserId = g.UserId
			AND g.UserId = @UserId
			AND g.IsDeleted = 0
			AND g.EndDate > @currentDateTime

		END
	ELSE IF (@Mode = 1)
		BEGIN
			SELECT g.Id, g.Name, g.EndDate, g.Priority, g.Description,  g.GoalStatus, g.UserId, g.IsScheduled, g.StartDate, g.IsStarted, g.StartOptionType
			FROM Goals g
			RIGHT JOIN GoalTasks gt ON gt.GoalId = g.id
			WHERE gt.TaskId = @TaskId
			AND (@Status = 4 OR g.GoalStatus = @Status)
			AND gt.UserId = g.UserId
			AND g.UserId = @UserId
			AND g.IsDeleted = 0
			AND g.EndDate > @currentDateTime
		END
	ELSE IF (@Mode = 2)
		BEGIN 
			SELECT g.Id, g.Name, g.UserId, g.GoalStatus
			FROM Goals g
			RIGHT JOIN GoalTasks gt ON gt.GoalId = g.id
			WHERE gt.TaskId = @TaskId
			AND (@Status = 4 OR g.GoalStatus = @Status)
			AND gt.UserId = g.UserId
			AND g.UserId = @UserId
			AND g.IsDeleted = 0
			AND g.EndDate > @currentDateTime
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