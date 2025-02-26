CREATE PROCEDURE [dbo].[usp_GetGoalById] (
	@UserId nvarchar(450),
	@GoalId int,
	@Mode int			-- The mode 
					    -- 0 = Goals - Model
						-- 1 = GoalDTO - Model
						-- 2 = GoalNameDTO - Model
)
as 
begin 
SET NOCOUNT ON;

	BEGIN TRANSACTION;

	BEGIN TRY

		-- Change the status of the ended goals as Date has passed but goalStatus has not changed....
		EXEC usp_UpdateEndedGoal @UserId;

		if(@Mode = 0)
			begin
				select g.*
				from Goals g
				where g.UserId = @UserId and g.Id = @GoalId and IsDeleted = 0;
			end
		else if(@Mode = 1)
			begin
				select g.Id, g.Name, g.EndDate, g.Priority, g.Description,  g.GoalStatus, g.UserId, g.IsScheduled, g.StartDate, g.IsStarted, g.StartOptionType
				from Goals g
				where g.UserId = @UserId and g.Id = @GoalId and IsDeleted = 0;
			end
		else if(@Mode = 2)
			begin 
				select g.Id, g.Name, g.UserId
				from Goals g
				where g.UserId = @UserId and g.Id = @GoalId and IsDeleted = 0;
			end
		else 
			begin 
				RAISERROR ('Invalid @Mode To Access Data From usp_GetGoalById', 16, 1);
				RETURN;
			end
		COMMIT; -- save all changes...
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
		return;
	END CATCH
end;
GO