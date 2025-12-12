using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;

namespace DoktarPlanning.Infrastructure.DTOs
{
    public class RecurrenceDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public Frequency Frequency { get; set; } = Frequency.Daily;

        public int Interval { get; set; }

        public DateTime? EndsAt { get; set; }
        public int? OccurrenceCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? RecurringJobId { get; set; }

    }
}
