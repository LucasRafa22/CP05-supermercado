using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Supermercado.Domain.Entities;
using Supermercado.Infrastructure.Data;

namespace Supermercado.API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string DatabaseName = "SupermercadoApiTests";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(DatabaseName);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        db.Produtos.AddRange(
            new Produto(
                "Arroz",
                25.90m,
                50,
                Guid.NewGuid()),

            new Produto(
                "Feijão",
                8.50m,
                100,
                Guid.NewGuid()),

            new Produto(
                "Macarrão",
                5.90m,
                80,
                Guid.NewGuid()),

            new Produto(
                "Leite",
                6.99m,
                70,
                Guid.NewGuid()),

            new Produto(
                "Café",
                18.90m,
                40,
                Guid.NewGuid())
        );

        db.SaveChanges();

        return host;
    }
}