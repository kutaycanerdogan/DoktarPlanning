using AutoMapper;

using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DoktarPlanning.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;
        private readonly IMapper _mapper;

        public AuthService(IRepository<User> userRepo, IConfiguration config, ILogger<AuthService> logger, IMapper mapper)
        {
            _userRepo = userRepo;
            _config = config;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<TokenDto> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                throw new InvalidOperationException("Invalid credentials.");

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiresAt = DateTime.Now.AddDays(7);
            await _userRepo.UpdateAsync(user);

            return GenerateToken(user);
        }

        public async Task<TokenDto> SignUpAsync(UserDto dto, CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<User>(dto);
            user.Id = Guid.NewGuid();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiresAt = DateTime.Now.AddDays(7);
            user.IsActive = true;
            await _userRepo.AddAsync(user);
            return GenerateToken(user);
        }

        public async Task<TokenDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiresAt < DateTime.Now)
                throw new InvalidOperationException("Invalid or expired refresh token.");

            user.RefreshToken = GenerateRefreshToken();
            user.RefreshTokenExpiresAt = DateTime.Now.AddDays(7);
            await _userRepo.UpdateAsync(user);

            return GenerateToken(user);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
                return;

            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;

            await _userRepo.UpdateAsync(user);
        }

        private TokenDto GenerateToken(User user)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiryMinutes = int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60");

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.DisplayName ?? string.Empty)
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.Now.AddMinutes(expiryMinutes), signingCredentials: creds);
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return new TokenDto
            {
                Token = tokenStr,
                AccessToken = tokenStr,
                RefreshToken = user.RefreshToken,
                ExpiresAt = token.ValidTo
            };
        }
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}