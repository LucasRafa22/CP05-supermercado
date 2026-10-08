using Microsoft.EntityFrameworkCore;

using Supermercado.Application.Interfaces;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Data;

namespace Supermercado.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly ApplicationDbContext _context;

    public ClienteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> GetAllAsync()
    {
        return await _context
            .Set<Cliente>()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Cliente?> GetByIdAsync(Guid id)
    {
        return await _context
            .Set<Cliente>()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Cliente cliente)
    {
        await _context
            .Set<Cliente>()
            .AddAsync(cliente);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Cliente cliente)
    {
        _context
            .Set<Cliente>()
            .Update(cliente);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var cliente = await GetByIdAsync(id);

        if (cliente == null)
        {
            return;
        }

        _context
            .Set<Cliente>()
            .Remove(cliente);

        await _context.SaveChangesAsync();
    }
}