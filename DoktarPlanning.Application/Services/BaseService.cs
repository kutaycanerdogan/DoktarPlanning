using AutoMapper;

using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Interfaces;

using Microsoft.Extensions.Logging;

using System.Linq.Expressions;

namespace DoktarPlanning.Application.Services
{
    public abstract class BaseService<TEntity>
        where TEntity : class, IEntity, new()
    {
        protected readonly IRepository<TEntity> _repository;
        protected readonly IMapper _mapper;
        protected readonly ILogger _baseServiceLogger;

        protected BaseService(IRepository<TEntity> repository, IMapper mapper, ILogger baseServiceLogger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _baseServiceLogger = baseServiceLogger ?? throw new ArgumentNullException(nameof(baseServiceLogger));
        }

        protected virtual Task ValidateCreateAsync(Guid userId, object dto, CancellationToken cancellationToken) => Task.CompletedTask;
        protected virtual Task ValidateUpdateAsync(Guid userId, Guid id, object dto, CancellationToken cancellationToken) => Task.CompletedTask;
        protected virtual Task BeforeCreateAsync(Guid userId, TEntity entity, CancellationToken cancellationToken) => Task.CompletedTask;
        protected virtual Task AfterCreateAsync(Guid userId, TEntity entity, CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual async Task<TDto> CreateAsync<TDto>(Guid userId, TDto dto, CancellationToken cancellationToken = default)
            where TDto : class
        {
            await ValidateCreateAsync(userId, dto, cancellationToken);

            var entity = _mapper.Map<TEntity>(dto);
            var userProp = typeof(TEntity).GetProperty("UserId");
            if (userProp != null && userProp.PropertyType == typeof(Guid))
                userProp.SetValue(entity, userId);

            await BeforeCreateAsync(userId, entity, cancellationToken);
            var added = await _repository.AddAsync(entity);
            await AfterCreateAsync(userId, added, cancellationToken);

            _baseServiceLogger.LogInformation("Created {Entity} {Id}", typeof(TEntity).Name, added.Id);
            return _mapper.Map<TDto>(added);
        }

        public virtual async Task<TDto> UpdateAsync<TDto>(Guid userId, Guid id, TDto dto, CancellationToken cancellationToken = default)
            where TDto : class
        {
            await ValidateUpdateAsync(userId, id, dto, cancellationToken);

            var existing = await _repository.FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null) throw new InvalidOperationException($"{typeof(TEntity).Name} not found.");

            _mapper.Map(dto, existing);
            existing = await _repository.UpdateAsync(existing);

            _baseServiceLogger.LogInformation("Updated {Entity} {Id}", typeof(TEntity).Name, id);
            return _mapper.Map<TDto>(existing);
        }

        public virtual async Task<TDto> DeleteAsync<TDto>(Guid userId, Guid id, CancellationToken cancellationToken = default)
        {
            var existing = await _repository.FirstOrDefaultAsync(e => e.Id == id);
            if (existing == null) throw new InvalidOperationException($"{typeof(TEntity).Name} not found.");
            await _repository.RemoveAsync(existing);
            _baseServiceLogger.LogInformation("Deleted {Entity} {Id}", typeof(TEntity).Name, id);
            return _mapper.Map<TDto>(existing);
        }

        public virtual async Task<TDto> GetByIdAsync<TDto>(Guid userId, Guid id, CancellationToken cancellationToken = default)
            where TDto : class
        {
            var entity = await _repository.FirstOrDefaultAsync(e => e.Id == id);
            if (entity == null) throw new InvalidOperationException($"{typeof(TEntity).Name} not found.");
            return _mapper.Map<TDto>(entity);
        }

        public virtual async Task<IEnumerable<TDto>> ListAsync<TDto>(Guid userId,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            int? skip = null, int? take = null,
            CancellationToken cancellationToken = default)
            where TDto : class
        {
            var items = await _repository.FindAsync(predicate, orderBy, skip, take, cancellationToken);
            return _mapper.Map<IEnumerable<TDto>>(items);
        }
    }
}