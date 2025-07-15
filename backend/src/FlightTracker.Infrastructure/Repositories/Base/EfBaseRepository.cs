using System.Linq.Expressions;
using FlightTracker.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories.Base;

/// <summary>
/// Generic EF Core repository base class implementing common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public abstract class EfBaseRepository<TEntity, TKey> : IBaseRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly FlightDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger _logger;

    protected EfBaseRepository(FlightDbContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by ID {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        int? skip = null,
        int? take = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Include related properties
            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            var results = await query.AsNoTracking().ToListAsync(cancellationToken);
            return results.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TEntity?> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "",
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<TEntity> query = _dbSet;

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            // Include related properties
            foreach (var includeProperty in includeProperties.Split(
                new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first {EntityType} entity", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (predicate == null)
            {
                return await _dbSet.CountAsync(cancellationToken);
            }
            return await _dbSet.CountAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added {EntityType} entity", typeof(TEntity).Name);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding {EntityType} entity", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<IReadOnlyList<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            var entityList = entities.ToList();
            await _dbSet.AddRangeAsync(entityList, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Added {Count} {EntityType} entities", entityList.Count, typeof(TEntity).Name);
            return entityList.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding range of {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Updated {EntityType} entity", typeof(TEntity).Name);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} entity", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FindAsync(new object[] { id! }, cancellationToken);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Deleted {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Deleted {EntityType} entity", typeof(TEntity).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} entity", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            if (entities.Any())
            {
                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogDebug("Deleted {Count} {EntityType} entities", entities.Count, typeof(TEntity).Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting range of {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }
}
