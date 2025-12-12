using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace DoktarPlanning.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return user ?? throw new InvalidOperationException($"User with email '{email}' not found.");
        }
    }
}