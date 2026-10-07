using Microsoft.EntityFrameworkCore;

using Supermercado.Application.Interfaces;
using Supermercado.Infrastructure.Data;

namespace Supermercado.Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<List<T>> GetAll()
    {
        return await _dbSet
            .ToListAsync();
    }

    public async Task<T?> GetById(Guid id)
    {
        return await _dbSet
            .FindAsync(id);
    }

    public async Task Add(T entity)
    {
        await _dbSet
            .AddAsync(entity);

        await _context
            .SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var entity =
            await _dbSet.FindAsync(id);

        if (entity != null)
        {
            _dbSet.Remove(entity);

            await _context
                .SaveChangesAsync();
        }
    }
}