using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskMonitoringApp.Models.Entities;

namespace TodoMonitoringApp.Models.Entities
{
    // this is calculate by using the task-scheduler 
    public class TodoProgressAnalysis
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [Required]
        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public DateTime? DeletedOn { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime CalculateDateFor { get; set; } = DateTime.Today;

        [Range(0, int.MaxValue)]
        public int TotalTodo { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalCompletedTodo { get; set; }

        [Range(0, int.MaxValue)]
        public int TotalMissedTodo { get; set; }

        public double ProductivityForDay { get; set; }

        public virtual Users User {get; set;}
    }
}
