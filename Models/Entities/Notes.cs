namespace TaskMonitoringApp.Models.Entities
{
    public class Notes
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public bool IsModified { get; set; } = false;

        public DateTime? ModifiedOn { get; set; }

        public int? ParentNoteId { get; set; }

        public Notes? ParentNote { get; set; }

        public Status Status { get; set; }

        public bool IsPinned { get; set; }

        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

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
