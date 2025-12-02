using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailService.Worker.Health;

public sealed class WorkerReadinessHealthCheck(ConsumerHealthState state) : IHealthCheck
{
    // mesaj gelmeyebilir; bu yüzden "silence" kontrolünü Degraded yaptım
    private static readonly TimeSpan MaxSilence = TimeSpan.FromMinutes(10);

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!state.Started)
            return Task.FromResult(HealthCheckResult.Unhealthy("Consumer not started."));

        if (!state.Consuming)
            return Task.FromResult(HealthCheckResult.Unhealthy($"Not consuming. LastError={state.LastError}"));

        if (state.LastMessageUtc == DateTime.MinValue)
            return Task.FromResult(HealthCheckResult.Healthy("Consuming (no messages yet)."));

        TimeSpan silence = DateTime.UtcNow - state.LastMessageUtc;
        if (silence > MaxSilence)
            return Task.FromResult(HealthCheckResult.Degraded($"No messages for {silence:g}. LastError={state.LastError}"));

        return Task.FromResult(HealthCheckResult.Healthy("Ready."));
    }
}
