using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;

namespace DoktarPlanning.Infrastructure.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}