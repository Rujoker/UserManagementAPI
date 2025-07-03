namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    public TokenAuthenticationMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Unauthorized. Missing or invalid token.\"}");
            return;
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var validTokens = new[] { "mysecrettoken123", "another-valid-token" };
        if (!validTokens.Contains(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Unauthorized. Invalid token.\"}");
            return;
        }

        await _next(context);
    }
}