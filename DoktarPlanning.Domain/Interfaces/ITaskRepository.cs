using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;

namespace DoktarPlanning.Infrastructure.Repositories.Interfaces
{
    public interface ITaskRepository : IRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetTasksForUserAsync(Guid userId,
                                                         DateTime? from = null,
                                                         DateTime? to = null,
                                                         int? priority = null,
                                                         bool? isCompleted = null,
                                                         CancellationToken cancellationToken = default);

        Task<TaskItem> GetByIdForUserAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid userId, Guid recurrenceRuleId, DateTime dueAt, CancellationToken cancellationToken = default);
    }
}