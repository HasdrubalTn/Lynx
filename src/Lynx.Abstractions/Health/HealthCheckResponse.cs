namespace Lynx.Abstractions.Health;

public sealed class HealthCheckResponse
{
    public HealthStatus Status { get; init; } = HealthStatus.Healthy;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public Dictionary<string, HealthStatus> Dependencies { get; init; } = new();
    public string? Error { get; init; }
}
