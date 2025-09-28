
CREATE OR ALTER PROCEDURE [dbo].[usp_AddAndGetTodoFromTask]
	@UserId NVARCHAR(450),
	@Status INT, -- The status of the todo. Possible values:
					-- 0 = NotStarted
					-- 1 = Running
					-- 2 = Completed
					-- 3 = Ended
					-- 4 = All
	@Mode INT    -- The mode 
					-- 0 = todos - Model
					-- 1 = todoDTO - Model
AS 
BEGIN

 -- RepeatType posible values
	--	RunOnce = 0,
	--  Daily = 1,
	--  Weekly = 2

DECLARE @ModelMode INT = 0;
DECLARE @ModelDTOMode INT = 1;

DECLARE @RunningStatus INT = 1; -- status as running
DECLARE @AllStatus INT = 4; -- All status

DECLARE @RepeateDaily INT = 1; -- Repeating daily
DECLARE @RepeateWeekly INT = 2;

DECLARE @Today DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);  -- Start of today
DECLARE @Tomorrow DATETIME = DATEADD(MINUTE,-1,DATEADD(DAY, 1, @Today));  -- Start of tomorrow

BEGIN TRANSACTION;

	BEGIN TRY
		IF @Status = @RunningStatus
		BEGIN
			Exec usp_AddTodoFromTask @UserId
		END

		IF @Mode = @ModelMode -- 0 return all the data....
			BEGIN 
			 -- Select and return the newly created Todo records
				SELECT td.Id,
					td.EndDate,
					td.Status,
					td.CreatedOn, 
					td.UpdatedOn, 
					td.DeletedOn, 
					td.TaskId,
					td.UserId,
					td.Notes,
					td.IsManualAdded,
					t.Name AS TaskName,
					t.EndDate AS TaskEndDate,
					t.CreatedOn AS TaskCreatedOn, 
					t.UpdatedOn AS TaskUpdatedOn, 
					t.DeletedOn AS TaskDeletedOn, 
					t.TaskStatus AS TaskStatus, 
					t.Description AS TaskDescription,
					t.Priority AS TaskPriority,
					t.Repeat AS TaskRepeat,
					t.RepeatWeekList As TaskRepeatWeekList
				FROM Todo td
				JOIN Tasks t ON td.TaskId = t.Id
				WHERE td.UserId = @UserId
					  AND td.CreatedOn >= @Today
					  AND td.CreatedOn < @Tomorrow
					  AND (@Status = @AllStatus OR td.Status = @Status)
			END
		ELSE IF @Mode = @ModelDTOMode
			BEGIN
			 -- Select and return the newly created Todo records
				SELECT td.Id,
					td.EndDate,
					td.Status,
					td.TaskId,
					td.UserId,
					td.Notes,
					td.IsManualAdded,
					t.Name AS TaskName,
					t.EndDate AS TaskEndDate,
					t.TaskStatus AS TaskStatus, 
					t.Description AS TaskDescription,
					t.Priority AS TaskPriority,
					t.Repeat AS TaskRepeat,
					t.RepeatWeekList As TaskRepeatWeekList,
					t.CompletedOn as TaskCompletedOn,
					t.EndedOn as TaskEndedOn
				FROM Todo td
				JOIN Tasks t ON td.TaskId = t.Id
				WHERE td.UserId = @UserId
					  AND td.CreatedOn >= @Today
					  AND td.CreatedOn < @Tomorrow
					  AND (@Status = @AllStatus OR td.Status = @Status)
			END
		ELSE 
			BEGIN
				RAISERROR ('Invalid @Mode To Access Data From usp_AddTodoFromTask', 16, 1);
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

