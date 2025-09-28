CREATE OR ALTER PROCEDURE [dbo].[usp_ChangeTodoStatus]
(
	@UserId NVARCHAR(450),
	@TodoId INT,
	@StatusToUpdate INT --- The status of the todo. Possible values:
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
)

AS 
BEGIN
 -- RepeatType posible values
	--	RunOnce = 0,
	--  Daily = 1,
	--  Weekly = 2

	DECLARE @RunOnce INT = 0;

	   -- Declare status constants
    DECLARE @Running INT = 1,
            @Completed INT = 2,
            @Ended INT = 3;

	DECLARE @CurrentDateTime DATETIME = GETDATE(); 
	DECLARE @YesterdayDate DATETIME = CAST(DATEADD(DAY, -1, GETDATE()) AS DATE);

	--- Steps
	-- 1. Task status is running and not delete-> then allow the todo to changed to completed
	-- 2. Task is other than running -> complete,ended,deleted -> changed to ended...
	-- 3. todo move to running if -> task is in running and not deleted
							-- else -> Not Allowed stay in the completed...
	-- 4. Allow to change the status for today and yesterday
	-- 5. if task repeat type is runonce and we change the todo status to completed then the task status also change to the completed 
			-- but runOnce task should not be move to the running as the task is completed...
	-- 6. what if the task is completed and does the todo itself move to completed on that day or manualy move to running 
			-- if the task and todo already completed then the todo should not be allowed to change the status...?....

	-- 7. If IsManualAdded is true -> Move to running, Move to Complete allow only for today and yesterday


	DECLARE @TodoCurrentStatus iNt;
	DECLARE @TaskCurrentStatus INT;
	DECLARE @TaskEndDate DATETIME;
	DECLARE @TaskIsDeleted BIT;
	DECLARE @IsManualAdded BIT;

	SELECT
		@TodoCurrentStatus = td.Status, 
		@TaskCurrentStatus =  t.TaskStatus,
		@TaskEndDate =  t.EndDate,
		@TaskIsDeleted = t.IsDeleted,
		@IsManualAdded = td.IsManualAdded
	FROM Todo td
	LEFT JOIN Tasks t ON t.Id = td.TaskId
	WHERE td.UserId = t.UserId
	AND t.UserId = @UserId
	AND ( -- only able to change the status of todo if the task is in running or manualy added todo
        (td.IsManualAdded = 1) 
        OR
        (td.IsManualAdded = 0 AND t.TaskStatus = @Running) 
    ) AND td.IsDeleted = 0
	AND td.Id = @TodoId
	AND td.EndDate > @YesterdayDate

	IF @TodoCurrentStatus IS NULL
    BEGIN
        RAISERROR ('Todo must be in an active state!', 16, 1);
        RETURN;
    END

	DECLARE @IsTaskActive BIT = 
	CASE 
		WHEN @TaskCurrentStatus = @Running AND @TaskIsDeleted = 0 AND @TaskEndDate > @CurrentDateTime
		THEN 1 ELSE 0
	END;

	IF(@StatusToUpdate = @Completed)
	BEGIN 
		BEGIN TRANSACTION;

		BEGIN TRY
			-- UPDATE THE TASK STATUS TO COMPLETED IF THE TASK IS OF REPEATE TYPE RUNONCE
			UPDATE t
			SET t.TaskStatus = @Completed,
				t.UpdatedOn = @CurrentDateTime, 
				t.CompletedOn = @CurrentDateTime
			FROM Tasks t
			JOIN Todo td ON td.TaskId = t.Id
			WHERE td.id = @TodoId
			AND td.UserId = t.UserId
			AND t.UserId = @UserId
			AND t.Repeat = @RunOnce


			IF @IsManualAdded = 1
			BEGIN
				UPDATE Todo
				SET UpdatedOn = @CurrentDateTime,
					CompletedOn = @CurrentDateTime,
					Status = @Completed 
				WHERE ID = @TodoId AND UserId = @UserId
			END
			ELSE
			BEGIN
				UPDATE Todo
				SET UpdatedOn = @CurrentDateTime,
					CompletedOn = CASE WHEN @IsTaskActive = 1 THEN @CurrentDateTime END,
					EndedOn = CASE WHEN @IsTaskActive = 0 THEN @CurrentDateTime END,
					Status = CASE WHEN @IsTaskActive = 1 THEN @Completed ELSE @Ended END
				WHERE ID = @TodoId AND UserId = @UserId
			 END

			COMMIT TRANSACTION;
		END TRY
		BEGIN CATCH
			ROLLBACK TRANSACTION;

			RAISERROR ('Error while updating the status!', 16, 1);
			RETURN;
		END CATCH

	END
	ELSE IF(@StatusToUpdate = @Running)
	BEGIN
		IF(@IsTaskActive = 1)
		BEGIN
			UPDATE TODO	
			SET UpdatedOn = @CurrentDateTime,
				Status = @Running
			WHERE Id = @TodoId AND UserId = @UserId
		END
		ELSE 
		BEGIN
			RAISERROR ('Not allowed to change the status!', 16, 1);
            RETURN;
		END
	END
	ELSE 
	BEGIN
        RAISERROR ('Todo Change Status must be valid!', 16, 1);
        RETURN;
    END

	-- Return success message
        PRINT 'Task status updated successfully!';
END
GO