using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;

namespace DoktarPlanning.Infrastructure.Repositories.Interfaces
{
    public interface IRecurrenceRuleRepository : IRepository<RecurrenceRule>
    {
        Task<IEnumerable<RecurrenceRule>> GetActiveRulesForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}