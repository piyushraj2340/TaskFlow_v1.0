CREATE OR ALTER PROCEDURE [dbo].[usp_GetAllTasksWithStatusByGoalId] 
(
    @UserId NVARCHAR(450), -- The unique identifier of the user.
    @Status INT,           -- The status of the task. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
	@GoalId INT,
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

	-- Change the status of the ended tasks as Date has passed but taskStatus has not changed....
	EXEC usp_UpdateEndedTask @UserId;
	EXEC usp_UpdateAutoStartedTask @UserId;
	
	if(@Mode = 0)
		BEGIN 
			SELECT t.*
			FROM Tasks t
			right join GoalTasks gt ON gt.TaskId = t.id
			where gt.GoalId = @GoalId
			and (@Status = 4 or t.TaskStatus = @Status)
			and gt.UserId = t.UserId
			and t.UserId = @UserId
			and t.IsDeleted = 0
			and t.EndDate > @currentDateTime
		end
	else if(@Mode = 1)
		begin
			select t.Id, t.Name, t.EndDate, t.Priority, t.Repeat, t.RepeatWeekList, t.Description,  t.TaskStatus, t.UserId
				from Tasks t
			right join GoalTasks gt on gt.TaskId = t.id
			where gt.GoalId = @GoalId
			and (@Status = 4 or t.TaskStatus = @Status)
			and gt.UserId = t.UserId
			and t.UserId = @UserId
			and t.IsDeleted = 0
			and t.EndDate > @currentDateTime
		end
	else if(@Mode = 2)
		begin 
			select t.Id, t.Name, t.UserId
			from Tasks t
			right join GoalTasks gt on gt.TaskId = t.id
			where gt.GoalId = @GoalId
			and (@Status = 4 or t.TaskStatus = @Status)
			and gt.UserId = t.UserId
			and t.UserId = @UserId
			and t.IsDeleted = 0
			and t.EndDate > @currentDateTime
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
		RETURN;
	END CATCH
end
GO