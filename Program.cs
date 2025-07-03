var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 1. Error-handling middleware (should be first)
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        var errorResponse = System.Text.Json.JsonSerializer.Serialize(new { error = "Internal server error." });
        await context.Response.WriteAsync(errorResponse);

        // Optional: log the exception details
        Console.WriteLine($"[{DateTime.UtcNow}] Unhandled Exception: {ex}");
    }
});

// 2. Authentication (token validation) middleware (should be second)
app.Use(async (context, next) =>
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

    await next();
});

// 3. Logging middleware (should be last)
app.Use(async (context, next) =>
{
    // Log request
    var request = context.Request;
    Console.WriteLine($"[{DateTime.UtcNow}] HTTP {request.Method} {request.Path}");

    // Copy original response body stream
    var originalBodyStream = context.Response.Body;
    using var responseBody = new MemoryStream();
    context.Response.Body = responseBody;

    await next();

    // Log response
    context.Response.Body.Seek(0, SeekOrigin.Begin);
    var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
    context.Response.Body.Seek(0, SeekOrigin.Begin);

    Console.WriteLine($"[{DateTime.UtcNow}] Response {context.Response.StatusCode}: {responseText}");

    // Copy the contents of the new memory stream (which contains the response) to the original stream
    await responseBody.CopyToAsync(originalBodyStream);
    context.Response.Body = originalBodyStream;
});

var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Email = "alice@example.com" },
    new User { Id = 2, Name = "Bob", Email = "bob@example.com" }
};

// GET: Retrieve all users
app.MapGet("/users", () =>
{
    try
    {
        // Return a copy to avoid exposing the internal list
        return Results.Ok(users.ToList());
    }
    catch
    {
        return Results.Problem("Failed to retrieve users.");
    }
})
.WithName("GetUsers")
.WithOpenApi();

// GET: Retrieve a specific user by ID
app.MapGet("/users/{id:int}", (int id) =>
{
    try
    {
        // Use Dictionary for O(1) lookup if performance is critical
        var user = users.FirstOrDefault(u => u.Id == id);
        return user is not null ? Results.Ok(user) : Results.NotFound();
    }
    catch
    {
        return Results.Problem("Failed to retrieve user.");
    }
})
.WithName("GetUserById")
.WithOpenApi();

// POST: Add a new user
app.MapPost("/users", (User user) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
            return Results.BadRequest("Name and Email are required.");

        if (!user.Email.Contains('@'))
            return Results.BadRequest("Invalid email format.");

        user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
        users.Add(user);
        return Results.Created($"/users/{user.Id}", user);
    }
    catch
    {
        return Results.Problem("Failed to create user.");
    }
})
.WithName("CreateUser")
.WithOpenApi();

// PUT: Update an existing user's details
app.MapPut("/users/{id:int}", (int id, User updatedUser) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(updatedUser.Name) || string.IsNullOrWhiteSpace(updatedUser.Email))
            return Results.BadRequest("Name and Email are required.");

        if (!updatedUser.Email.Contains('@'))
            return Results.BadRequest("Invalid email format.");

        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound();

        user.Name = updatedUser.Name;
        user.Email = updatedUser.Email;
        return Results.Ok(user);
    }
    catch
    {
        return Results.Problem("Failed to update user.");
    }
})
.WithName("UpdateUser")
.WithOpenApi();

// DELETE: Remove a user by ID
app.MapDelete("/users/{id:int}", (int id) =>
{
    try
    {
        var user = users.FirstOrDefault(u => u.Id == id);
        if (user is null) return Results.NotFound();

        users.Remove(user);
        return Results.NoContent();
    }
    catch
    {
        return Results.Problem("Failed to delete user.");
    }
})
.WithName("DeleteUser")
.WithOpenApi();

app.Run();

record User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}