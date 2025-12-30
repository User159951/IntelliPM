using IntelliPM.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IntelliPM.Infrastructure.Persistence;

public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken: ct);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.ToListAsync(ct);
    }

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _dbSet.CountAsync(ct);
    }

    public IQueryable<TEntity> Query()
    {
        return _dbSet.AsQueryable();
    }
}

