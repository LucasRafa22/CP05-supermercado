using System.Globalization;
using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Supermercado.API.Middlewares;
using Supermercado.Application.Interfaces;
using Supermercado.Infrastructure.Data;
using Supermercado.Infrastructure.Repositories;
using System.Threading.RateLimiting;

namespace Supermercado.API;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ============================================================
        // CONTROLLERS
        // ============================================================
        builder.Services.AddControllers();

        // ============================================================
        // API VERSIONING
        // ============================================================
        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(2, 0);

                options.AssumeDefaultVersionWhenUnspecified = true;

                options.ReportApiVersions = true;

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
        // DATABASE - ORACLE
        // ============================================================
        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("SupermercadoApiTests");
            });
        }
        else
        {
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                var connectionString =
                    builder.Configuration.GetConnectionString(
                        "RecommendaContextOracle");

                options.UseOracle(connectionString);
            });
        }

        // ============================================================
        // DEPENDENCY INJECTION
        // ============================================================
        builder.Services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        builder.Services.AddScoped<
            IProdutoRepository,
            ProdutoRepository>();

        // ============================================================
        // SWAGGER
        // ============================================================
        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Supermercado API",
                    Version = "v1",
                    Description = "API do Supermercado - Versão 1 (Deprecated)"
                });

            options.SwaggerDoc(
                "v2",
                new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Supermercado API",
                    Version = "v2",
                    Description = "API do Supermercado - Versão 2"
                });
        });

        // ============================================================
        // RATE LIMITING
        // ============================================================
        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(
                            retryAfter.TotalSeconds))
                        .ToString(
                            CultureInfo.InvariantCulture);
                }
                else
                {
                    context.HttpContext.Response.Headers.RetryAfter = "60";
                }

                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                context.HttpContext.Response.ContentType =
                    "application/json";

                var response = new
                {
                    type = "https://httpstatuses.com/429",
                    title = "Limite de requisições excedido",
                    status = 429,
                    detail =
                        "O limite de requisições para este endpoint foi excedido. Tente novamente após o período informado no header Retry-After."
                };

                await context.HttpContext.Response.WriteAsJsonAsync(
                    response,
                    cancellationToken);
            };

            options.AddFixedWindowLimiter(
                "produto-write",
                limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;

                    limiterOptions.Window =
                        TimeSpan.FromMinutes(1);

                    limiterOptions.QueueLimit = 0;

                    limiterOptions.QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst;

                    limiterOptions.AutoReplenishment = true;
                });
        });

        // ============================================================
        // HEALTH CHECK
        // ============================================================
        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>("database")
            .AddCheck(
                "self",
                () => HealthCheckResult.Healthy(
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
        // MIDDLEWARE
        // ============================================================
        app.UseHttpsRedirection();

        app.UseMiddleware<ExceptionHandlerMiddleware>();

        app.UseRouting();

        app.UseRateLimiter();

        // ============================================================
        // HEALTH CHECK
        // ============================================================
        // Não possui RequireRateLimiting, portanto permanece
        // disponível mesmo após atingir o limite da API.
        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                Predicate = _ => true
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
        // RUN
        // ============================================================
        app.Run();
    }
}