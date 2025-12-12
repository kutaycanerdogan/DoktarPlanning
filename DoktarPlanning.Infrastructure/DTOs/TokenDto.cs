namespace DoktarPlanning.Infrastructure.DTOs
{
    public class TokenDto
    {
        public string Token { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
