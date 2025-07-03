using System.Text.Json;

namespace UserManagementAPI.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var errorResponse = JsonSerializer.Serialize(new { error = "Internal server error." });
            await context.Response.WriteAsync(errorResponse);

            // Optional: log the exception details
            Console.WriteLine($"[{DateTime.UtcNow}] Unhandled Exception: {ex}");
        }
    }
}