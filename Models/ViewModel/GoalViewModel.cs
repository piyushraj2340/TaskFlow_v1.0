using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.ViewModel
{
    public class GoalViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime EndDate { get; set; }

        public Priority Priority { get; set; }

        public string Description { get; set; }

        public Status GoalStatus { get; set; }

        public string? UserId { get; set; }
    }

    public class GoalWithNotesListViewModel : GoalViewModel
    {
        public IEnumerable<NoteDTOWithGoalDTO> NotesList { get; set; }
    }
}
