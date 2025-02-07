using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class NoteDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public bool IsModified { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public int? ParentNoteId { get; set; }

        public Status Status { get; set; }

        public bool IsPinned { get; set; }

        public DateTime TimeStamp { get; set; }

        public string UserId { get; set; }
    }

    public class NoteDTOWithGoalDTO : NoteDTO
    {
        public int GoalId { get; set; }

        public GoalDTO? Goal { get; set; }

    }

    public class NoteDTOWithTaskDTO : NoteDTO
    {
        public int TaskId { get; set; }

        public TaskDTO? Task { get; set; }
    }

    public class NoteDTOWithGoalAndTaskDTO : NoteDTO
    {
        public int GoalId { get; set; }

        public GoalDTO? Goal { get; set; }

        public int TaskId { get; set; }

        public TaskDTO? Task { get; set; }
    }
}
