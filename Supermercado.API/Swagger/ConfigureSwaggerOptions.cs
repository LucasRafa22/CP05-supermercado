using Asp.Versioning.ApiExplorer;

using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Supermercado.API.Swagger;

public class ConfigureSwaggerOptions
    : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(
        SwaggerGenOptions options)
    {
        foreach (
            var description
            in _provider.ApiVersionDescriptions)
        {
            var info =
                CreateInfoForApiVersion(
                    description);

            options.SwaggerDoc(
                description.GroupName,
                info);
        }

        options.DocInclusionPredicate(
            (documentName, apiDescription) =>
                apiDescription.GroupName ==
                documentName);
    }

    private static OpenApiInfo
        CreateInfoForApiVersion(
            ApiVersionDescription description)
    {
        var isDeprecated =
            description.IsDeprecated;

        var info = new OpenApiInfo
        {
            Title =
                "Supermercado API",

            Version =
                description.ApiVersion.ToString(),

            Description =
                isDeprecated
                    ? "API do Supermercado - " +
                      "VERSÃO DEPRECATED. " +
                      "Utilize a V2."
                    : "API do Supermercado - " +
                      "versão atual."
        };

        return info;
    }
}