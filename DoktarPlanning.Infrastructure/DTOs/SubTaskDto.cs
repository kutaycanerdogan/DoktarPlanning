using DoktarPlanning.Domain.Entities;

namespace DoktarPlanning.Infrastructure.DTOs
{
    public class SubTaskDto
    {
        public Guid Id { get; set; }
        public Guid TaskItemId { get; set; }

        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

    }
}
