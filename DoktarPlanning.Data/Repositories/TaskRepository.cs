using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DoktarPlanning.Data.Repositories
{
    public class TaskRepository : Repository<TaskItem>, ITaskRepository
    {
        public TaskRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<TaskItem>> GetTasksForUserAsync(Guid userId,
                                                                      DateTime? from = null,
                                                                      DateTime? to = null,
                                                                      int? priority = null,
                                                                      bool? isCompleted = null,
                                                                      CancellationToken cancellationToken = default)
        {
            IQueryable<TaskItem> query = _dbSet.AsNoTracking().Where(t => t.UserId == userId);

            if (from.HasValue) query = query.Where(t => t.DueAt != null && t.DueAt >= from.Value);
            if (to.HasValue) query = query.Where(t => t.DueAt != null && t.DueAt <= to.Value);
            if (priority.HasValue) query = query.Where(t => (int)t.Priority == priority.Value);
            if (isCompleted.HasValue) query = query.Where(t => t.IsCompleted == isCompleted.Value);

            query = query.OrderBy(t => t.DueAt);

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<TaskItem> GetByIdForUserAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.AsNoTracking()
                                     .Include(t => t.SubTasks)
                                     .Include(t => t.RecurrenceRule)
                                     .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId, cancellationToken);

            return entity ?? throw new InvalidOperationException($"Task with id {taskId} for user {userId} not found.");
        }
        public async Task<bool> ExistsAsync(Guid userId, Guid recurrenceRuleId, DateTime dueAt, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(t =>
                    t.UserId == userId &&
                    t.RecurrenceRuleId == recurrenceRuleId &&
                    t.DueAt == dueAt,
                    cancellationToken);
        }
    }
}