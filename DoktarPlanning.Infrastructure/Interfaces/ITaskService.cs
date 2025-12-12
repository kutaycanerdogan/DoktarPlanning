using DoktarPlanning.Infrastructure.DTOs;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(Guid userId, TaskDto task, CancellationToken cancellationToken = default);
    Task<TaskDto> UpdateAsync(Guid userId, Guid taskId, TaskDto task, CancellationToken cancellationToken = default);
    Task<TaskDto> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);

    Task<TaskDto> GetByIdAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskDto>> ListAsync(Guid userId, TaskQueryParameters query, CancellationToken cancellationToken = default);

    Task MarkCompleteAsync(Guid userId, Guid taskId, bool isComplete, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskDto>> GetForDayAsync(Guid userId, DateTime day, CancellationToken cancellationToken = default);
}