using AutoMapper;

using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.Extensions.Logging;

namespace DoktarPlanning.Application.Services
{
    public class RecurrenceService : IRecurrenceService
    {
        private readonly IRepository<RecurrenceRule> _repo;
        private readonly IRecurrenceRuleRepository _recurrenceRepo;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly IMapper _mapper;
        private readonly ILogger<RecurrenceService> _logger;

        public RecurrenceService(
            IRepository<RecurrenceRule> repo,
            IRecurrenceRuleRepository recurrenceRepo,
            IBackgroundJobService backgroundJobs,
            IMapper mapper,
            ILogger<RecurrenceService> logger)
        {
            _repo = repo;
            _recurrenceRepo = recurrenceRepo;
            _backgroundJobs = backgroundJobs;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RecurrenceDto> CreateRuleAsync(Guid userId, RecurrenceDto rule, CancellationToken cancellationToken = default)
        {
            if (rule.Interval < 1)
                throw new ArgumentException("Interval must be >= 1");

            var entity = _mapper.Map<RecurrenceRule>(rule);
            entity.UserId = userId;
            entity.Frequency = Frequency.Daily;
            entity.CreatedAt = DateTime.Now;

            var added = await _repo.AddAsync(entity, cancellationToken);
            return _mapper.Map<RecurrenceDto>(added);
        }

        public async Task<RecurrenceDto> UpdateRuleAsync(Guid userId, Guid ruleId, RecurrenceDto rule, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.FirstOrDefaultAsync(r => r.Id == ruleId && r.UserId == userId, null, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException("Recurrence rule not found.");

            if (rule.Interval < 1)
                throw new ArgumentException("Interval must be >= 1");

            existing.Interval = rule.Interval;
            existing.EndsAt = rule.EndsAt;
            existing.OccurrenceCount = rule.OccurrenceCount;

            await _repo.UpdateAsync(existing, cancellationToken);
            return _mapper.Map<RecurrenceDto>(existing);
        }

        public async Task DeleteRuleAsync(Guid userId, Guid ruleId, CancellationToken cancellationToken = default)
        {
            var existing = await _repo.FirstOrDefaultAsync(r => r.Id == ruleId && r.UserId == userId, null, cancellationToken);
            if (existing == null)
                throw new InvalidOperationException("Recurrence rule not found.");

            if (!string.IsNullOrEmpty(existing.RecurringJobId))
            {
                await _backgroundJobs.DeleteAsync(existing.RecurringJobId, cancellationToken);
                _logger.LogInformation("Deleted recurrence Hangfire job {JobId} for rule {RuleId}", existing.RecurringJobId, ruleId);
            }

            await _repo.RemoveAsync(existing, cancellationToken);
        }

        public async Task<IEnumerable<RecurrenceDto>> ListRulesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var rules = await _recurrenceRepo.GetActiveRulesForUserAsync(userId, cancellationToken);
            return _mapper.Map<IEnumerable<RecurrenceDto>>(rules);
        }

        public async Task<IEnumerable<TaskDto>> GenerateInstancesAsync(
            Guid userId,
            Guid ruleId,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var instances = new List<TaskDto>();
            if (to < from)
                throw new ArgumentException("to must be >= from");

            var rule = (await _recurrenceRepo.FindAsync(
                r => r.Id == ruleId && r.UserId == userId,
                cancellationToken: cancellationToken))
                .FirstOrDefault();

            if (rule == null)
                throw new InvalidOperationException("Recurrence rule not found.");

            if (rule.Frequency != Frequency.Daily)
                throw new NotSupportedException("Only daily recurrence is supported.");

            var interval = Math.Max(1, rule.Interval);
            var anchor = rule.CreatedAt.Date;
            var endsAt = rule.EndsAt?.Date;
            var maxOccurrences = rule.OccurrenceCount;

            var occurrences = 0;

            var start = from.Date;
            var daysOffset = (start - anchor).Days;
            var remainder = daysOffset % interval;
            if (remainder < 0) remainder += interval;
            var current = remainder == 0 ? start : start.AddDays(interval - remainder);

            while (current <= to.Date)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (endsAt.HasValue && current > endsAt.Value)
                    break;

                if (maxOccurrences.HasValue && occurrences >= maxOccurrences.Value)
                    break;

                instances.Add(new TaskDto
                {
                    Id = Guid.NewGuid(),
                    Title = $"Recurring Task Instance (Daily every {interval} days)",
                    DueAt = current,
                    Priority = Priority.Medium,
                    IsCompleted = false,
                    CreatedAt = DateTime.Now
                });

                occurrences++;
                current = current.AddDays(interval);
            }

            return instances;
        }
    }
}