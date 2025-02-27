
CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateGoalState](
@UserId NVARCHAR(256)
)
AS
BEGIN 
-- The status of the goal. Possible values:
    -- 0 = NotStarted
    -- 1 = Running
    -- 2 = Completed
    -- 3 = Ended
	-- 4 = All

	DECLARE @currentDateTime DATETIME = GETDATE();

	DECLARE @NotStarted INT = 0; -- Completed
	DECLARE @Running INT = 1; -- Running
	DECLARE @Completed INT = 2; -- Completed
	DECLARE @Ended INT = 3;    -- Ended

	UPDATE Goals
	SET GoalStatus = CASE
			WHEN EndDate < @currentDateTime AND GoalStatus NOT IN (@Completed, @Ended)
				THEN @Ended
			WHEN IsScheduled = 1 AND StartDate < @currentDateTime AND GoalStatus != @Completed
				THEN @Running 
			END, 
		UpdatedOn = @currentDateTime,
		IsStarted = CASE 
			WHEN IsScheduled = 1 AND StartDate < @currentDateTime AND GoalStatus != @Completed 
				THEN 1
			END,
		StartedOn = CASE
			WHEN IsScheduled = 1 AND StartDate < @currentDateTime AND GoalStatus != @Completed 
				THEN @currentDateTime
			END,
		EndedOn = CASE 
			WHEN EndDate < @currentDateTime AND GoalStatus NOT IN (@Completed, @Ended)
				THEN @currentDateTime
			END
	WHERE UserId = @UserId
		AND IsDeleted = 0 -- Goal should not be deleted
END