CREATE OR ALTER PROCEDURE [dbo].[usp_TodoProgressAnalyses]
(
	@UserId NVARCHAR(450),
	@Mode INT	-- The mode 
					-- 0 = TodoProgressAnalyses - Model
					-- 1 = TodoProgressDTO - Model
)
AS 
BEGIN
			-- The status of the Todos. Possible values:
                -- 0 = NotStarted
                -- 1 = Running
                -- 2 = Completed
                -- 3 = Ended
				-- 4 = ALL

	DECLARE @completedStatus INT = 2; -- completed staus
	DECLARE @today datetime = CAST(GETDATE() AS DATE);
	DECLARE @id INT;
	DECLARE @totalTodos INT;
	DECLARE @completedTodos INT;
	DECLARE @productivityForTodays FLOAT;

	BEGIN TRANSACTION;

	BEGIN TRY
	    ------------- Add Todo if not added ------------------------------------

		Exec usp_AddTodoFromTask @UserId

		------------- Add Todo if not added ------------------------------------


		------------- Checking if the already todo or not -----------------------
		SELECT TOP 1 
		@id = id 
		FROM TodoProgressAnalyses
		WHERE UserId = @UserId 
		AND CAST(CalculateDateFor AS DATE) = @today

		--------------------- Fetching the Current data -------------------------------
		;WITH TodaysTodo AS (
			SELECT COUNT(*) AS TotalTodos, 
				SUM(CASE 
					WHEN STATUS = @completedStatus 
						THEN 1 
						ELSE 0 
					END) AS CompletedTodos		
			FROM Todo 
			WHERE UserId = @UserId
				AND CAST(CreatedOn AS date) = @today
				AND IsDeleted = 0
		)
		SELECT @totalTodos = t.TotalTodos, @completedTodos = t.CompletedTodos 
		FROM TodaysTodo t

		------------- Returing data if the todo count is zero -------------------
		
		if(@totalTodos > 0)
			BEGIN
				SELECT @productivityForTodays = CAST(((CAST(@completedTodos AS FLOAT)/@totalTodos) * 100) AS FLOAT)
			END
		ELSE 
			BEGIN 
				SET @productivityForTodays = 0
				SET @completedTodos = 0
				SET @totalTodos = 0
			END

		if(@id IS NULL) -- insert the data....
			BEGIN
				INSERT INTO TodoProgressAnalyses(CreatedOn, UpdatedOn, IsDeleted, DeletedOn, CalculateDateFor, TotalTodo, TotalCompletedTodo, TotalMissedTodo, ProductivityForDay, UserId)
				VALUES(GETDATE(), GETDATE(), 0, NULL, @today, @totalTodos, @completedTodos, (@totalTodos - @completedTodos), @productivityForTodays, @UserId)

				-- Get the SCOPE_IDENTITY() for the inserted task
				SET @id = SCOPE_IDENTITY();
			END
		ELSE -- UPDATE...
			BEGIN 
				UPDATE TodoProgressAnalyses
				SET TotalTodo = @totalTodos, TotalCompletedTodo = @completedTodos, TotalMissedTodo = (@totalTodos - @completedTodos), ProductivityForDay = @productivityForTodays, UpdatedOn = GETDATE()
				WHERE Id = @id
			END

		----------- Returing Data ---------------
		
		if(@Mode = 1)
			BEGIN 
				SELECT * FROM TodoProgressAnalyses
				WHERE Id = @id
				AND UserId = @UserId
				AND IsDeleted = 0
			END
		ELSE if(@Mode = 2)
			BEGIN
				SELECT 
					tpa.id,
					tpa.CalculateDateFor,
					tpa.TotalTodo,
					tpa.TotalCompletedTodo,
					tpa.TotalMissedTodo,
					tpa.ProductivityForDay,
					tpa.UserId
					FROM TodoProgressAnalyses tpa
					WHERE Id = @id
					AND UserId = @UserId
					AND IsDeleted = 0
			END

		ELSE 
			BEGIN 
				RAISERROR ('Invalid @Mode To Access Data From usp_TodoProgressAnalyses', 16, 1);
				RETURN;
			END	

		----------- Returing Data ---------------

		COMMIT; -- SAVING ALL THE CHANGES.....
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
END

