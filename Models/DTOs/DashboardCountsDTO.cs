namespace TaskMonitoringApp.Models.DTOs
{
    public class DashboardCountsDTO
    {
        public int TotalGoalCount { get; set; }
        public int RunningGoalCount { get; set; }
        public int CompletedGoalCount { get; set; }
        public int EndedGoalCount { get; set; }

        public int TotalTaskCount { get; set; }
        public int RunningTaskCount { get; set; }
        public int CompletedTaskCount { get; set; }
        public int EndedTaskCount { get; set; }
    }
}
