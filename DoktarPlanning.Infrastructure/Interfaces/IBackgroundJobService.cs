using System.Linq.Expressions;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IBackgroundJobService
    {
        Task<string> EnqueueAsync(Expression<Func<Task>> job, CancellationToken cancellationToken = default);
        Task<string> ScheduleAsync(Expression<Func<Task>> job, TimeSpan delay, CancellationToken cancellationToken = default);
        Task AddOrUpdateRecurringAsync(string recurringJobId, Expression<Func<Task>> job, string cronExpression, CancellationToken cancellationToken = default);
        Task DeleteAsync(string jobId, CancellationToken cancellationToken = default);
    }
}