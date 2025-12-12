using AutoMapper;

using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;
using DoktarPlanning.Infrastructure.DTOs;
using DoktarPlanning.Infrastructure.Interfaces;
using DoktarPlanning.Infrastructure.Repositories.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DoktarPlanning.Application.Services
{
    public class SubTaskService : ISubTaskService
    {
        private readonly ISubTaskRepository _repo;
        private readonly IRepository<TaskItem> _taskRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<SubTaskService> _logger;

        public SubTaskService(ISubTaskRepository repo, IRepository<TaskItem> taskRepo, IMapper mapper, ILogger<SubTaskService> logger)
        {
            _repo = repo;
            _taskRepo = taskRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<SubTaskDto> CreateAsync(Guid userId, Guid taskId, SubTaskDto dto, CancellationToken cancellationToken = default)
        {
            var task = await _taskRepo.FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId);
            if (task == null) throw new InvalidOperationException("Task not found.");

            var entity = _mapper.Map<SubTask>(dto);
            entity.TaskItemId = taskId;
            var added = await _repo.AddAsync(entity);
            return _mapper.Map<SubTaskDto>(added);
        }

        public async Task<SubTaskDto> UpdateAsync(Guid userId, Guid taskId, Guid subTaskId, SubTaskDto dto, CancellationToken cancellationToken = default)
        {
            var sub = await _repo.FirstOrDefaultAsync(s => s.Id == subTaskId && s.TaskItemId == taskId && s.TaskItem.UserId == userId, x => x.Include(t => t.TaskItem));
            if (sub == null) throw new InvalidOperationException("Subtask not found.");
            if (sub.IsCompleted || sub.CompletedAt is null)
            {
                sub.CompletedAt = DateTime.Now;
            }
            _mapper.Map(dto, sub);
            await _repo.UpdateAsync(sub);
            return _mapper.Map<SubTaskDto>(sub);
        }

        public async Task DeleteAsync(Guid userId, Guid taskId, Guid subTaskId, CancellationToken cancellationToken = default)
        {
            var sub = await _repo.FirstOrDefaultAsync(s => s.Id == subTaskId && s.TaskItemId == taskId);
            if (sub == null) throw new InvalidOperationException("Subtask not found.");
            await _repo.RemoveAsync(sub);
        }

        public async Task<IEnumerable<SubTaskDto>> ListByTaskAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        {
            var list = await _repo.GetByTaskIdAsync(taskId, cancellationToken);
            return _mapper.Map<IEnumerable<SubTaskDto>>(list);
        }

        public async Task MarkCompleteAsync(Guid userId, Guid taskId, Guid subTaskId, bool isComplete, CancellationToken cancellationToken = default)
        {
            var sub = await _repo.FirstOrDefaultAsync(s => s.Id == subTaskId && s.TaskItemId == taskId);
            if (sub == null) throw new InvalidOperationException("Subtask not found.");
            sub.IsCompleted = isComplete;
            sub.CompletedAt = isComplete ? DateTime.Now : null;
            await _repo.UpdateAsync(sub);
        }
    }
}