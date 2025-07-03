namespace UserManagementAPI.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    public RequestResponseLoggingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        var request = context.Request;
        Console.WriteLine($"[{DateTime.UtcNow}] HTTP {request.Method} {request.Path}");

        // Copy original response body stream
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // Log response
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        Console.WriteLine($"[{DateTime.UtcNow}] Response {context.Response.StatusCode}: {responseText}");

        // Copy the contents of the new memory stream (which contains the response) to the original stream
        await responseBody.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }
}