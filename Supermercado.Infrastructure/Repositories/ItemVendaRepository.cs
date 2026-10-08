using Microsoft.EntityFrameworkCore;

using Supermercado.Application.Interfaces;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Data;

namespace Supermercado.Infrastructure.Repositories;

public class ItemVendaRepository : IItemVendaRepository
{
    private readonly ApplicationDbContext _context;

    public ItemVendaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ItemVenda>> GetAllAsync()
    {
        return await _context
            .Set<ItemVenda>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ItemVenda?> GetByIdAsync(Guid id)
    {
        return await _context
            .Set<ItemVenda>()
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddAsync(ItemVenda itemVenda)
    {
        await _context
            .Set<ItemVenda>()
            .AddAsync(itemVenda);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(ItemVenda itemVenda)
    {
        _context
            .Set<ItemVenda>()
            .Update(itemVenda);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var itemVenda = await GetByIdAsync(id);

        if (itemVenda == null)
        {
            return;
        }

        _context
            .Set<ItemVenda>()
            .Remove(itemVenda);

        await _context.SaveChangesAsync();
    }
}