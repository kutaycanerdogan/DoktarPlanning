using System.Security.Claims;

namespace DoktarPlanning.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetUserId(this ClaimsPrincipal user)
        {
            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? user.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (Guid.TryParse(sub, out var id)) return id;
            throw new InvalidOperationException("User id claim not found or invalid.");
        }
    }
}