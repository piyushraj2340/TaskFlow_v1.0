CREATE OR ALTER PROCEDURE [dbo].[usp_UpdateEndedTask](
@UserId NVARCHAR(256)
)
AS
BEGIN 

	DECLARE @TaskStatusCompleted INT = 2; -- Completed
	DECLARE @TaskStatusEnded INT = 3;    -- Ended

	-- Change the status of the ended goals as Date has passed but goalStatus has not changed....
	;WITH TaskToUpdate AS (
		SELECT Id, EndDate 
		FROM Tasks
		WHERE UserId = @UserId
			AND EndDate < GETDATE() -- EndDate as datetime must be in the past
			AND TaskStatus NOT IN (@TaskStatusCompleted, @TaskStatusEnded) -- Task must be either running or not started
			AND IsDeleted = 0 -- Task should not be deleted
	)
	UPDATE g
	SET g.TaskStatus = 3, -- Change the goal status to 'Ended'
		g.UpdatedOn = GETDATE(),
		g.EndedOn = gtu.EndDate
	FROM Tasks g
	INNER JOIN TaskToUpdate gtu ON g.Id = gtu.Id
	WHERE g.UserId = @UserId;
END
GO