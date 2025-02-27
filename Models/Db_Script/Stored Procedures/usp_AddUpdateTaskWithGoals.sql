CREATE OR ALTER PROCEDURE [dbo].[usp_AddUpdateTaskWithGoals]
(
    @Name NVARCHAR(MAX),
	@StartDate DATETIME,
	@StartOptionType INT,
	@IsScheduled BIT,
    @EndDate DATETIME,
    @TaskStatus INT,
    @Description NVARCHAR(MAX),
    @Priority INT,
    @Repeat INT,
    @RepeatWeekList NVARCHAR(MAX),
    @TaskId INT,
    @GoalIds NVARCHAR(MAX), -- multiple seperated with comma "1,2,3,4" so on...
    @UserId NVARCHAR(450),
	@Mode INT -- values 
				-- 1 = Add
				-- 2 = Update
)
AS
BEGIN
-- The status of the Goals. Possible values:
                           -- 0 = NotStarted
                           -- 1 = Running
                           -- 2 = Completed
                           -- 3 = Ended
						
	DECLARE @NotStarted INT = 0;
	DECLARE @Running INT = 1;
	DECLARE @currentDateTime DATETIME = GETDATE();

	DECLARE @ListOfGoalIds TABLE (Id INT);

	IF LTRIM(RTRIM(@GoalIds)) != ''
	BEGIN
		INSERT INTO @ListOfGoalIds (Id)
		SELECT CAST(value AS INT) FROM STRING_SPLIT(@GoalIds, ',');
	END

	-- Start a transaction
    BEGIN TRANSACTION;

    BEGIN TRY
		------------------------ INSERT NEW TASK --------------------------
		IF @Mode = 1
			BEGIN 
				---------------------- Insert TASK --------------------------
				INSERT INTO Tasks(
					Name,
					StartDate,
					IsScheduled,
					StartOptionType,
					IsStarted,
					StartedOn,
					EndDate,
					CreatedOn,
					UpdatedOn,
					TaskStatus, 
					Description,
					Priority,
					Repeat,
					RepeatWeekList,
					UserId
				)
				VALUES(
					@Name, 
					@StartDate, 
					@IsScheduled,
					@StartOptionType,
					CASE WHEN @IsScheduled = 1 AND @StartDate <= @currentDateTime THEN 1 ELSE 0 END, -- IsStarted
					CASE WHEN @IsScheduled = 1 AND @StartDate <= @currentDateTime THEN @currentDateTime ELSE NULL END, --StartedOn
					@EndDate, 
					@currentDateTime, -- CreatedOn
					@currentDateTime, -- UpdatedOn
					CASE WHEN @IsScheduled = 1 AND @StartDate <= @currentDateTime THEN @Running ELSE @TaskStatus END, -- TaskStatus
					@Description,
					@Priority,
					@Repeat,
					@RepeatWeekList,
					@UserId
				);

				----------------------- UPDATE TASKID ----------------------
				SET @TaskId = SCOPE_IDENTITY();
			END
		------------------------- UPDATE EXISTING TASK ------------------------
		ELSE IF @Mode = 2
			BEGIN 
				IF EXISTS(SELECT ID FROM Tasks WHERE ID = @TaskId AND UserId = @UserId AND IsDeleted = 0)
					BEGIN 
						UPDATE Tasks
							SET 
								Name = @Name,
								EndDate = @EndDate,
								UpdatedOn = @currentDateTime,
								TaskStatus = @TaskStatus,
								Description = @Description,
								Priority = @Priority,
								Repeat = @Repeat,
								RepeatWeekList =  @RepeatWeekList
							WHERE 
								UserId = @UserId
								AND Id = @TaskId
								AND IsDeleted = 0
					END
				ELSE 
					BEGIN
						RAISERROR ('Task Data Not Found!', 16, 1);
						RETURN;
					END
			END

		-------------------- DELEATE RELATIONS BETWEEN GOAL AND TASK ---------------------------
		DELETE FROM GoalTasks
			WHERE TaskId = @TaskId
				AND UserId = @UserId
				AND GoalId NOT IN (SELECT ID FROM @ListOfGoalIds);


		--------------------- ADD NEW RELATIONS BETWEEN GOAL AND TASK ----------------------------
		INSERT INTO GoalTasks (GoalId, TaskId, UserId)
		SELECT 
			GIDs.Id AS GoalId,
			@TaskId AS TaskId,
			@UserId AS UserId
		FROM @ListOfGoalIds GIDs
		JOIN Goals gol ON gol.Id = GIds.Id
		WHERE GIDs.Id NOT IN (
			SELECT gt.GoalId FROM
				GoalTasks gt
				WHERE gt.UserId = @UserId
				AND gt.TaskId = @taskId
			)
			AND gol.GoalStatus IN (@Running, @NotStarted)
			AND gol.IsDeleted = 0
			AND gol.EndDate > GETDATE()
        
		------------------ RETURN UPDATED TASK ID ----------------------
		SELECT @TaskId AS Id;

        
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
END;
GO