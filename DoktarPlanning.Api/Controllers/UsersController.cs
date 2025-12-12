using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoktarPlanning.Api.Controllers
{
    [Authorize]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _users;

        public UsersController(IUserService users)
        {
            _users = users;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var dto = await _users.GetByIdAsync(userId, cancellationToken);
            return Ok(dto);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userId = GetUserId();
            var updated = await _users.UpdateProfileAsync(userId, dto, cancellationToken);
            return Ok(updated);
        }

        [HttpDelete("me")]
        public async Task<IActionResult> DeleteProfile(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _users.DeleteAsync(userId, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var dto = await _users.GetByIdAsync(id, cancellationToken);
            return Ok(dto);
        }
    }
}