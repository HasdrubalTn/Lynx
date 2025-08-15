using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lynx.Abstractions.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace NotificationService.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class HealthController(
    IConfiguration configuration,
    ILogger<HealthController> logger) : ControllerBase
{
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(5);

    [HttpGet("health")]
    public IActionResult Health()
    {
        using var _ = logger.BeginScope("HealthCheck:{Service}", "NotificationService");
        logger.LogInformation("Health check requested");

        return Ok(new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> ReadyAsync(CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope("HealthCheck:{Service}", "NotificationService");
        logger.LogInformation("Readiness check requested");

        var postgresqlStatus = await CheckPostgreSqlHealthAsync(cancellationToken);

        var response = new HealthCheckResponse
        {
            Status = postgresqlStatus,
            Dependencies = new Dictionary<string, HealthStatus>
            {
                ["postgresql"] = postgresqlStatus
            }
        };

        var statusCode = response.Status == HealthStatus.Healthy ? 200 : 503;
        return StatusCode(statusCode, response);
    }

    private async Task<HealthStatus> CheckPostgreSqlHealthAsync(CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("DependencyCheck:{Dependency}", "PostgreSQL");
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example";

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await using var connection = new NpgsqlConnection(connectionString);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_timeout);

            await connection.OpenAsync(timeoutCts.Token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(timeoutCts.Token);
            stopwatch.Stop();

            logger.LogInformation("Dependency check completed: PostgreSQL - {Status} - {ResponseTime}ms",
                HealthStatus.Healthy, stopwatch.ElapsedMilliseconds);
            return HealthStatus.Healthy;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Dependency check timeout: PostgreSQL - {Error}", "TimeoutError");
            return HealthStatus.Unhealthy;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Dependency check connection failure: PostgreSQL - {Error}", ex.Message);
            return HealthStatus.Unhealthy;
        }
    }
}
