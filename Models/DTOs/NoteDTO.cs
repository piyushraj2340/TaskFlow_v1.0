using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums; // Add this

namespace TaskMonitoringApp.Models.DTOs
{
    public class NoteDTO
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        // this will only update when we changed the content like title, content
        // other updations like pinned, unpinned will not modified 
        // true : if modified
        public bool IsModified { get; set; } = false;

        // this will only update when we changed the content like title, content with the current date and time
        // other updations like pinned, unpinned will not modified 
        public DateTime? ModifiedOn { get; set; }

        public int? ParentNoteId { get; set; }

        public NoteDTO? ParentNote { get; set; }

        public Status Status { get; set; }

        public bool IsPinned { get; set; } = false;

        // This will only update when we create with the current date and time
        public DateTime? TimeStamp { get; set; }

        public string? UserId { get; set; }
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

        // Mutable children collection to represent parent-child tree in responses.
        public ICollection<NoteDTOWithGoalAndTaskDTO> Children { get; set; } = new List<NoteDTOWithGoalAndTaskDTO>();
    }
}
