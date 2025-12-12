using DoktarPlanning.Domain.Common;

namespace DoktarPlanning.Domain.Entities
{
    public class SubTask : IEntity
    {
        public Guid Id { get; set; }
        public Guid TaskItemId { get; set; }

        public string Title { get; set; } = null!;
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        public TaskItem? TaskItem { get; set; }
    }
}