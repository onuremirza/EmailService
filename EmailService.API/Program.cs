using EmailService.Application;
using EmailService.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Swagger + Controller
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Email Service API", Version = "v1" });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    _ = options.AddPolicy("EmailRateLimit", context =>
        RateLimitPartition.GetFixedWindowLimiter<string>(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            key => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            }));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        _ = policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Config Cache
builder.Services.AddMemoryCache();

// DI Services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

WebApplication app = builder.Build();

// Middleware order matters
app.UseRateLimiter();
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email Service API v1");
    });
}

app.MapControllers();
app.Run();
