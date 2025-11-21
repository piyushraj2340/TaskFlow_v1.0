using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.DTOs
{
    public class CollectionDto
    {
        public int CollectionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    public class CreateCollectionDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateCollectionDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;
    }
}
