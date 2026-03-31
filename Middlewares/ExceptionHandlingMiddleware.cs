namespace BusinessFlowApp.Middlewares;

using Microsoft.AspNetCore.Http;
using System.Text.Json;

/// <summary>
/// Middleware для глобальной обработки исключений
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Flow not found: {Message}", ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status404NotFound, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Internal server error: {Message}", ex.Message);
            await HandleExceptionAsync(context, StatusCodes.Status500InternalServerError, 
                new { error = "Internal server error", details = ex.Message });
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, int statusCode, object error)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(error, options);
        await context.Response.WriteAsync(json);
    }
}
