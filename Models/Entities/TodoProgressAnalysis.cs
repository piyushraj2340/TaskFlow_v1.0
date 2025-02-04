using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TaskMonitoringApp.Models.Entities;

namespace TodoMonitoringApp.Models.Entities
{
    // this is calculate by using the task-scheduler 
    public class TodoProgressAnalysis
    {
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime UpdatedOn { get; set; } = DateTime.Now;

        public int IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public DateTime CalculateDateFor { get; set; } = DateTime.Today.Date;

        public int TotalTodo { get; set; }

        public int TotalCompletedTodo { get; set; }

        public int TotalMissedTodo { get; set; }

        public double ProductivityForDay { get; set; }

        public string UserId { get; set; }

        public  Users User {get; set;}
    }
}
