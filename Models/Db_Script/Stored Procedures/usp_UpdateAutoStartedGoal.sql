CREATE   PROCEDURE [dbo].[usp_UpdateAutoStartedGoal](
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

	DECLARE @NotStartedStatus INT = 0; -- Completed
	DECLARE @RunningStatus INT = 1; -- Running

	UPDATE Goals
	SET GoalStatus = @RunningStatus, -- Change the goal status to 'Running'
		UpdatedOn = @currentDateTime,
		IsStarted = 1,
		StartedOn = @currentDateTime
	WHERE UserId = @UserId
		AND IsScheduled = 1 
		AND StartDate < @currentDateTime
		AND GoalStatus = @NotStartedStatus
		AND IsDeleted = 0 -- Goal should not be deleted
END
GO
/****** Object:  StoredProcedure [dbo].[usp_UpdateEndedGoal]    Script Date: 26-02-2025 06:00:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[usp_UpdateEndedGoal](
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