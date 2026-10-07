using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Supermercado.API.Swagger;

public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated = apiDescription.IsDeprecated();

        foreach (var response in apiDescription.SupportedResponseTypes)
        {
            if (response.StatusCode == 200)
            {
                continue;
            }

            if (!operation.Responses.ContainsKey(
                    response.StatusCode.ToString()))
            {
                operation.Responses.Add(
                    response.StatusCode.ToString(),
                    new OpenApiResponse
                    {
                        Description = "Resposta da API"
                    });
            }
        }
    }
}