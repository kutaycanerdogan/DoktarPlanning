using DoktarPlanning.Infrastructure.DTOs;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserDto dto, CancellationToken cancellationToken = default);
        Task<UserDto> UpdateProfileAsync(Guid userId, UserDto dto, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}