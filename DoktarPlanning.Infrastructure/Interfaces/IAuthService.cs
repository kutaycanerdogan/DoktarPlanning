using DoktarPlanning.Infrastructure.DTOs;

namespace DoktarPlanning.Infrastructure.Interfaces
{
    public interface IAuthService
    {
        Task<TokenDto> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<TokenDto> SignUpAsync(UserDto dto, CancellationToken cancellationToken = default);
        Task<TokenDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    }
}