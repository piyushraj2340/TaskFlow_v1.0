using Azure;
using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.Entities
{
    public class Item
    {
        public int ItemId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        public string Description { get; set; }

        // Foreign key for the parent collection
        public int CollectionId { get; set; }
        public virtual Collection Collection { get; set; }

        // Many-to-many relationships
        public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
