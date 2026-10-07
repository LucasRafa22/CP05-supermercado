using Supermercado.Domain.Entities;

namespace Supermercado.Application.Interfaces;

public interface IProdutoRepository
{
    Task<List<Produto>> GetAllAsync();

    Task<Produto?> GetByIdAsync(Guid id);

    Task AddAsync(Produto produto);

    Task UpdateAsync(Produto produto);

    Task DeleteAsync(Guid id);

    IQueryable<Produto> GetQueryable();

    Task<(int TotalItems, List<Produto> Items)> GetPagedAsync(
        int page,
        int pageSize);
}