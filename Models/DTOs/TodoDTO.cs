using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class TodoDTO
    {
        public int Id { get; set; }

        public DateTime? EndDate { get; set; } = DateTime.Now.AddDays(1); // apply for the mid-night or some custom logic...

        public Status Status { get; set; } = Status.Running;

        public int TaskId { get; set; }

        public string UserId { get; set; }

        public TaskDTO? Task { get; set; }

        public Users? User { get; set; }
    }

    public class TodoDTOWithTaskDTO
    {
        public int Id { get; set; }

        // todo end date...
        public DateTime EndDate { get; set; }

        // todo status 
        public Status Status { get; set; }

        public int TaskId { get; set; }

        public string UserId { get; set; }

        public string TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public RepeatType TaskRepeat { get; set; }

        public List<Weekly> TaskRepeatWeekList { get; set; }

        public Priority TaskPriority { get; set; }

        public DateTime TaskEndDate { get; set; }

        public Status TaskStatus { get; set; }
    }


    public class TodoStatusDTO
    {
        public int Id { get; set; }

        public Status Status { get; set; } = Status.NotStarted;
    }


    public class TodoProductivityDTO(double productivity, int runningTask, int completedTask, int totalTask)
    {
        public double Productivity { get; set; } = productivity;

        public int RunningTask { get; set; } = runningTask;

        public int CompletedTask { get; set; } = completedTask;

        public int TotalTask { get; set; } = totalTask;
    }
}
