namespace TaskMonitoringApp.Models.Entities
{
    public class StickyNotes
    {
        public int Id { get; set; }

        public int NoteId { get; set; }

        public Notes Notes { get; set; }
    }
}
