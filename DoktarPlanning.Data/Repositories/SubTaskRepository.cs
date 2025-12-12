using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DoktarPlanning.Data.Repositories
{
    public class SubTaskRepository : Repository<SubTask>, ISubTaskRepository
    {
        public SubTaskRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<SubTask>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsNoTracking().Where(s => s.TaskItemId == taskId).ToListAsync(cancellationToken);
        }
    }
}