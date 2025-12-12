using DoktarPlanning.Infrastructure.DTOs;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface ISubTaskService
    {
        Task<SubTaskDto> CreateAsync(Guid userId, Guid taskId, SubTaskDto dto, CancellationToken cancellationToken = default);
        Task<SubTaskDto> UpdateAsync(Guid userId, Guid taskId, Guid subTaskId, SubTaskDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userId, Guid taskId, Guid subTaskId, CancellationToken cancellationToken = default);
        Task<IEnumerable<SubTaskDto>> ListByTaskAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default);
        Task MarkCompleteAsync(Guid userId, Guid taskId, Guid subTaskId, bool isComplete, CancellationToken cancellationToken = default);
    }
}