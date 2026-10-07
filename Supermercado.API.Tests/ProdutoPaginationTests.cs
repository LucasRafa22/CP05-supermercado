using System.Net;
using System.Text.Json;
using Xunit;

namespace Supermercado.API.Tests;

public class ProdutoPaginationTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProdutoPaginationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeveUsarPageInformada()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0&page=2&pageSize=2");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.Equal(
            2,
            root.GetProperty("page").GetInt32());

        Assert.Equal(
            2,
            root.GetProperty("pageSize").GetInt32());
    }

    [Fact]
    public async Task DeveUsarPageSizeInformado()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0&page=1&pageSize=3");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.Equal(
            3,
            root.GetProperty("pageSize").GetInt32());

        var items = root.GetProperty("items");

        Assert.True(
            items.GetArrayLength() <= 3);
    }

    [Fact]
    public async Task PageSizeMaiorQue100_DeveRetornar400()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0&pageSize=101");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "pageSize",
            json,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PageMenorQue1_DeveRetornar400()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0&page=0");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "page",
            json,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PaginaForaDoIntervalo_DeveRetornar200ComListaVazia()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0&page=999&pageSize=20");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        var items = root.GetProperty("items");

        Assert.Equal(
            JsonValueKind.Array,
            items.ValueKind);

        Assert.Empty(items.EnumerateArray());
    }

    [Fact]
    public async Task DeveRetornarMetadadosDaPaginacao()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0&page=1&pageSize=2");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.True(
            root.GetProperty("totalItems").GetInt32() >= 5);

        Assert.True(
            root.GetProperty("totalPages").GetInt32() >= 3);

        Assert.Equal(
            1,
            root.GetProperty("page").GetInt32());

        Assert.Equal(
            2,
            root.GetProperty("pageSize").GetInt32());
    }
}