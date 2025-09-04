using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class TaskViewModel
    {
        [Display(Name = "Task Id")]
        public int? Id { get; set; }

        [Required]
        [Length(minimumLength: 3, maximumLength: 200, ErrorMessage = "Length of Name must be less than 200 and 3")]
        [Display(Name = "Task Name")]
        public string Name { get; set; } = default!;

        [Required]
        [Display(Name = "Task Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Task Repeat")]
        public RepeatType Repeat { get; set; } = RepeatType.RunOnce;

        [Display(Name = "Select Days (Weekly)")]

        public List<Weekly>? RepeatWeekList { get; set; }

        [Display(Name = "Task Priority")]
        public Priority Priority { get; set; } = Priority.High;

        [Display(Name = "Task Due Date")]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Task Start Date")]
        public DateTime? StartDate { get; set; }

        public bool IsScheduled { get; set; } = false;

        public StartOptions StartOptionType { get; set; }

        public bool IsStarted { get; set; } = false;

        [Display(Name = "Task Task Status")]
        public Status TaskStatus { get; set; } = Status.NotStarted;

        public string? GoalIds { get; set; }

        public IEnumerable<GoalNameDTO>? GoalLists { get; set; }
        public IEnumerable<NoteDTOWithTaskDTO>? NotesLists { get; set; }

        public string? UserId { get; set; }
    }

    public class TaskProductivityViewModel
    {
        public double Productivity { get; set; }

        public int RunningGoal { get; set; }

        public int CompletedGoal { get; set; }

        public double GrowthPercentage { get; set; }
    }
}
