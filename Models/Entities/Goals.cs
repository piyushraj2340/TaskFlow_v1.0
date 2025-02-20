using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskMonitoringApp.Models.Entities
{
    public enum Status
    {
        NotStarted = 0,
        Running = 1,
        Completed = 2,
        Ended = 3,
        All = 4,
        Draft = 5,
        Published = 6,
        Archived = 7,
    }

    public enum ResponseDataMode
    {
        Model = 0,
        ModelDTO,
        ModelNameDTO
    }

    public enum StartOptions
    {
        Manual = 0,
        Scheduled,
        Immediate
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

        public bool IsScheduled { get; set; } = false;

        public StartOptions StartOptionType { get; set; } = StartOptions.Manual;

        public DateTime? StartDate { get; set; }

        public bool IsStarted { get; set; } = false;

        public DateTime? StartedOn { get; set; }

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
