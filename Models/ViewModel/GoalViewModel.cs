using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class GoalViewModel : GoalDTO
    {
    }

    public class GoalWithNotesListViewModel : GoalViewModel
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesLists { get; set; }
    }

    public class GoalWithNotesAndTaskNameListViewModel : GoalViewModel
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesLists { get; set; }

        public IEnumerable<TaskNameDTO>? TaskLists { get; set; }
    }

    public class GoalWithTaskNameListViewModel : GoalViewModel
    {
        public IEnumerable<TaskNameDTO>? TaskLists { get; set; }
    }

    public class GoalWithTaskListViewModel : GoalViewModel
    {
        public IEnumerable<TaskDTO> TaskLists { get; set; }
    }
}
