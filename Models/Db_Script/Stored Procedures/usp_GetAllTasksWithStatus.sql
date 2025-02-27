CREATE OR ALTER PROCEDURE [dbo].[usp_GetAllTasksWithStatus]
(
    @UserId NVARCHAR(450), -- The unique identifier of the user.
    @Status INT,           -- The status of the task. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
	@Mode INT              -- The mode 
					       -- 0 = Tasks - Model
						   -- 1 = TaskDTO - Model
						   -- 2 = TaskNameDTO - Model
)
AS 
BEGIN 
	SET NOCOUNT ON;

	BEGIN TRANSACTION;

	BEGIN TRY

	-- Change the status of the ended tasks as Date has passed but taskStatus has not changed....
	EXEC usp_UpdateEndedTask @UserId;
	EXEC usp_UpdateAutoStartedTask @UserId;

	IF(@Mode = 0) 
		BEGIN
			SELECT t.*
			FROM Tasks t
			WHERE t.UserId = @UserId
			AND (@Status IS NULL OR t.TaskStatus = @Status)
			AND IsDeleted = 0;
		END
	ELSE IF (@Mode = 1)
		BEGIN 
			SELECT t.Id, t.Name, t.EndDate, t.Priority, t.Repeat, t.RepeatWeekList, t.Description,  t.TaskStatus, t.UserId,  t.IsScheduled, t.StartDate, t.IsStarted, t.StartOptionType
			FROM Tasks t
			WHERE t.UserId = @UserId
			AND (@Status IS NULL OR t.TaskStatus = @Status)
			AND IsDeleted = 0;
		END
	ELSE IF (@Mode = 2)
		BEGIN
			SELECT t.Id, t.Name, t.UserId
			FROM Tasks t
			WHERE t.UserId = @UserId
			AND (@Status IS NULL OR t.TaskStatus = @Status)
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