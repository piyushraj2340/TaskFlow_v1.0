using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class TaskDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public RepeatType Repeat { get; set; } = RepeatType.RunOnce;

        public List<Weekly>? RepeatWeekList { get; set; }

        public Priority Priority { get; set; } = Priority.High;

        public DateTime? EndDate { get; set; }

        public string UserId { get; set; }

        public Status TaskStatus { get; set; } = Status.NotStarted;
    }

    public class TaskDTOWithGoalNameDTOs
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public RepeatType Repeat { get; set; } = RepeatType.RunOnce;

        public List<Weekly>? RepeatWeekList { get; set; }

        public Priority Priority { get; set; } = Priority.High;

        public DateTime? EndDate { get; set; }

        public string UserId { get; set; }

        public Status TaskStatus { get; set; } = Status.NotStarted;

        public IEnumerable<GoalNameDTO>? GoalLists { get; set; }
    }

    public class TaskDTOWithGoalDTOs
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public RepeatType Repeat { get; set; } = RepeatType.RunOnce;

        public List<Weekly>? RepeatWeekList { get; set; }

        public Priority Priority { get; set; } = Priority.High;

        public DateTime? EndDate { get; set; }

        public string UserId { get; set; }

        public Status TaskStatus { get; set; } = Status.NotStarted;

        public IEnumerable<GoalDTO>? GoalLists { get; set; }
    }

    public class TaskStatusDTO
    {
        public int Id { get; set; }

        public Status TaskStatus { get; set; } = Status.NotStarted;

        public string? UserId { get; set; }
    }

    public class TaskProductivityDTO(double productivity, int runningTask, int completedTask, double growthPercentage)
    {
        public double Productivity { get; set; } = productivity;

        public int RunningTask { get; set; } = runningTask;

        public int CompletedTask { get; set; } = completedTask;

        public double GrowthPercentage { get; set; } = growthPercentage;
    }

    public class TaskNameDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? UserId { get; set; }
    }
}
