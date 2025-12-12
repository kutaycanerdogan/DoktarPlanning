using DoktarPlanning.Api.ViewModels;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace DoktarPlanning.Api.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("signin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var token = await _auth.SignInAsync(request.Email, request.Password, cancellationToken);
            return Ok(token);
        }

        [HttpPost("signup")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userDto = new DoktarPlanning.Infrastructure.DTOs.UserDto
            {
                Email = request.Email,
                DisplayName = request.DisplayName,
                Password = request.Password
            };

            var token = await _auth.SignUpAsync(userDto, cancellationToken);
            return Created(string.Empty, token);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            var token = await _auth.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            return Ok(token);
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            await _auth.RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            return NoContent();
        }
    }
}