using Microsoft.EntityFrameworkCore;

using Supermercado.Application.Interfaces;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Data;

namespace Supermercado.Infrastructure.Repositories;

public class VendaRepository : IVendaRepository
{
    private readonly ApplicationDbContext _context;

    public VendaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venda>> GetAllAsync()
    {
        return await _context
            .Set<Venda>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Venda?> GetByIdAsync(Guid id)
    {
        return await _context
            .Set<Venda>()
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(Venda venda)
    {
        await _context
            .Set<Venda>()
            .AddAsync(venda);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Venda venda)
    {
        _context
            .Set<Venda>()
            .Update(venda);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var venda = await GetByIdAsync(id);

        if (venda == null)
        {
            return;
        }

        _context
            .Set<Venda>()
            .Remove(venda);

        await _context.SaveChangesAsync();
    }
}