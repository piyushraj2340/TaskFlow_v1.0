namespace TaskMonitoringApp.Models.DTOs
{
    public class FilterOptionDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public int? ParentId { get; set; } // For hierarchy if needed
    }

    public class FilterMenuDataDTO
    {
        public IEnumerable<FilterOptionDTO> Goals { get; set; }
        public IEnumerable<FilterOptionDTO> Tasks { get; set; }
        public int JournalCount { get; set; } // Standalone notes
    }
}