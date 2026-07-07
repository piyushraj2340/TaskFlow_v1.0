using System.ComponentModel.DataAnnotations;

namespace TaskMonitoringApp.Models.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        // Many-to-many relationship with Item
        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }
}
