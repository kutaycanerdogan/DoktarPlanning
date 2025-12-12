using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.Extensions.Logging;

namespace DoktarPlanning.Application.Services
{
    public class RecurrenceJobRunner : IRecurrenceJobRunner
    {
        private readonly IRecurrenceService _recurrenceService;
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<RecurrenceJobRunner> _logger;

        public RecurrenceJobRunner(
            IRecurrenceService recurrenceService,
            ITaskRepository taskRepository,
            ILogger<RecurrenceJobRunner> logger)
        {
            _recurrenceService = recurrenceService;
            _taskRepository = taskRepository;
            _logger = logger;
        }

        public async Task RunAsync(Guid userId, Guid ruleId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.Now;
            var horizon = now.AddDays(30);

            _logger.LogInformation("Running recurrence job for user {UserId}, rule {RuleId}, horizon {From} - {To}",
                userId, ruleId, now, horizon);

            var instances = await _recurrenceService.GenerateInstancesAsync(userId, ruleId, now, horizon, cancellationToken);

            foreach (var dto in instances)
            {
                if (!dto.DueAt.HasValue)
                    continue;

                var dueAt = dto.DueAt.Value;

                var exists = await _taskRepository.ExistsAsync(userId, ruleId, dueAt, cancellationToken);
                if (exists)
                    continue;

                var entity = new TaskItem
                {
                    Id = dto?.Id != Guid.Empty ? (Guid)dto?.Id : Guid.NewGuid(),
                    UserId = userId,
                    Title = dto.Title,
                    Description = dto.Description,
                    DueAt = dueAt,
                    StartAt = dto.StartAt,
                    EndAt = dto.EndAt,
                    Priority = dto.Priority,
                    IsCompleted = false,
                    CreatedAt = dto.CreatedAt != default ? dto.CreatedAt : DateTime.Now,
                    IsRecurring = false,
                    RecurrenceRuleId = ruleId
                };

                await _taskRepository.AddAsync(entity, cancellationToken);
                _logger.LogInformation("Created recurring task instance {TaskId} for rule {RuleId} at {DueAt}",
                    entity.Id, ruleId, dueAt);
            }
        }
    }
}