using System.Globalization;
using System.Threading.RateLimiting;

using Asp.Versioning;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

using Supermercado.API.Middlewares;
using Supermercado.API.Swagger;
using Supermercado.Application.Interfaces;
using Supermercado.Application.Services;
using Supermercado.Infrastructure.Data;
using Supermercado.Infrastructure.Repositories;

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
                // V2 é a versão padrão
                options.DefaultApiVersion =
                    new ApiVersion(2, 0);

                // Sem versão informada -> V2
                options.AssumeDefaultVersionWhenUnspecified =
                    true;

                // Informa versões suportadas/depreciadas
                options.ReportApiVersions =
                    true;

                // Permite:
                // ?api-version=1.0
                // X-Api-Version: 1.0
                options.ApiVersionReader =
                    ApiVersionReader.Combine(
                        new QueryStringApiVersionReader(
                            "api-version"),

                        new HeaderApiVersionReader(
                            "X-Api-Version")
                    );
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat =
                    "'v'VVV";

                options.SubstituteApiVersionInUrl =
                    false;
            });

        // ============================================================
        // DATABASE
        // ============================================================

        if (builder.Environment.IsEnvironment("Testing"))
        {
            builder.Services.AddDbContext<ApplicationDbContext>(
                options =>
                {
                    options.UseInMemoryDatabase(
                        "SupermercadoApiTests");
                });
        }
        else
        {
            builder.Services.AddDbContext<ApplicationDbContext>(
                options =>
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

        builder.Services.AddScoped<
            ICategoriaRepository,
            CategoriaRepository>();

        builder.Services.AddScoped<
            IClienteRepository,
            ClienteRepository>();

        builder.Services.AddScoped<
            IItemVendaRepository,
            ItemVendaRepository>();

        builder.Services.AddScoped<
            IVendaRepository,
            VendaRepository>();

        builder.Services.AddScoped<ProdutoService>();

        // ============================================================
        // SWAGGER
        // ============================================================

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.OperationFilter<
                SwaggerDefaultValues>();
        });

        builder.Services.AddTransient<
            IConfigureOptions<SwaggerGenOptions>,
            ConfigureSwaggerOptions>();

        // ============================================================
        // RATE LIMITING
        // ============================================================

        builder.Services.AddRateLimiter(options =>
        {
            options.OnRejected = async (
                context,
                cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out var retryAfter))
                {
                    context.HttpContext
                        .Response
                        .Headers
                        .RetryAfter =
                        ((int)Math.Ceiling(
                            retryAfter.TotalSeconds))
                        .ToString(
                            CultureInfo.InvariantCulture);
                }
                else
                {
                    context.HttpContext
                        .Response
                        .Headers
                        .RetryAfter = "60";
                }

                context.HttpContext
                    .Response
                    .StatusCode =
                    StatusCodes
                        .Status429TooManyRequests;

                context.HttpContext
                    .Response
                    .ContentType =
                    "application/problem+json";

                var response = new
                {
                    type =
                        "https://httpstatuses.com/429",

                    title =
                        "Limite de requisições excedido",

                    status = 429,

                    detail =
                        "O limite de requisições para " +
                        "este endpoint foi excedido. " +
                        "Tente novamente após o período " +
                        "informado no header Retry-After."
                };

                await context.HttpContext
                    .Response
                    .WriteAsJsonAsync(
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

                    limiterOptions.AutoReplenishment =
                        true;
                });
        });

        // ============================================================
        // HEALTH CHECK
        // ============================================================

        builder.Services
            .AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(
                "database")
            .AddCheck(
                "self",
                () =>
                    HealthCheckResult.Healthy(
                        "API está funcionando"));

        // ============================================================
        // BUILD
        // ============================================================

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
                    "Supermercado API V1 - Deprecated");

                options.SwaggerEndpoint(
                    "/swagger/v2/swagger.json",
                    "Supermercado API V2");
            });
        }

        // ============================================================
        // PIPELINE
        // ============================================================

        app.UseHttpsRedirection();

        app.UseMiddleware<
            ExceptionHandlerMiddleware>();

        app.UseRouting();
        
        app.UseRateLimiter();

        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api/Produto"))
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["api-supported-versions"] =
                        "1.0, 2.0";

                    return Task.CompletedTask;
                });
            }

            await next();
        });

// ============================================================
// HEALTH
// ============================================================

        app.MapHealthChecks(
            "/health",
            new HealthCheckOptions
            {
                Predicate = _ => true
            });

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
        
    }
}