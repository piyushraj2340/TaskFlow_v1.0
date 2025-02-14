using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class GoalViewModel : GoalDTO { }

    public class GoalWithNotesListViewModel : GoalViewModel
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesList { get; set; }
    }

    public class GoalWithTaskNameListViewModel : GoalViewModel
    {
        public IEnumerable<TaskNameDTO>? TaskLists { get; set; }
    }

    public class GoalWithTaskListViewModel : GoalViewModel
    {
        public IEnumerable<TaskDTO> TaskList { get; set; }
    }
}
