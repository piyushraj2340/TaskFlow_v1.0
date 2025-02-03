using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class TodoDTO
    {
        public int Id { get; set; }

        public DateTime? EndDate { get; set; } = DateTime.Now.AddDays(1); // apply for the mid-night or some custom logic...

        public Status Status { get; set; } = Status.Running;

        public virtual TaskDTO? Task { get; set; }
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
