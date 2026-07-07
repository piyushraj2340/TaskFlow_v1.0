using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class DashboardRepository(ApplicationDbContext context) : IDashboardRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<DashboardCountsDTO> GetDashboardAnalysesAsync(string userId)
        {
            var goalsStats = await _context.Goals
                .Where(g => g.UserId == userId && !g.IsDeleted && g.GoalStatus != Status.NotStarted)
                .GroupBy(g => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Running = g.Count(x => x.GoalStatus == Status.Running),
                    Completed = g.Count(x => x.GoalStatus == Status.Completed),
                    Ended = g.Count(x => x.GoalStatus == Status.Ended)
                })
                .FirstOrDefaultAsync();

            var tasksStats = await _context.Tasks
                .Where(t => t.UserId == userId && !t.IsDeleted && t.TaskStatus != Status.NotStarted)
                .GroupBy(t => 1)
                .Select(t => new
                {
                    Total = t.Count(),
                    Running = t.Count(x => x.TaskStatus == Status.Running),
                    Completed = t.Count(x => x.TaskStatus == Status.Completed),
                    Ended = t.Count(x => x.TaskStatus == Status.Ended)
                })
                .FirstOrDefaultAsync();

            return new DashboardCountsDTO
            {
                TotalGoalCount = goalsStats?.Total ?? 0,
                RunningGoalCount = goalsStats?.Running ?? 0,
                CompletedGoalCount = goalsStats?.Completed ?? 0,
                EndedGoalCount = goalsStats?.Ended ?? 0,
                TotalTaskCount = tasksStats?.Total ?? 0,
                RunningTaskCount = tasksStats?.Running ?? 0,
                CompletedTaskCount = tasksStats?.Completed ?? 0,
                EndedTaskCount = tasksStats?.Ended ?? 0
            };
        }
    }
}
