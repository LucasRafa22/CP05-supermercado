using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Supermercado.API.Tests;

public class RateLimitingTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RateLimitingTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveRetornar429AoExcederLimite()
    {
        var dto = new
        {
            nome = "Produto Rate Limit",
            preco = 10.50m,
            estoque = 10,
            categoriaId = Guid.NewGuid()
        };

        HttpResponseMessage? ultimaResposta = null;

        for (var i = 0; i < 11; i++)
        {
            ultimaResposta = await _client.PostAsJsonAsync(
                "/api/Produto?api-version=2.0",
                dto);
        }

        Assert.NotNull(ultimaResposta);

        Assert.Equal(
            (HttpStatusCode)429,
            ultimaResposta!.StatusCode);
    }

    [Fact]
    public async Task DeveRetornarRetryAfterAoExcederLimite()
    {
        var dto = new
        {
            nome = "Produto Retry After",
            preco = 15.90m,
            estoque = 20,
            categoriaId = Guid.NewGuid()
        };

        HttpResponseMessage? ultimaResposta = null;

        for (var i = 0; i < 11; i++)
        {
            ultimaResposta = await _client.PostAsJsonAsync(
                "/api/Produto?api-version=2.0",
                dto);
        }

        Assert.NotNull(ultimaResposta);

        Assert.Equal(
            (HttpStatusCode)429,
            ultimaResposta!.StatusCode);

        Assert.True(
            ultimaResposta.Headers.Contains(
                "Retry-After"));

        var retryAfter =
            ultimaResposta.Headers.GetValues(
                "Retry-After").First();

        Assert.False(
            string.IsNullOrWhiteSpace(retryAfter));
    }

    [Fact]
    public async Task DeveRetornarJsonAoExcederLimite()
    {
        var dto = new
        {
            nome = "Produto JSON Rate Limit",
            preco = 20.00m,
            estoque = 20,
            categoriaId = Guid.NewGuid()
        };

        HttpResponseMessage? ultimaResposta = null;

        for (var i = 0; i < 11; i++)
        {
            ultimaResposta = await _client.PostAsJsonAsync(
                "/api/Produto?api-version=2.0",
                dto);
        }

        Assert.NotNull(ultimaResposta);

        Assert.Equal(
            (HttpStatusCode)429,
            ultimaResposta!.StatusCode);

        var json =
            await ultimaResposta.Content.ReadAsStringAsync();

        Assert.Contains(
            "Limite de requisições",
            json,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "\"status\":429",
            json,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HealthDeveContinuarFuncionandoAposRateLimit()
    {
        var dto = new
        {
            nome = "Produto Health",
            preco = 30.00m,
            estoque = 30,
            categoriaId = Guid.NewGuid()
        };

        HttpResponseMessage? ultimaResposta = null;

        for (var i = 0; i < 11; i++)
        {
            ultimaResposta = await _client.PostAsJsonAsync(
                "/api/Produto?api-version=2.0",
                dto);
        }

        Assert.NotNull(ultimaResposta);

        Assert.Equal(
            (HttpStatusCode)429,
            ultimaResposta!.StatusCode);

        var healthResponse =
            await _client.GetAsync("/health");

        Assert.Equal(
            HttpStatusCode.OK,
            healthResponse.StatusCode);
    }
}