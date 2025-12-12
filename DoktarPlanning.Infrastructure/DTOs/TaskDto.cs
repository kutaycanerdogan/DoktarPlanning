using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;

namespace DoktarPlanning.Infrastructure.DTOs
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Notes { get; set; }

        public DateTime? DueAt { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public Priority Priority { get; set; }
        public bool IsCompleted { get; set; }
        public string? ReminderJobId { get; set; }
        public ReminderChannel? ReminderChannel { get; set; }
        public string? ReminderTarget { get; set; }


        public bool IsRecurring { get; set; }
        public Guid? RecurrenceRuleId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }
    }
}