using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailService.Worker.Health;

public sealed class WorkerLivenessHealthCheck(ConsumerHealthState state) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(state.Started
                ? HealthCheckResult.Healthy("Consumer started.")
                : HealthCheckResult.Degraded("Consumer not started yet."));
    }
}
