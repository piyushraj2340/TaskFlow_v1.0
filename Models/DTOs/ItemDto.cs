using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.DTOs
{
    public class ItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<CategoryTagDto> Categories { get; set; } = default!;
        public List<CategoryTagDto> Tags { get; set; } = default!;
    }
    public class CreateItemDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class UpdateItemDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();
    }
}
