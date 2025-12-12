using DoktarPlanning.Infrastructure.Interfaces;

using Hangfire;

using Microsoft.Extensions.Logging;

using System.Linq.Expressions;

namespace DoktarPlanning.Application.Services
{
    public class HangfireBackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient _client;
        private readonly IRecurringJobManager _recurring;
        private readonly ILogger<HangfireBackgroundJobService> _logger;

        public HangfireBackgroundJobService(IBackgroundJobClient client, IRecurringJobManager recurring, ILogger<HangfireBackgroundJobService> logger)
        {
            _client = client;
            _recurring = recurring;
            _logger = logger;
        }

        public Task<string> EnqueueAsync(Expression<Func<Task>> job, CancellationToken cancellationToken = default)
        {
            var jobId = _client.Enqueue(job);
            return Task.FromResult(jobId);
        }

        public Task<string> ScheduleAsync(Expression<Func<Task>> job, TimeSpan delay, CancellationToken cancellationToken = default)
        {
            var jobId = _client.Schedule(job, delay);
            return Task.FromResult(jobId);
        }

        public Task AddOrUpdateRecurringAsync(string recurringJobId, Expression<Func<Task>> job, string cronExpression, CancellationToken cancellationToken = default)
        {
            _recurring.AddOrUpdate(recurringJobId, job, cronExpression);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string jobId, CancellationToken cancellationToken = default)
        {
            _client.Delete(jobId);
            return Task.CompletedTask;
        }
    }
}