using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using TodoMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Entities
{
    public class Todo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; } = DateTime.Now.AddDays(1); 

        [Required]
        public Status Status { get; set; } = Status.Running;

        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public string TaskId { get; set; }

        public Tasks Task { get; set; }

        public string UserId { get; set; }

        public Users User { get; set; }

        public string TodoProgressId { get; set; }

        public TodoProgressAnalysis TodoProgress { get; set; }
    }
}
