using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;

namespace DoktarPlanning.Infrastructure.Repositories.Interfaces
{
    public interface ISubTaskRepository : IRepository<SubTask>
    {
        Task<IEnumerable<SubTask>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    }
}