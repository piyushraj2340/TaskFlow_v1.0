using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class GoalDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime EndDate { get; set; }

        public Priority Priority { get; set; }

        public string Description { get; set; }

        public Status GoalStatus { get; set; }

        public string UserId { get; set; }
    }

    public class GoalDTOWithTaskNameDTOs
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime EndDate { get; set; }

        public Priority Priority { get; set; }

        public string Description { get; set; }

        public Status GoalStatus { get; set; }

        public string UserId { get; set; }

        public ICollection<TaskNameDTO>? TaskLists { get; set; }
    }

    public class GoalDTOWithTaskDTOs
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime EndDate { get; set; }

        public Priority Priority { get; set; }

        public string Description { get; set; }

        public Status GoalStatus { get; set; }

        public string UserId { get; set; }

        public ICollection<TaskDTO> TaskList { get; set; }
    }

    public class GoalStatusDTO
    {
        public int Id { get; set; }

        public Status GoalStatus { get; set; }

        public string? UserId { get; set; }
    }

    public class GoalProductivityDTO(double productivity, int runningGoal, int completedGoal, double growthPercentage)
    {
        public double Productivity { get; set; } = productivity;

        public int RunningGoal { get; set; } = runningGoal;

        public int CompletedGoal { get; set; } = completedGoal;

        public double GrowthPercentage { get; set; } = growthPercentage;
    }

    public class GoalNameDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? UserId { get; set; }

    }

    public class GoalDTOWithNoteListDTO : GoalDTO
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesList { get; set; }
    }
}
