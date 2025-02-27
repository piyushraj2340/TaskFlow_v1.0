CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateEndedGoal](
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

	DECLARE @Completed INT = 2; -- Completed
	DECLARE @Ended INT = 3;    -- Ended

	-- Change the status of the ended goals as Date has passed but goalStatus has not changed....
	UPDATE Goals
	SET GoalStatus = @Ended, -- Change the goal status to 'Ended'
		UpdatedOn = @currentDateTime,
		EndedOn = @currentDateTime
	WHERE UserId = @UserId
			AND EndDate < @currentDateTime -- EndDate as datetime must be in the past
			AND GoalStatus NOT IN (@Completed, @Ended) -- Goal must be either running or not started
			AND IsDeleted = 0 -- Goal should not be deleted
END
GO