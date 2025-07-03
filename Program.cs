using UserManagementAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
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
app.UseMiddleware<ErrorHandlingMiddleware>();

// 2. Authentication (token validation) middleware (should be second)
app.UseMiddleware<TokenAuthenticationMiddleware>();

// 3. Logging middleware (should be last)
app.UseMiddleware<RequestResponseLoggingMiddleware>();

app.MapControllers();

app.Run();

record User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}