using DoktarPlanning.Infrastructure.DTOs;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IRecurrenceService
    {
        Task<RecurrenceDto> CreateRuleAsync(Guid userId, RecurrenceDto rule, CancellationToken cancellationToken = default);
        Task<RecurrenceDto> UpdateRuleAsync(Guid userId, Guid ruleId, RecurrenceDto rule, CancellationToken cancellationToken = default);
        Task DeleteRuleAsync(Guid userId, Guid ruleId, CancellationToken cancellationToken = default);
        Task<IEnumerable<RecurrenceDto>> ListRulesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<TaskDto>> GenerateInstancesAsync(Guid userId, Guid ruleId, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    }
}