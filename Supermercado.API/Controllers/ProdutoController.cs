using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// V1 - Lista todos os produtos.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> GetAllV1()
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /api/Produto V1 iniciado | TraceId: {TraceId}",
            traceId);

        var result = await _repo.GetAllAsync();

        _logger.LogInformation(
            "GET /api/Produto V1 finalizado | TraceId: {TraceId} | Total: {Count}",
            traceId,
            result.Count);

        return Ok(result);
    }

    /// <summary>
    /// V2 - Lista produtos.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult> GetAllV2()
    {
        var traceId = HttpContext.TraceIdentifier;

        _logger.LogInformation(
            "GET /api/Produto V2 iniciado | TraceId: {TraceId}",
            traceId);

        var result = await _repo.GetAllAsync();

        _logger.LogInformation(
            "GET /api/Produto V2 finalizado | TraceId: {TraceId} | Total: {Count}",
            traceId,
            result.Count);

        return Ok(result);
    }

    /// <summary>
    /// Busca produto por ID.
    /// Disponível nas versões da API.
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

    /// <summary>
    /// Cria produto.
    /// Disponível nas versões da API.
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

    /// <summary>
    /// Atualiza produto.
    /// Disponível nas versões da API.
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

    /// <summary>
    /// Remove produto.
    /// Disponível nas versões da API.
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