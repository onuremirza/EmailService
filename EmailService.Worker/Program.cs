using EmailService.Application;
using EmailService.Infrastructure;
using EmailService.Worker.Extensions;
using EmailService.Worker.Health;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConsumerHealthState>();

builder.Services.AddHealthChecks()
    .AddCheck<WorkerLivenessHealthCheck>("worker_live", tags: new[] { "live" })
    .AddCheck<WorkerReadinessHealthCheck>("worker_ready", tags: new[] { "ready" });

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddWorkerServices();

WebApplication app = builder.Build();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = WriteJson
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = WriteJson
});

app.Run();

static async Task WriteJson(HttpContext ctx, HealthReport report)
{
    ctx.Response.ContentType = "application/json";

    ConsumerHealthState state = ctx.RequestServices.GetRequiredService<ConsumerHealthState>();

    DateTime? lastMessageUtc = state.LastMessageUtc == DateTime.MinValue
    ? null
    : state.LastMessageUtc;

    var payload = new
    {
        status = report.Status.ToString(),
        consumer = new
        {
            started = state.Started,
            consuming = state.Consuming,
            lastMessageUtc = lastMessageUtc,
            lastError = state.LastError
        },
        checks = report.Entries.Select(e => new
        {
            name = e.Key,
            status = e.Value.Status.ToString(),
            description = e.Value.Description,
            durationMs = (long)e.Value.Duration.TotalMilliseconds
        })
    };

    await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
}
