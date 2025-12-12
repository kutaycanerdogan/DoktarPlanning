using DoktarPlanning.Domain.Common;

namespace DoktarPlanning.Infrastructure.DTOs
{
    public class TaskQueryParameters
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Priority? Priority { get; set; }
        public bool? IsCompleted { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
