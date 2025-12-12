using DoktarPlanning.Domain.Entities;

namespace DoktarPlanning.Infrastructure.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string Password { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public bool IsActive { get; set; }


    }
}
