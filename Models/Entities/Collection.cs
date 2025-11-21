using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.Entities
{
    public class Collection
    {
        public int CollectionId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        // Foreign key for the user who owns this collection
        public string UserId { get; set; }
        public virtual Users User { get; set; }

        // Navigation property for the items within this collection
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
