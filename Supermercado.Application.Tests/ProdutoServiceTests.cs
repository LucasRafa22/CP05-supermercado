using Moq;
using Supermercado.Application.Interfaces;
using Supermercado.Application.Services;
using Supermercado.Domain.Entities;
using Xunit;

namespace Supermercado.Application.Tests;

public class ProdutoServiceTests
{
    [Fact]
    public async Task AddProduto_Com_Preco_Valido_Deve_Chamar_Repository()
    {
        var repositoryMock =
            new Mock<IRepository<Produto>>();

        var produtoRepositoryMock =
            new Mock<IProdutoRepository>();

        var service = new ProdutoService(
            repositoryMock.Object,
            produtoRepositoryMock.Object);

        var produto = new Produto(
            "Arroz",
            10,
            5,
            Guid.NewGuid());

        await service.AddProdutoAsync(produto);

        repositoryMock.Verify(
            r => r.Add(produto),
            Times.Once);
    }

    [Fact]
    public async Task DeleteProduto_Inexistente_Deve_Lancar_Excecao()
    {
        var repositoryMock =
            new Mock<IRepository<Produto>>();

        var produtoRepositoryMock =
            new Mock<IProdutoRepository>();

        repositoryMock
            .Setup(r => r.GetById(It.IsAny<Guid>()))
            .ReturnsAsync((Produto?)null);

        var service = new ProdutoService(
            repositoryMock.Object,
            produtoRepositoryMock.Object);

        var ex = await Assert.ThrowsAsync<Exception>(
            () => service.DeleteProdutoAsync(Guid.NewGuid()));

        Assert.Equal(
            "Produto não encontrado",
            ex.Message);

        repositoryMock.Verify(
            r => r.Delete(It.IsAny<Guid>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetProdutosPaginados_Com_Parametros_Invalidos_Deve_Lancar_Excecao(
        int page,
        int pageSize)
    {
        var repositoryMock =
            new Mock<IRepository<Produto>>();

        var produtoRepositoryMock =
            new Mock<IProdutoRepository>();

        var service = new ProdutoService(
            repositoryMock.Object,
            produtoRepositoryMock.Object);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetProdutosPaginadosAsync(
                page,
                pageSize));

        produtoRepositoryMock.Verify(
            r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProdutosPaginados_Com_Parametros_Validos_Deve_Retornar_Dados_Paginados()
    {
        var repositoryMock =
            new Mock<IRepository<Produto>>();

        var produtoRepositoryMock =
            new Mock<IProdutoRepository>();

        var produtos = new List<Produto>
        {
            new Produto(
                "Arroz",
                10,
                10,
                Guid.NewGuid()),

            new Produto(
                "Feijão",
                8,
                20,
                Guid.NewGuid())
        };

        produtoRepositoryMock
            .Setup(r => r.GetPagedAsync(1, 2))
            .ReturnsAsync((2, produtos));

        var service = new ProdutoService(
            repositoryMock.Object,
            produtoRepositoryMock.Object);

        var result =
            await service.GetProdutosPaginadosAsync(1, 2);

        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(2, result.Items.Count);

        produtoRepositoryMock.Verify(
            r => r.GetPagedAsync(1, 2),
            Times.Once);
    }
}