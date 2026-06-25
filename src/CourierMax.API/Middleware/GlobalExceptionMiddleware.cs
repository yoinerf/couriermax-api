using System.Net;
using System.Text.Json;
using CourierMax.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace CourierMax.API.Middleware;

/// <summary>
/// Middleware de manejo global de excepciones.
/// Retorna RFC 7807 Problem Details para todas las excepciones no manejadas.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {Code} — {Message}", ex.Code, ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status422UnprocessableEntity,
                ex.Code, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await WriteProblemDetailsAsync(context, StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR", "An unexpected error occurred. Please try again later.");
        }
    }

    private static async Task WriteProblemDetailsAsync(
        HttpContext context, int statusCode, string type, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Type = $"https://couriermax.api/errors/{type.ToLowerInvariant()}",
            Title = GetTitle(statusCode),
            Detail = detail
        };

        problem.Extensions["errorCode"] = type;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        422 => "Business Rule Violation",
        404 => "Not Found",
        400 => "Bad Request",
        409 => "Conflict",
        _ => "Internal Server Error"
    };
}
