using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IReminderService
    {
        Task ScheduleReminderAsync(Guid userId, Guid taskId, DateTime remindAt, ReminderChannel channel, string? channelTarget = null, CancellationToken cancellationToken = default);
        Task ScheduleReminderAsync(Guid userId, TaskItem task, DateTime remindAt, ReminderChannel channel, string? channelTarget = null, CancellationToken cancellationToken = default);
        Task CancelReminderAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
        Task SendReminderNowAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    }
}