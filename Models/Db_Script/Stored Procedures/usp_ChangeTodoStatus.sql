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


	DECLARE @TodoCurrentStatus iNt;
	DECLARE @TaskCurrentStatus INT;
	DECLARE @TaskEndDate DATETIME;
	DECLARE @TaskIsDeleted BIT;

	SELECT @TodoCurrentStatus = td.Status, @TaskCurrentStatus =  t.TaskStatus, @TaskEndDate =  t.EndDate, @TaskIsDeleted = t.IsDeleted
	FROM Todo td
	LEFT JOIN Tasks t ON t.Id = td.TaskId
	WHERE td.UserId = t.UserId
	AND t.UserId = @UserId
	AND td.IsDeleted = 0
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
		UPDATE Todo
		SET UpdatedOn = @CurrentDateTime,
			CompletedOn = CASE WHEN @IsTaskActive = 1 THEN @CurrentDateTime END,
			EndedOn = CASE WHEN @IsTaskActive = 0 THEN @CurrentDateTime END,
			Status = CASE WHEN @IsTaskActive = 1 THEN @Completed ELSE @Ended END
		WHERE ID = @TodoId AND UserId = @UserId
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