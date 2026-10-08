using Microsoft.EntityFrameworkCore;

using Supermercado.Application.Interfaces;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Data;

namespace Supermercado.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly ApplicationDbContext _context;

    public CategoriaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> GetAllAsync()
    {
        return await _context
            .Set<Categoria>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Categoria?> GetByIdAsync(Guid id)
    {
        return await _context
            .Set<Categoria>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Categoria categoria)
    {
        await _context
            .Set<Categoria>()
            .AddAsync(categoria);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Categoria categoria)
    {
        _context
            .Set<Categoria>()
            .Update(categoria);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var categoria = await GetByIdAsync(id);

        if (categoria == null)
        {
            return;
        }

        _context
            .Set<Categoria>()
            .Remove(categoria);

        await _context.SaveChangesAsync();
    }
}