CREATE procedure [dbo].[usp_GetAllGoalsWithStatusByTaskId] 
(
    @UserId NVARCHAR(450), -- The unique identifier of the user.
    @Status INT,            -- The status of the task. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
	@TaskId int,
	@Mode int              -- The mode 
					       -- 0 = Tasks - Model
						   -- 1 = TaskDTO - Model
						   -- 2 = TaskNameDTO - Model
)
as 
begin
SET NOCOUNT ON;

	DECLARE @currentDateTime DATETIME = GETDATE()
	BEGIN TRANSACTION;

	BEGIN TRY

	-- Change the status of the ended tasks as Date has passed but taskStatus has not changed....
	EXEC usp_UpdateEndedGoal @UserId;

	if(@Mode = 0) 
		begin 
			select g.* 
			from Goals g
			right join GoalTasks gt on gt.GoalId = g.id
			where gt.TaskId = @TaskId
			and (@Status = 4 or g.GoalStatus = @Status)
			and gt.UserId = g.UserId
			and g.UserId = @UserId
			and g.IsDeleted = 0
			and g.EndDate > @currentDateTime

		end
	else if (@Mode = 1)
		begin
			select g.Id, g.Name, g.EndDate, g.Priority, g.Description,  g.GoalStatus, g.UserId, g.IsScheduled, g.StartDate, g.IsStarted, g.StartOptionType
			from Goals g
			right join GoalTasks gt on gt.GoalId = g.id
			where gt.TaskId = @TaskId
			and (@Status = 4 or g.GoalStatus = @Status)
			and gt.UserId = g.UserId
			and g.UserId = @UserId
			and g.IsDeleted = 0
			and g.EndDate > @currentDateTime
		end
	else if (@Mode = 2)
		begin 
			select g.Id, g.Name, g.UserId
			from Goals g
			right join GoalTasks gt on gt.GoalId = g.id
			where gt.TaskId = @TaskId
			and (@Status = 4 or g.GoalStatus = @Status)
			and gt.UserId = g.UserId
			and g.UserId = @UserId
			and g.IsDeleted = 0
			and g.EndDate > @currentDateTime
		end
	else 
		begin
			RAISERROR ('Invalid @Mode To Access Data From usp_GetAllTasksWithStatusByGoalId', 16, 1);
			RETURN;
		end
	
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
		return;
	END CATCH
end
GO