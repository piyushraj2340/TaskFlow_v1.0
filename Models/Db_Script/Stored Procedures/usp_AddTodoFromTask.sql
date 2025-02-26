CREATE OR ALTER PROCEDURE [dbo].[usp_AddTodoFromTask]
	@UserId NVARCHAR(450)
AS 
BEGIN

 -- RepeatType posible values
	--	RunOnce = 0,
	--  Daily = 1,
	--  Weekly = 2

-- The status of the todo. Possible values:
	-- 0 = NotStarted
	-- 1 = Running
	-- 2 = Completed
	-- 3 = Ended
	-- 4 = All
DECLARE @RunningStatus INT = 1; -- status as running

DECLARE @RepeateDaily INT = 1; -- Repeating daily
DECLARE @RepeateWeekly INT = 2;

DECLARE @Today DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);  -- Start of today
DECLARE @CurrentDateTime DATETIME = GETDATE();
DECLARE @Tomorrow DATETIME = DATEADD(MINUTE,-1,DATEADD(DAY, 1, @Today));  -- Start of tomorrow
DECLARE @CurrentWeekday INT = (DATEPART(WEEKDAY, GETDATE()) + 5) % 7; -- Adjusted weekday (Mon = 0, Sun = 6)

BEGIN TRANSACTION;

	BEGIN TRY
		;WITH task AS (
			SELECT t.Id 
			FROM Tasks t
			WHERE t.UserId = @UserId
				And t.TaskStatus = @RunningStatus -- runnig status 
				AND (
					t.Repeat = @RepeateDaily -- repeate daily
					OR (t.Repeat = @RepeateWeekly -- repeate weekly
						AND t.RepeatWeekList 
						LIKE '%' + CAST(@CurrentWeekday AS VARCHAR) + '%'
						)
				)
				AND t.IsDeleted = 0
				AND t.id NOT IN (
					select td.TaskId from Todo td
					where
					td.CreatedOn >= @Today
					AND td.CreatedOn < @Tomorrow
					AND td.UserId = @UserId
					AND td.IsDeleted = 0
				)
		) 

		-- Insert tasks INTo Todo for the user
		INSERT INTO Todo(EndDate, Status, CreatedOn, UpdatedOn, TaskId, UserId)
		SELECT @Tomorrow, 1, @CurrentDateTime, @CurrentDateTime, t.Id, @UserId
		FROM task t; -- Insert each taskId from the CTE


		-- Return success message
		PRINT 'Todo Added successfully!';

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
