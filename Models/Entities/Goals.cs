using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.Entities
{
    public enum Status
    {
        NotStarted = 0,
        Running,
        Completed,
        Ended,
        All
    }

    public enum ResponseDataMode
    {
        Model = 0,
        ModelDTO,
        ModelNameDTO
    }

    public class Goals
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public Priority Priority { get; set; } = Priority.High;

        public DateTime EndDate { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public DateTime? EndedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public Status GoalStatus { get; set; } = Status.NotStarted;

        public string UserId { get; set; }

        public Users? User { get; set; }

        public ICollection<GoalTask>? GoalTasks { get; set; }
    }
}
