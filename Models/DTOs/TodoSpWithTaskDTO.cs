using System.ComponentModel.DataAnnotations;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.DTOs
{
    public class TodoSpWithTaskDTO 
    {
        public int Id { get; set; }

        public DateTime? EndDate { get; set; } 

        public Status Status { get; set; }
        
        public DateTime? CreatedOn { get; set; } 

        public DateTime? UpdatedOn { get; set; } 

        public DateTime? DeletedOn { get; set; }

        public int TaskId { get; set; }

        public  string UserId { get; set; }

        public string? TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public RepeatType TaskRepeat { get; set; }

        public List<Weekly> TaskRepeatWeekList { get; set; }

        public Priority TaskPriority { get; set; }

        public DateTime? TaskEndDate { get; set; }

        public DateTime? TaskCreatedOn { get; set; } 

        public DateTime? TaskUpdatedOn { get; set; }

        public DateTime? TaskDeletedOn { get; set; }

        public Status TaskStatus { get; set; }
    }
}
