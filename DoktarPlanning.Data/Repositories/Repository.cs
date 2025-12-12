using DoktarPlanning.Data.Contexts;
using DoktarPlanning.Domain.Common;
using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

using System.Linq.Expressions;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DoktarPlanning.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public virtual IQueryable<T> GetQueryable()
        {
            return _dbSet.AsNoTracking();
        }

        public virtual async Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            return entity ?? throw new InvalidOperationException($"Entity of type {typeof(T).Name} with id {id} not found.");
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await GetQueryable().ToListAsync(cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            CancellationToken cancellationToken = default)
        {
            var query = GetQueryable();

            if (predicate != null) query = query.Where(predicate);
            if (orderBy != null) query = orderBy(query);
            if (skip.HasValue) query = query.Skip(skip.Value);
            if (take.HasValue) query = query.Take(take.Value);

            return await query.ToListAsync(cancellationToken);
        }

        public virtual async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null, CancellationToken cancellationToken = default)
        {
            var query = GetQueryable();
            if (include != null)
                query = include(query);
            var entity = await query.FirstOrDefaultAsync(predicate, cancellationToken);

            return entity ?? throw new InvalidOperationException($"No entity of type {typeof(T).Name} matches the given predicate.");
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity, cancellationToken);
            await CommitAsync(cancellationToken);
            return entity;
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            await _dbSet.AddRangeAsync(entities, cancellationToken);
            await CommitAsync(cancellationToken);
            return entities;
        }

        public virtual async Task RemoveAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            await CommitAsync(cancellationToken);
        }

        public virtual async Task RemoveRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            _dbSet.RemoveRange(entities);
            await CommitAsync(cancellationToken);
        }

        public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var tracked = _context.ChangeTracker.Entries<TaskItem>().FirstOrDefault(e => e.Entity.Id == entity.Id);

            if (tracked != null)
            {
                _context.Entry(tracked.Entity).CurrentValues.SetValues(entity);
            }
            else
            {
                _dbSet.Update(entity);
            }
            await CommitAsync(cancellationToken);
            return entity;
        }

        public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));

            _dbSet.UpdateRange(entities);
            await CommitAsync(cancellationToken);
        }

        private async Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    var result = await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }
    }
}