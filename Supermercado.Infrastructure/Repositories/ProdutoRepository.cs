using Microsoft.EntityFrameworkCore;
using Supermercado.Application.Interfaces;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Data;

namespace Supermercado.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly ApplicationDbContext _context;

    public ProdutoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Produto>> GetAllAsync()
    {
        return await _context
            .Set<Produto>()
            .AsNoTracking()
            .ToListAsync();
    }

    public IQueryable<Produto> GetQueryable()
    {
        return _context
            .Set<Produto>()
            .AsNoTracking();
    }

    public async Task<Produto?> GetByIdAsync(Guid id)
    {
        return await _context
            .Set<Produto>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Produto produto)
    {
        await _context
            .Set<Produto>()
            .AddAsync(produto);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Produto produto)
    {
        _context
            .Set<Produto>()
            .Update(produto);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var produto = await GetByIdAsync(id);

        if (produto == null)
        {
            return;
        }

        _context
            .Set<Produto>()
            .Remove(produto);

        await _context.SaveChangesAsync();
    }

    public async Task<(int TotalItems, List<Produto> Items)> GetPagedAsync(
        int page,
        int pageSize)
    {
        var query = _context
            .Set<Produto>()
            .AsNoTracking();

        // Total de registros
        var totalItems = await query.CountAsync();

        // Paginação feita no banco
        var items = await query
            .OrderBy(p => p.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (totalItems, items);
    }
}