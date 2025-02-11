using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class GoalViewModel : GoalDTO { }

    public class GoalWithNotesListViewModel : GoalViewModel
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesList { get; set; }
    }
}
