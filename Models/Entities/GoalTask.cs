namespace TaskMonitoringApp.Models.Entities
{
    public class GoalTask
    {
        public int GoalId { get; set; }
        public Goals Goal { get; set; }

        public int TaskId { get; set; }
        public Tasks Task { get; set; }

        public string UserId { get; set; }
        public Users User { get; set; }
    }
}
