CREATE OR ALTER PROCEDURE usp_UpdateAutoStartedTask
(
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

	DECLARE @NotStartedStatus INT = 0; -- Completed
	DECLARE @RunningStatus INT = 1; -- Running

	UPDATE Tasks
	SET TaskStatus = @RunningStatus, -- Change the goal status to 'Running'
		UpdatedOn = @currentDateTime,
		IsStarted = 1,
		StartedOn = @currentDateTime
	WHERE UserId = @UserId
		AND IsScheduled = 1 
		AND StartDate < @currentDateTime
		AND TaskStatus = @NotStartedStatus
		AND IsDeleted = 0 -- Goal should not be deleted
END