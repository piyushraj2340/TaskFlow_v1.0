CREATE OR ALTER PROCEDURE [dbo].[usp_ChangeTaskStatus](
@UserId NVARCHAR(450),
@TaskId INT,
@StatusToUpdate INT -- The status of the task. Possible values:
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

	DECLARE @CurrentTaskStatus INT;

	SELECT @CurrentTaskStatus = TaskStatus 
	FROM Tasks 
	WHERE UserId = @UserId
		AND Id = @TaskId
		AND IsDeleted = 0
		AND EndDate > @CurrentDateTime;

	IF(@CurrentTaskStatus IS NULL) 
	BEGIN 
		RAISERROR ('Task Not Found! Task Must be in Active State', 16, 1);
		RETURN;
	END 


	BEGIN TRANSACTION;

	BEGIN TRY
		IF(@StatusToUpdate = @NotStarted) -- Change to NotStarted
			BEGIN 
				IF(@CurrentTaskStatus = @Running) -- IF Task is in running state then only change allowed... 
					BEGIN 
						update Tasks
						SET TaskStatus = @NotStarted, UpdatedOn = @CurrentDateTime
						WHERE Id = @TaskId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Task must be in running state!', 16, 1);
						RETURN;
					END 
			END
		ELSE IF(@StatusToUpdate = @Running) -- Change to running
			BEGIN 
				IF(@CurrentTaskStatus IN (@NotStarted, @Completed, @Ended)) -- either in complete, notstarted amd Ended state
					BEGIN 
						update t
						SET t.TaskStatus = @Running, 
							t.UpdatedOn = @CurrentDateTime,
							t.IsStarted = CASE WHEN @CurrentTaskStatus = @NotStarted AND t.IsStarted = 0 THEN 1 ELSE t.IsStarted END,
							t.StartedOn = CASE WHEN @CurrentTaskStatus = @NotStarted AND t.IsStarted = 0 THEN @CurrentDateTime ELSE t.StartedOn END
						FROM Tasks t
						WHERE Id = @TaskId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Task must be in Active State!', 16, 1);
						RETURN;
					END 
			END
		ELSE IF(@StatusToUpdate = @Completed) -- Change to completed
			BEGIN 
				IF(@CurrentTaskStatus = @Running) -- IF Task is in running state then only change allowed... 
					BEGIN 
						update Tasks
						SET TaskStatus = @Completed, UpdatedOn = @CurrentDateTime, CompletedOn = @CurrentDateTime
						WHERE Id = @TaskId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Task must be in running state!', 16, 1);
						RETURN;
					END 
			END

		ELSE IF(@StatusToUpdate = @Ended) -- Change to ENDed
			BEGIN 
				IF(@CurrentTaskStatus = @Running) -- IF Task is in running state then only change allowed... 
					BEGIN 
						update Tasks
						SET TaskStatus = @Ended, UpdatedOn = @CurrentDateTime, EndedOn = @CurrentDateTime
						WHERE Id = @TaskId
						AND UserId = @UserId
					END
				ELSE 
					BEGIN
						RAISERROR ('Task must be in running state!', 16, 1);
						RETURN;
					END 
			END
		ELSE 
			BEGIN
				RAISERROR ('Invalid Task Status!', 16, 1);
				RETURN;
			END 

		COMMIT;
		
		-- Return success message
        PRINT 'Task status updated successfully!';
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
	END CATCH
END 
GO