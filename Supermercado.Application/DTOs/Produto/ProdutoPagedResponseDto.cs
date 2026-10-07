using ProdutoEntity = Supermercado.Domain.Entities.Produto;

namespace Supermercado.Application.DTOs.Produto;

public class ProdutoPagedResponseDto
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }

    public List<ProdutoEntity> Items { get; set; } = new();
}