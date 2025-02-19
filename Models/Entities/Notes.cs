namespace TaskMonitoringApp.Models.Entities
{
    public enum NotesAttachedWith
    {
        All = 0,
        Goal,
        Task,
        Todo,
    }

    public class Notes
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

        public Notes? ParentNote { get; set; }

        public Status Status { get; set; }

        public bool IsPinned { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

        // This will only update when we create with the current date and time
        public DateTime TimeStamp { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public int? TodoId { get; set; }

        public Todo? Todo { get; set; }

        public int? TaskId { get; set; }

        public Tasks? Task { get; set; }

        public int? GoalId { get; set; }

        public Goals? Goal { get; set; }

        public string UserId { get; set; }

        public Users User { get; set; }
    }
}
