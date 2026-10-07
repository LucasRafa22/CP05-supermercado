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

    // ============================================================
    // V1
    // Retorna todos os produtos.
    // Mantido para preservar o comportamento antigo da API.
    // ============================================================
    public async Task<List<Produto>> GetAllAsync()
    {
        return await _context.Set<Produto>()
            .ToListAsync();
    }

    // ============================================================
    // PAGINAÇÃO - V2
    // Retorna um IQueryable para que o EF Core monte a consulta
    // diretamente no banco de dados.
    // ============================================================
    public IQueryable<Produto> GetQueryable()
    {
        return _context.Set<Produto>()
            .AsNoTracking();
    }

    // ============================================================
    // BUSCAR POR ID
    // ============================================================
    public async Task<Produto?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Produto>()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // ============================================================
    // CRIAR
    // ============================================================
    public async Task AddAsync(Produto produto)
    {
        await _context.Set<Produto>()
            .AddAsync(produto);

        await _context.SaveChangesAsync();
    }

    // ============================================================
    // ATUALIZAR
    // ============================================================
    public async Task UpdateAsync(Produto produto)
    {
        _context.Set<Produto>()
            .Update(produto);

        await _context.SaveChangesAsync();
    }

    // ============================================================
    // EXCLUIR
    // ============================================================
    public async Task DeleteAsync(Guid id)
    {
        var produto = await GetByIdAsync(id);

        if (produto == null)
            return;

        _context.Set<Produto>()
            .Remove(produto);

        await _context.SaveChangesAsync();
    }
}