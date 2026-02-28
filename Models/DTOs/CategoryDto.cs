using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.Enums;

namespace TaskMonitoringApp.Models.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

         // Likely uses ColorType
    }

    public class UpdateCategoryDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
