using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Supermercado.Application.DTOs.Produto;
using Supermercado.Application.Interfaces;
using Supermercado.Application.Services;

namespace Supermercado.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
public class ProdutoController : ControllerBase
{
    private readonly IProdutoRepository _repo;
    private readonly ProdutoService _service;
    private readonly ILogger<ProdutoController> _logger;

    public ProdutoController(
        IProdutoRepository repo,
        ProdutoService service,
        ILogger<ProdutoController> logger)
    {
        _repo = repo;
        _service = service;
        _logger = logger;
    }

    // ============================================================
    // GET ALL - V1
    // ============================================================

    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> GetAllV1()
    {
        var traceId =
            HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /api/Produto V1 iniciado | TraceId: {TraceId}",
            traceId);

        var result =
            await _repo.GetAllAsync();

        _logger.LogInformation(
            "GET /api/Produto V1 finalizado | TraceId: {TraceId} | Total: {Count}",
            traceId,
            result.Count);

        return Ok(result);
    }

    // ============================================================
    // GET ALL - V2
    // ============================================================

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult> GetAllV2(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var traceId =
            HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /api/Produto V2 iniciado | TraceId: {TraceId} | Page: {Page} | PageSize: {PageSize}",
            traceId,
            page,
            pageSize);

        try
        {
            var response =
                await _service
                    .GetProdutosPaginadosAsync(
                        page,
                        pageSize);

            _logger.LogInformation(
                "GET /api/Produto V2 finalizado | TraceId: {TraceId} | TotalItems: {TotalItems}",
                traceId,
                response.TotalItems);

            return Ok(new
            {
                page =
                    response.Page,

                pageSize =
                    response.PageSize,

                totalItems =
                    response.TotalItems,

                totalPages =
                    response.TotalPages,

                items =
                    response.Items
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                "Paginação inválida | TraceId: {TraceId} | Detail: {Detail}",
                traceId,
                ex.Message);

            return BadRequest(new
            {
                type =
                    "https://httpstatuses.com/400",

                title =
                    "Parâmetro inválido",

                status = 400,

                detail =
                    ex.Message
            });
        }
    }

    // ============================================================
    // GET BY ID
    // ============================================================

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(
        Guid id)
    {
        var produto =
            await _repo.GetByIdAsync(id);

        if (produto == null)
        {
            return NotFound();
        }

        return Ok(produto);
    }

    // ============================================================
    // POST
    // ============================================================

    [HttpPost]
    [EnableRateLimiting("produto-write")]
    public async Task<ActionResult> Create(
        ProdutoCreateDto dto)
    {
        var produto =
            new Supermercado.Domain.Entities.Produto(
                dto.Nome,
                dto.Preco,
                dto.Estoque,
                dto.CategoriaId);

        await _repo.AddAsync(produto);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = produto.Id
            },
            produto);
    }

    // ============================================================
    // PUT
    // ============================================================

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(
        Guid id,
        ProdutoCreateDto dto)
    {
        var produto =
            await _repo.GetByIdAsync(id);

        if (produto == null)
        {
            return NotFound();
        }

        produto.UpdateNome(dto.Nome);
        produto.UpdatePreco(dto.Preco);
        produto.UpdateEstoque(dto.Estoque);

        await _repo.UpdateAsync(produto);

        return NoContent();
    }

    // ============================================================
    // DELETE
    // ============================================================

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(
        Guid id)
    {
        var produto =
            await _repo.GetByIdAsync(id);

        if (produto == null)
        {
            return NotFound();
        }

        await _repo.DeleteAsync(id);

        return NoContent();
    }
}