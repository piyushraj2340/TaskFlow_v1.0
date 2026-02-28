using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskMonitoringApp.Models.Enums; // Added using

namespace TaskMonitoringApp.Models.Entities
{
    public class Goals
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public Priority Priority { get; set; } = Priority.High;

        public DateTime EndDate { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime UpdatedOn { get; set; } = DateTime.Now;

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

        public IEnumerable<GoalTask>? GoalTasks { get; set; }

        // Multi-level Goal Relationships
        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Goals? Parent { get; set; }

        public ICollection<Goals>? SubGoals { get; set; }
    }
}
