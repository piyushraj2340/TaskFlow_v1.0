using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using TodoMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Entities
{
    public class Todo
    {
        public int Id { get; set; }

        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(1).Date;

        public Status Status { get; set; } = Status.Running;

        public DateTime? CreatedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public DateTime? EndedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public int TaskId { get; set; }

        public Tasks Task { get; set; }

        public string UserId { get; set; }

        public Users User { get; set; }

    }


    public class TodoWithTask
    {
        public int Id { get; set; }

        public DateTime? EndDate { get; set; }

        public Status Status { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public int TaskId { get; set; }

        public string UserId { get; set; }

        public string? TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public RepeatType TaskRepeat { get; set; }

        public List<Weekly> TaskRepeatWeekList { get; set; }

        public Priority TaskPriority { get; set; }

        public DateTime? TaskEndDate { get; set; }

        public DateTime? TaskCreatedOn { get; set; }

        public DateTime? TaskUpdatedOn { get; set; }

        public bool TaskIsDeleted { get; set; }

        public DateTime? TaskDeletedOn { get; set; }

        public Status TaskStatus { get; set; }
    }

}
