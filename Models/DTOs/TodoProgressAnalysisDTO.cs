using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class TodoProgressAnalysisDTO
    {
        public int Id { get; set; }

        public DateTime CalculateDateFor { get; set; } = DateTime.Today.Date;

        public int TotalTodo { get; set; }

        public int TotalCompletedTodo { get; set; }

        public int TotalMissedTodo { get; set; }

        public double ProductivityForDay { get; set; }

        public string UserId { get; set; }

        public Users User { get; set; }
    }
}
