using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

namespace DoktarPlanning.Application.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IBackgroundJobService _jobs;
        private readonly IEmailSender _emailSender;
        private readonly IWebhookSender _webhookSender;
        private readonly IRepository<TaskItem> _taskRepo;
        private readonly ILogger<ReminderService> _logger;

        public ReminderService(IBackgroundJobService jobs, IEmailSender emailSender, IRepository<TaskItem> taskRepo, ILogger<ReminderService> logger, IWebhookSender webhookSender)
        {
            _jobs = jobs;
            _emailSender = emailSender;
            _taskRepo = taskRepo;
            _logger = logger;
            _webhookSender = webhookSender;
        }

        public async Task ScheduleReminderAsync(Guid userId, Guid taskId, DateTime remindAt, ReminderChannel channel, string? channelTarget = null, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepo.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) throw new InvalidOperationException("Task not found.");
            await ScheduleReminderDoJobAsync(userId, task, remindAt, channel, channelTarget, cancellationToken);
        }
        public async Task ScheduleReminderAsync(Guid userId, TaskItem task, DateTime remindAt, ReminderChannel channel, string? channelTarget = null, CancellationToken cancellationToken = default)
        {
            if (task == null) throw new InvalidOperationException("Task not found.");
            await ScheduleReminderDoJobAsync(userId, task, remindAt, channel, channelTarget, cancellationToken);
        }
        private async Task ScheduleReminderDoJobAsync(Guid userId, TaskItem task, DateTime remindAt, ReminderChannel channel, string? channelTarget = null, CancellationToken cancellationToken = default)
        {
            var to = channelTarget ?? throw new ArgumentException("channelTarget is required");
            var delay = remindAt - DateTime.Now;
            string jobId = "";
            if (channel == ReminderChannel.Email)
            {
                jobId = await _jobs.ScheduleAsync(() => _emailSender.SendAsync(to, $"Reminder: {task.Title}", $"Task due at {task.DueAt}"),
                    delay, cancellationToken);
            }
            else if (channel == ReminderChannel.Webhook)
            {
                jobId = await _jobs.ScheduleAsync(() => _webhookSender.SendAsync(to, new { task.Id, task.Title, task.DueAt }),
                    delay, cancellationToken);
            }
            else
            {
                throw new NotImplementedException("Unsupported channel");
            }

            task.ReminderJobId = jobId;
            task.ReminderChannel = channel;
            task.ReminderTarget = to;
            await _taskRepo.UpdateAsync(task, cancellationToken);
        }
        public async Task CancelReminderAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepo.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) throw new InvalidOperationException("Task not found.");

            if (string.IsNullOrEmpty(task.ReminderJobId))
                return;

            try
            {
                await _jobs.DeleteAsync(task.ReminderJobId!, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Job cancel failed for {TaskId}", taskId);
            }

            task.ReminderJobId = null;
            task.ReminderChannel = null;
            task.ReminderTarget = null;
            await _taskRepo.UpdateAsync(task, cancellationToken);
        }

        public async Task SendReminderNowAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepo.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) throw new InvalidOperationException("Task not found.");

            if (task.ReminderChannel == ReminderChannel.Email)
            {
                var to = task.ReminderTarget ?? throw new InvalidOperationException("No email target configured");
                await _emailSender.SendAsync(to, $"Reminder: {task.Title}", $"Task due at {task.DueAt}", cancellationToken);
            }
            else if (task.ReminderChannel == ReminderChannel.Webhook)
            {
                var to = task.ReminderTarget ?? throw new InvalidOperationException("No webhook url configured");
                var payload = new { task.Id, task.Title, task.DueAt };
                if (_webhookSender == null) throw new InvalidOperationException("Webhook sender not available");
                await _webhookSender.SendAsync(to, payload, cancellationToken);
            }
        }
    }
}