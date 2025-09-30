using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class SearchResultDTO
    {
        public int EntityId { get; set; }           // id of the item (goal/task/todo/note)
        public string Title { get; set; } = string.Empty; // Name or Title
        public string Url { get; set; } = string.Empty;   // redirect url
        public string Type { get; set; } = string.Empty;  // "goal" | "task" | "todo" | "note"
        public Status? Status { get; set; }                // status string if applicable
        public Priority? Priority { get; set; }              // priority string if applicable
        public DateTime? TimeStamp { get; set; }           // created/occurred timestamp to sort by
        public bool? IsPinned { get; set; }                // only for notes (nullable for others)
        public string? Snippet { get; set; }               // optional snippet/preview
    }
}