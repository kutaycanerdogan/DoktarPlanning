using AutoMapper;

using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Hangfire;

using Microsoft.Extensions.Logging;

using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace DoktarPlanning.Application.Services
{
    public class TaskService : BaseService<TaskItem>, ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ISubTaskRepository _subTaskRepository;
        private readonly IUserRepository _userRepository;
        private readonly IReminderService _reminderService;
        private readonly IRecurrenceRuleRepository _recurrenceRepository;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<TaskService> _logger;
        private readonly IRecurrenceJobRunner _recurrenceJobRunner;

        public TaskService(
            IRepository<TaskItem> genericRepo,
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            IRecurrenceRuleRepository recurrenceRepository,
            IMapper mapper,
            ILogger<TaskService> logger,
            IBackgroundJobService backgroundJobService,
            IEmailSender emailSender,
            IReminderService reminderService,
            IRecurrenceJobRunner recurrenceJobRunner,
            ISubTaskRepository subTaskRepository)
            : base(genericRepo, mapper, logger)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _recurrenceRepository = recurrenceRepository;
            _backgroundJobService = backgroundJobService;
            _emailSender = emailSender;
            _logger = logger;
            _reminderService = reminderService;
            _recurrenceJobRunner = recurrenceJobRunner;
            _subTaskRepository = subTaskRepository;
        }
        private async Task ValidateTaskAsync(Guid userId, object dto, CancellationToken cancellationToken)
        {
            var taskDto = dto as TaskDto ?? throw new ArgumentException("Invalid dto");
            if (string.IsNullOrWhiteSpace(taskDto.Title)) throw new ArgumentException("Title is required.");

            var exists = (await _taskRepository.FindAsync(t => t.UserId == userId && t.Title == taskDto.Title)).Any();
            if (exists) throw new InvalidOperationException("A task with the same title already exists.");
        }
        protected override async Task ValidateCreateAsync(Guid userId, object dto, CancellationToken cancellationToken)
        {
            await ValidateTaskAsync(userId, dto, cancellationToken);
        }

        protected override async Task AfterCreateAsync(Guid userId, TaskItem entity, CancellationToken cancellationToken)
        {
            await HandleReminderAndRecurringTask(userId, entity, -30, cancellationToken);
        }

        public async Task<TaskDto> CreateAsync(Guid userId, TaskDto task, CancellationToken cancellationToken = default)
            => await CreateAsync<TaskDto>(userId, task, cancellationToken);

        public async Task<TaskDto> UpdateAsync(Guid userId, Guid taskId, TaskDto task, CancellationToken cancellationToken = default)
        {
            var dto = await UpdateAsync<TaskDto>(userId, taskId, task, cancellationToken);
            await HandleReminderAndRecurringTask(userId, taskId, -30, cancellationToken);
            return dto;
        }

        public async Task<TaskDto> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        {
            var subtasks = await _subTaskRepository.GetByTaskIdAsync(taskId);
            await _subTaskRepository.RemoveRangeAsync(subtasks);
            _logger.LogInformation("Related Subtasks for TaskId: {TaskId} was removed", taskId);
            return await DeleteAsync<TaskDto>(userId, taskId, cancellationToken);
        }

        public async Task<TaskDto> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
            => await GetByIdAsync<TaskDto>(userId, taskId, cancellationToken);

        public async Task<IEnumerable<TaskDto>> ListAsync(Guid userId, TaskQueryParameters query, CancellationToken cancellationToken = default)
        {
            var items = await _taskRepository.GetTasksForUserAsync(userId, query.From, query.To, query.Priority.HasValue ? (int?)query.Priority.Value : null, query.IsCompleted, cancellationToken);
            return _mapper.Map<IEnumerable<TaskDto>>(items);
        }

        public async Task MarkCompleteAsync(Guid userId, Guid taskId, bool isComplete, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepository.GetByIdForUserAsync(userId, taskId, cancellationToken);
            if (task == null) throw new InvalidOperationException("Task not found.");

            task.IsCompleted = isComplete;
            task.CompletedAt = isComplete ? DateTime.Now : null;
            await _taskRepository.UpdateAsync(task);
            _logger.LogInformation("User {UserId} set task {TaskId} completed={IsComplete}", userId, taskId, isComplete);
        }

        public async Task<IEnumerable<TaskDto>> GetForDayAsync(Guid userId, DateTime day, CancellationToken cancellationToken = default)
        {
            var from = day.Date;
            var to = from.AddDays(1).AddTicks(-1);
            var tasks = await _taskRepository.GetTasksForUserAsync(userId, from, to, null, null, cancellationToken);
            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }
        private async Task AddRecurringJobForTask(Guid userId, TaskItem entity, double remindAtMinutes = -30, CancellationToken cancellationToken = default)
        {
            var rule = (await _recurrenceRepository
                    .FindAsync(r => r.Id == entity.RecurrenceRuleId.Value && r.UserId == userId))
                    .FirstOrDefault();

            if (rule == null)
                throw new InvalidOperationException("Recurrence rule not found.");

            var recurringJobId = rule.RecurringJobId;
            if (string.IsNullOrEmpty(recurringJobId))
            {
                recurringJobId = $"recurrence:{userId}:{rule.Id}";
                rule.RecurringJobId = recurringJobId;
                await _recurrenceRepository.UpdateAsync(rule, cancellationToken);
            }

            var cronExpression = "0 * * * *";

            await _backgroundJobService.AddOrUpdateRecurringAsync(
                recurringJobId,
                () => _recurrenceJobRunner.RunAsync(userId, rule.Id, cancellationToken),
                cronExpression,
                cancellationToken);

            _logger.LogInformation("Scheduled recurrence Hangfire job {JobId} for rule {RuleId}", recurringJobId, rule.Id);
        }
        private async Task ScheduleReminderForTask(Guid userId, TaskItem entity, double remindAtMinutes = -30, CancellationToken cancellationToken = default)
        {
            if (entity.DueAt.HasValue && !string.IsNullOrEmpty(entity.ReminderTarget) && entity.ReminderChannel is not null)
            {
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                var remindAt = entity.DueAt.Value.AddMinutes(remindAtMinutes);
                if (remindAt > DateTime.Now)
                {
                    await _reminderService.ScheduleReminderAsync(
                        userId,
                        entity,
                        remindAt,
                        entity.ReminderChannel == ReminderChannel.Email ? ReminderChannel.Email : ReminderChannel.Webhook,
                        entity.ReminderChannel == ReminderChannel.Email ? user.Email : entity.ReminderTarget,
                        cancellationToken);
                }
            }
        }
        private async Task HandleReminderAndRecurringTask(Guid userId, Guid taskId, double remindAtMinutes = -30, CancellationToken cancellationToken = default)
        {
            var entity = await _taskRepository.GetByIdForUserAsync(userId, taskId, cancellationToken);

            await ScheduleReminderForTask(userId, entity, remindAtMinutes, cancellationToken);

            if (entity.IsRecurring && entity.RecurrenceRuleId.HasValue)
            {
                await AddRecurringJobForTask(userId, entity, remindAtMinutes, cancellationToken);
            }
        }
        private async Task HandleReminderAndRecurringTask(Guid userId, TaskItem entity, double remindAtMinutes = -30, CancellationToken cancellationToken = default)
        {
            await ScheduleReminderForTask(userId, entity, remindAtMinutes, cancellationToken);

            if (entity.IsRecurring && entity.RecurrenceRuleId.HasValue)
            {
                await AddRecurringJobForTask(userId, entity, remindAtMinutes, cancellationToken);
            }
        }
    }
}