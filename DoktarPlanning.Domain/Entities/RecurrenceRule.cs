using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;

using System;

namespace DoktarPlanning.Domain.Entities
{
    public class RecurrenceRule : IEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public Frequency Frequency { get; set; } = Frequency.Daily;

        public int Interval { get; set; }

        public DateTime? EndsAt { get; set; }
        public int? OccurrenceCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? RecurringJobId { get; set; }

        public User? User { get; set; }
    }
}