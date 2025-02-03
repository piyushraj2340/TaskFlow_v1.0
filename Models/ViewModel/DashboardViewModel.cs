using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class DashboardViewModel
    {
        public IEnumerable<GoalViewModel>? Goals { get; set; }

        public IEnumerable<TaskViewModel>? Tasks { get; set; }

        public DashboardProductivityViewModel? Productivity { get; set; }
    }

    public class DashboardProductivityViewModel
    {
        public int CompletedGoals { get; set; }

        public int CompletedTasks { get; set; }

        public double GrowthPercentage { get; set; }

        public double Productivity { get; set; }
    }
}
