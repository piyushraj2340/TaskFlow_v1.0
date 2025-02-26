
CREATE PROCEDURE [dbo].[usp_UpdateGoalState](
@UserId nvarchar(256)
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
	SET GoalStatus = case
			when EndDate < @currentDateTime and GoalStatus NOT IN (@Completed, @Ended)
				then @Ended
			when IsScheduled = 1 and StartDate < @currentDateTime and GoalStatus != @Completed
				Then @Running 
			end, 
		UpdatedOn = @currentDateTime,
		IsStarted = case 
			when IsScheduled = 1 and StartDate < @currentDateTime and GoalStatus != @Completed 
				Then 1
			end,
		StartedOn = case
			when IsScheduled = 1 and StartDate < @currentDateTime and GoalStatus != @Completed 
				Then @currentDateTime
			end,
		EndedOn = case 
			when EndDate < @currentDateTime and GoalStatus NOT IN (@Completed, @Ended)
				then @currentDateTime
			end
	WHERE UserId = @UserId
		AND IsDeleted = 0 -- Goal should not be deleted
END