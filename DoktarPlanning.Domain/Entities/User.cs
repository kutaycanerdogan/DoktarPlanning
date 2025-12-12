using DoktarPlanning.Domain.Common;

namespace DoktarPlanning.Domain.Entities
{
    public class User : IEntity
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string? DisplayName { get; set; }
        public string PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public bool IsActive { get; set; }

        public ICollection<TaskItem>? Tasks { get; set; }
        public ICollection<RecurrenceRule>? RecurrenceRules { get; set; }
    }
}