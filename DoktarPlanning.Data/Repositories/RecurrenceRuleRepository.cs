using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DoktarPlanning.Data.Repositories
{
    public class RecurrenceRuleRepository : Repository<RecurrenceRule>, IRecurrenceRuleRepository
    {
        public RecurrenceRuleRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<RecurrenceRule>> GetActiveRulesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var query = _dbSet.AsNoTracking()
                              .Where(r => r.UserId == userId && (r.EndsAt == null || r.EndsAt >= now));

            return await query.ToListAsync(cancellationToken);
        }
    }
}