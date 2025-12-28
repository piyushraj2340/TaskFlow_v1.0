using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class GoalDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime? StartDate { get; set; }

        public bool IsScheduled { get; set; } = false;

        public StartOptions StartOptionType { get; set; }

        public bool IsStarted { get; set; } = false;

        public Priority Priority { get; set; }

        public string Description { get; set; }

        public Status GoalStatus { get; set; }

        public string? UserId { get; set; }

        // Multi-level properties
        public int? ParentId { get; set; }
        
        public string? ParentName { get; set; }

        public int SubGoalsCount { get; set; }
    }


    public class GoalDTOWithTaskNameListDTO : GoalDTO
    {
        public IEnumerable<TaskNameDTO>? TaskLists { get; set; }
        public IEnumerable<GoalDTO>? SubGoals { get; set; } // Added for Details view
    }

    public class GoalDTOWithTaskListDTO : GoalDTO
    {
        public IEnumerable<TaskDTO> TaskLists { get; set; }
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

        public string? Name { get; set; }

        public string? UserId { get; set; }

        public Status? GoalStatus { get; set; }

    }

    public class GoalDTOWithNoteListDTO : GoalDTO
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesLists { get; set; }
    }

    public class GoalDTOWithNotesAndTaskNameListDTO : GoalDTO
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesLists { get; set; }

        public IEnumerable<TaskNameDTO>? TaskLists { get; set; }
    }
}
