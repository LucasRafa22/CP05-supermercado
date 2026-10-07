using System.Net;

using Supermercado.API.Exceptions;

namespace Supermercado.API.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlerMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                ex);
        }
    }

    private static async Task
        HandleExceptionAsync(
            HttpContext context,
            Exception exception)
    {
        var statusCode =
            exception switch
            {
                ArgumentException =>
                    (int)HttpStatusCode.BadRequest,

                KeyNotFoundException =>
                    (int)HttpStatusCode.NotFound,

                InvalidOperationException =>
                    (int)HttpStatusCode.BadRequest,

                _ =>
                    (int)HttpStatusCode.InternalServerError
            };

        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();

        context.Response.StatusCode =
            statusCode;

        context.Response.ContentType =
            "application/problem+json";

        var problem =
            ProblemDetailsFactory.Create(
                exception,
                context,
                statusCode);

        await context.Response
            .WriteAsJsonAsync(problem);
    }
}