CREATE OR ALTER PROCEDURE [dbo].[usp_DashboardAnalyses](
	@userId NVARCHAR(450)
)

AS
BEGIN
-- The status of the goal. Possible values:
    -- 0 = NotStarted
    -- 1 = Running
    -- 2 = Completed
    -- 3 = Ended

	DECLARE @NotStarted INT = 0
	DECLARE @Running INT = 1
	DECLARE @Completed INT = 2
	DECLARE @Ended INT = 3

	DECLARE @today DATETIME = CAST(GETDATE() AS DATE)

	DECLARE @TotalGoalCount INT;
	DECLARE @RunningGoalCount INT;
	DECLARE @CompletedGoalCount INT;
	DECLARE @EndedGoalCount INT;

	DECLARE @TotalTaskCount INT;
	DECLARE @RunningTaskCount INT;
	DECLARE @CompletedTaskCount INT;
	DECLARE @EndedTaskCount INT;

	SELECT  @TotalGoalCount = COUNT(*),
			@RunningGoalCount =	SUM(CASE WHEN GoalStatus = @Running THEN 1 ELSE 0 END),
			@CompletedGoalCount = SUM(CASE WHEN GoalStatus = @Completed THEN 1 ELSE 0 END),
			@EndedGoalCount = SUM(CASE WHEN GoalStatus = @Ended THEN 1 ELSE 0 END)
	FROM Goals 
	WHERE UserId = @userId
		AND IsDeleted = 0
		AND GoalStatus != @NotStarted


	SELECT  @TotalTaskCount = COUNT(*),
			@RunningTaskCount =	SUM(CASE WHEN TaskStatus = @Running THEN 1 ELSE 0 END),
			@CompletedTaskCount = SUM(CASE WHEN TaskStatus = @Completed THEN 1 ELSE 0 END),
			@EndedTaskCount = SUM(CASE WHEN TaskStatus = @Ended THEN 1 ELSE 0 END)
	FROM Tasks 
	WHERE UserId = @userId
	AND IsDeleted = 0
	AND TaskStatus != @NotStarted

	-- overall productivity
	-- groth this weeks 

END