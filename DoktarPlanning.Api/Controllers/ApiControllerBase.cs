using Microsoft.AspNetCore.Mvc;

using System.Security.Claims;

namespace DoktarPlanning.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected Guid GetUserId()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (Guid.TryParse(sub, out var userId)) return userId;
            throw new InvalidOperationException("User id claim not found or invalid.");
        }
    }
}