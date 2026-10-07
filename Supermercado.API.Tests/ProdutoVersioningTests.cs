using System.Net;
using System.Text.Json;
using Xunit;

namespace Supermercado.API.Tests;

public class ProdutoVersioningTests :
    IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProdutoVersioningTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task V1_DeveRetornar200()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=1.0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Array,
            document.RootElement.ValueKind);
    }

    [Fact]
    public async Task V2_DeveRetornar200()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Object,
            document.RootElement.ValueKind);

        Assert.True(
            document.RootElement.TryGetProperty(
                "page",
                out _));

        Assert.True(
            document.RootElement.TryGetProperty(
                "pageSize",
                out _));

        Assert.True(
            document.RootElement.TryGetProperty(
                "totalItems",
                out _));

        Assert.True(
            document.RootElement.TryGetProperty(
                "totalPages",
                out _));

        Assert.True(
            document.RootElement.TryGetProperty(
                "items",
                out _));
    }

    [Fact]
    public async Task Header_DeveSelecionarV1()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/Produto");

        request.Headers.Add(
            "X-Api-Version",
            "1.0");

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Array,
            document.RootElement.ValueKind);
    }

    [Fact]
    public async Task Header_DeveSelecionarV2()
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/api/Produto");

        request.Headers.Add(
            "X-Api-Version",
            "2.0");

        var response = await _client.SendAsync(request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Object,
            document.RootElement.ValueKind);
    }

    [Fact]
    public async Task SemVersao_DeveUsarV2()
    {
        var response = await _client.GetAsync(
            "/api/Produto");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        Assert.Equal(
            JsonValueKind.Object,
            document.RootElement.ValueKind);

        Assert.True(
            document.RootElement.TryGetProperty(
                "page",
                out _));
    }

    [Fact]
    public async Task DeveInformarVersoesSuportadas()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=2.0");

        Assert.True(
            response.Headers.Contains(
                "api-supported-versions"));

        var supportedVersions =
            string.Join(
                ",",
                response.Headers.GetValues(
                    "api-supported-versions"));

        Assert.Contains("1.0", supportedVersions);
        Assert.Contains("2.0", supportedVersions);
    }

    [Fact]
    public async Task DeveInformarV1ComoDepreciada()
    {
        var response = await _client.GetAsync(
            "/api/Produto?api-version=1.0");

        Assert.True(
            response.Headers.Contains(
                "api-deprecated-versions"));

        var deprecatedVersions =
            string.Join(
                ",",
                response.Headers.GetValues(
                    "api-deprecated-versions"));

        Assert.Contains("1.0", deprecatedVersions);
    }
}