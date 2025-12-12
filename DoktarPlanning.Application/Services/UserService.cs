using AutoMapper;

using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

namespace DoktarPlanning.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IRepository<User> repo, IMapper mapper, ILogger<UserService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserDto> RegisterAsync(UserDto dto, CancellationToken cancellationToken = default)
        {
            var user = _mapper.Map<User>(dto);
            user.Id = Guid.NewGuid();
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var added = await _repo.AddAsync(user);
            var outDto = _mapper.Map<UserDto>(added);
            outDto.Password = string.Empty;
            return outDto;
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UserDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _repo.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("User not found.");
            user.DisplayName = dto.DisplayName ?? user.DisplayName;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _repo.UpdateAsync(user);
            var outDto = _mapper.Map<UserDto>(user);
            outDto.Password = string.Empty;
            return outDto;
        }

        public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _repo.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("User not found.");
            await _repo.RemoveAsync(user);
        }

        public async Task<UserDto> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _repo.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) throw new InvalidOperationException("User not found.");
            var outDto = _mapper.Map<UserDto>(user);
            outDto.Password = string.Empty;
            return outDto;
        }

        public async Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _repo.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) throw new InvalidOperationException("User not found.");
            var outDto = _mapper.Map<UserDto>(user);
            outDto.Password = string.Empty;
            return outDto;
        }
    }
}