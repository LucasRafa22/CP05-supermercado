using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Supermercado.Application.DTOs.Produto;
using Supermercado.Application.Interfaces;
using Supermercado.Domain.Entities;
using Asp.Versioning;

namespace Supermercado.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProdutoController : ControllerBase
{
    private readonly IProdutoRepository _repo;
    private readonly ILogger<ProdutoController> _logger;

    public ProdutoController(
        IProdutoRepository repo,
        ILogger<ProdutoController> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    // ============================================================
    // V1 - LISTAR PRODUTOS
    // ============================================================
    /// <summary>
    /// Lista todos os produtos - V1.
    /// A V1 não possui paginação para manter compatibilidade
    /// com o contrato antigo da API.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> GetAllV1()
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /produtos V1 iniciado | TraceId: {TraceId}",
            traceId);

        var result = await _repo.GetAllAsync();

        _logger.LogInformation(
            "GET /produtos V1 finalizado | TraceId: {TraceId} | Total: {Count}",
            traceId,
            result.Count);

        return Ok(result);
    }

    // ============================================================
    // V2 - LISTAR PRODUTOS COM PAGINAÇÃO
    // ============================================================
    /// <summary>
    /// Lista produtos com paginação - V2.
    /// </summary>
    /// <param name="page">
    /// Número da página. Padrão: 1.
    /// </param>
    /// <param name="pageSize">
    /// Quantidade de itens por página. Padrão: 20. Máximo: 100.
    /// </param>
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult> GetAllV2(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /produtos V2 iniciado | TraceId: {TraceId} | Page: {Page} | PageSize: {PageSize}",
            traceId,
            page,
            pageSize);

        // ========================================================
        // VALIDAÇÃO DA PÁGINA
        // ========================================================
        if (page < 1)
        {
            return BadRequest(new
            {
                type = "https://httpstatuses.com/400",
                title = "Parâmetro inválido",
                status = 400,
                detail = "O parâmetro 'page' deve ser maior ou igual a 1."
            });
        }

        // ========================================================
        // VALIDAÇÃO DO TAMANHO DA PÁGINA
        // ========================================================
        if (pageSize < 1)
        {
            return BadRequest(new
            {
                type = "https://httpstatuses.com/400",
                title = "Parâmetro inválido",
                status = 400,
                detail = "O parâmetro 'pageSize' deve ser maior ou igual a 1."
            });
        }

        if (pageSize > 100)
        {
            return BadRequest(new
            {
                type = "https://httpstatuses.com/400",
                title = "Parâmetro inválido",
                status = 400,
                detail = "O parâmetro 'pageSize' não pode ser maior que 100."
            });
        }

        // ========================================================
        // QUERYABLE
        // ========================================================
        var query = _repo.GetQueryable();

        // ========================================================
        // TOTAL DE REGISTROS
        // ========================================================
        // Executado no banco com COUNT.
        var totalItems = await query.CountAsync();

        // ========================================================
        // TOTAL DE PÁGINAS
        // ========================================================
        var totalPages = (int)Math.Ceiling(
            totalItems / (double)pageSize);

        // ========================================================
        // PAGINAÇÃO
        // ========================================================
        // OrderBy -> define ordem estável
        // Skip    -> pula os registros das páginas anteriores
        // Take    -> pega somente os registros da página atual
        //
        // Tudo isso é convertido pelo EF Core para SQL.
        // ========================================================
        var items = await query
            .OrderBy(p => p.Nome)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // ========================================================
        // RESPOSTA PAGINADA
        // ========================================================
        var response = new
        {
            page,
            pageSize,
            totalItems,
            totalPages,
            items
        };

        _logger.LogInformation(
            "GET /produtos V2 finalizado | TraceId: {TraceId} | Page: {Page} | PageSize: {PageSize} | TotalItems: {TotalItems} | TotalPages: {TotalPages}",
            traceId,
            page,
            pageSize,
            totalItems,
            totalPages);

        return Ok(response);
    }

    // ============================================================
    // BUSCAR POR ID
    // ============================================================
    /// <summary>
    /// Busca produto por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /produtos/{Id} iniciado | TraceId: {TraceId}",
            id,
            traceId);

        var produto = await _repo.GetByIdAsync(id);

        if (produto == null)
        {
            _logger.LogWarning(
                "Produto não encontrado | TraceId: {TraceId} | Id: {Id}",
                traceId,
                id);

            return NotFound();
        }

        _logger.LogInformation(
            "Produto encontrado | TraceId: {TraceId} | Id: {Id}",
            traceId,
            id);

        return Ok(produto);
    }

    // ============================================================
    // CRIAR PRODUTO
    // ============================================================
    /// <summary>
    /// Cria produto.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> Create(ProdutoCreateDto dto)
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "POST /produtos iniciado | TraceId: {TraceId}",
            traceId);

        var produto = new Produto(
            dto.Nome,
            dto.Preco,
            dto.Estoque,
            dto.CategoriaId);

        await _repo.AddAsync(produto);

        _logger.LogInformation(
            "Produto criado | TraceId: {TraceId} | Id: {Id}",
            traceId,
            produto.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = produto.Id },
            produto);
    }

    // ============================================================
    // ATUALIZAR PRODUTO
    // ============================================================
    /// <summary>
    /// Atualiza produto.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        ProdutoCreateDto dto)
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "PUT /produtos/{Id} iniciado | TraceId: {TraceId}",
            id,
            traceId);

        var produto = await _repo.GetByIdAsync(id);

        if (produto == null)
        {
            _logger.LogWarning(
                "Update falhou - produto não encontrado | TraceId: {TraceId} | Id: {Id}",
                traceId,
                id);

            return NotFound();
        }

        produto.UpdateNome(dto.Nome);
        produto.UpdatePreco(dto.Preco);
        produto.UpdateEstoque(dto.Estoque);

        await _repo.UpdateAsync(produto);

        _logger.LogInformation(
            "Produto atualizado | TraceId: {TraceId} | Id: {Id}",
            traceId,
            id);

        return NoContent();
    }

    // ============================================================
    // EXCLUIR PRODUTO
    // ============================================================
    /// <summary>
    /// Remove produto.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "DELETE /produtos/{Id} iniciado | TraceId: {TraceId}",
            id,
            traceId);

        var produto = await _repo.GetByIdAsync(id);

        if (produto == null)
        {
            _logger.LogWarning(
                "Delete falhou - produto não encontrado | TraceId: {TraceId} | Id: {Id}",
                traceId,
                id);

            return NotFound();
        }

        await _repo.DeleteAsync(id);

        _logger.LogInformation(
            "Produto removido | TraceId: {TraceId} | Id: {Id}",
            traceId,
            id);

        return NoContent();
    }
}