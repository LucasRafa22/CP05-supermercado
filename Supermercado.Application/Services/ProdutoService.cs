using Supermercado.Application.DTOs.Produto;
using Supermercado.Application.Interfaces;
using ProdutoEntity = Supermercado.Domain.Entities.Produto;

namespace Supermercado.Application.Services;

public class ProdutoService
{
    private readonly IRepository<ProdutoEntity> _repository;
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(
        IRepository<ProdutoEntity> repository,
        IProdutoRepository produtoRepository)
    {
        _repository = repository;
        _produtoRepository = produtoRepository;
    }

    public async Task AddProdutoAsync(ProdutoEntity produto)
    {
        if (produto.Preco <= 0)
        {
            throw new Exception("Preço inválido");
        }

        await _repository.Add(produto);
    }

    public async Task DeleteProdutoAsync(Guid id)
    {
        var produto = await _repository.GetById(id);

        if (produto == null)
        {
            throw new Exception("Produto não encontrado");
        }

        await _repository.Delete(id);
    }

    public async Task<ProdutoPagedResponseDto> GetProdutosPaginadosAsync(
        int page,
        int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException(
                "O parâmetro 'page' deve ser maior ou igual a 1.",
                nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException(
                "O parâmetro 'pageSize' deve ser maior ou igual a 1.",
                nameof(pageSize));
        }

        if (pageSize > 100)
        {
            throw new ArgumentException(
                "O parâmetro 'pageSize' não pode ser maior que 100.",
                nameof(pageSize));
        }

        var resultado = await _produtoRepository.GetPagedAsync(
            page,
            pageSize);

        var totalPages = resultado.TotalItems == 0
            ? 0
            : (int)Math.Ceiling(
                resultado.TotalItems / (double)pageSize);

        return new ProdutoPagedResponseDto
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = resultado.TotalItems,
            TotalPages = totalPages,
            Items = resultado.Items
        };
    }
}