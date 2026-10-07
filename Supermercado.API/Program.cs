using Microsoft.EntityFrameworkCore;
using Supermercado.Infrastructure.Data;
using Supermercado.Application.Interfaces;
using Supermercado.Infrastructure.Repositories;
using Supermercado.API.Middlewares;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning;

namespace Supermercado.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================================
        // VERSIONAMENTO DA API
        // ============================================================
        builder.Services
            .AddApiVersioning(options =>
            {
                // V2 é a versão padrão
                options.DefaultApiVersion = new ApiVersion(2, 0);

                // Quando a versão não for informada, utiliza V2
                options.AssumeDefaultVersionWhenUnspecified = true;

                // Retorna os headers:
                // api-supported-versions
                // api-deprecated-versions
                options.ReportApiVersions = true;

                // Permite informar a versão por:
                // ?api-version=1.0
                // X-Api-Version: 1.0
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Api-Version")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = false;
            });

        // ============================================================
        // CONTROLLERS
        // ============================================================
        builder.Services.AddControllers();

        // ============================================================
        // SWAGGER
        // ============================================================
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen();

        // Gera um Swagger separado para cada versão
        builder.Services.AddTransient<
            IConfigureOptions<SwaggerGenOptions>,
            ConfigureSwaggerOptions>();

        // ============================================================
        // BANCO DE DADOS ORACLE
        // ============================================================
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString =
                builder.Configuration.GetConnectionString(
                    "RecommendaContextOracle");

            options.UseOracle(connectionString);
        });

        // ============================================================
        // REPOSITÓRIO GENÉRICO
        // ============================================================
        builder.Services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        // ============================================================
        // REPOSITÓRIO DE PRODUTO
        // ============================================================
        builder.Services.AddScoped<
            IProdutoRepository,
            ProdutoRepository>();

        // ============================================================
        // HEALTH CHECKS
        // ============================================================
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database")
            .AddCheck(
                "self",
                () =>
                    HealthCheckResult.Healthy(
                        "API está funcionando"));

        var app = builder.Build();

        // ============================================================
        // SWAGGER
        // ============================================================
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    "/swagger/v1/swagger.json",
                    "Supermercado API V1");

                options.SwaggerEndpoint(
                    "/swagger/v2/swagger.json",
                    "Supermercado API V2");
            });
        }

        // ============================================================
        // HTTPS
        // ============================================================
        app.UseHttpsRedirection();

        // ============================================================
        // TRATAMENTO GLOBAL DE EXCEÇÕES
        // ============================================================
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        // ============================================================
        // HEALTH CHECK
        // ============================================================
        // O /health permanece independente dos demais endpoints.
        app.MapHealthChecks(
            "/health",
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType =
                        "application/json";

                    var result = new
                    {
                        status = report.Status.ToString(),

                        totalDuration =
                            report.TotalDuration.TotalMilliseconds,

                        checks = report.Entries.Select(x => new
                        {
                            name = x.Key,

                            status =
                                x.Value.Status.ToString(),

                            duration =
                                x.Value.Duration.TotalMilliseconds,

                            error =
                                x.Value.Exception?.Message
                        })
                    };

                    await context.Response
                        .WriteAsJsonAsync(result);
                }
            });

        // ============================================================
        // AUTHORIZATION
        // ============================================================
        app.UseAuthorization();

        // ============================================================
        // CONTROLLERS
        // ============================================================
        app.MapControllers();

        // ============================================================
        // EXECUÇÃO
        // ============================================================
        app.Run();
    }
}