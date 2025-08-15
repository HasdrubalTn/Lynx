// <copyright file="HealthController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Lynx.Abstractions.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

[ApiController]
[Route("[controller]")]
public sealed class HealthController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<HealthController> logger) : ControllerBase
{
    private readonly TimeSpan timeout = TimeSpan.FromSeconds(5);

    [HttpGet("health")]
    public IActionResult Health()
    {
        using var _ = logger.BeginScope("HealthCheck:{Service}", "ApiGateway");
        logger.LogInformation("Health check requested");

        return this.Ok(new HealthCheckResponse
        {
            Status = HealthStatus.Healthy,
            Timestamp = DateTime.UtcNow,
        });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> ReadyAsync(CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope("HealthCheck:{Service}", "ApiGateway");
        logger.LogInformation("Readiness check requested");

        var dependencies = new Dictionary<string, HealthStatus>();

        // Check IdentityService
        var identityStatus = await this.CheckServiceHealthAsync("IdentityService", "http://localhost:8081/health", cancellationToken);
        dependencies.Add("identityService", identityStatus);

        // Check NotificationService
        var notificationStatus = await this.CheckServiceHealthAsync("NotificationService", "http://localhost:8082/health", cancellationToken);
        dependencies.Add("notificationService", notificationStatus);

        // Check PostgreSQL
        var postgresqlStatus = await this.CheckPostgreSqlHealthAsync(cancellationToken);
        dependencies.Add("postgresql", postgresqlStatus);

        var response = new HealthCheckResponse
        {
            Status = dependencies.Values.All(s => s == HealthStatus.Healthy)
                ? HealthStatus.Healthy
                : HealthStatus.Unhealthy,
            Timestamp = DateTime.UtcNow,
            Dependencies = dependencies,
        };

        var statusCode = response.Status == HealthStatus.Healthy ? 200 : 503;
        return this.StatusCode(statusCode, response);
    }

    private async Task<HealthStatus> CheckServiceHealthAsync(string serviceName, string healthUrl, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("DependencyCheck:{Dependency}", serviceName);
        var httpClient = httpClientFactory.CreateClient();

        try
        {
            var stopwatch = Stopwatch.StartNew();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(this.timeout);

            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, healthUrl), timeoutCts.Token);
            stopwatch.Stop();

            var status = response.IsSuccessStatusCode ? HealthStatus.Healthy : HealthStatus.Unhealthy;
            logger.LogInformation(
                "Dependency check completed: {Service} - {Status} - {ResponseTime}ms",
                serviceName,
                status,
                stopwatch.ElapsedMilliseconds);

            return status;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Dependency check timeout: {Service} - {Error}", serviceName, "TimeoutError");
            return HealthStatus.Unhealthy;
        }
        catch (HttpRequestException ex)
        {
            logger.LogWarning("Dependency check connection failure: {Service} - {Error}", serviceName, ex.Message);
            return HealthStatus.Unhealthy;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Dependency check unexpected error: {Service}", serviceName);
            return HealthStatus.Unhealthy;
        }
    }

    private async Task<HealthStatus> CheckPostgreSqlHealthAsync(CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope("DependencyCheck:{Dependency}", "PostgreSQL");
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example";

        try
        {
            var stopwatch = Stopwatch.StartNew();
            await using var connection = new NpgsqlConnection(connectionString);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(this.timeout);

            await connection.OpenAsync(timeoutCts.Token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(timeoutCts.Token);
            stopwatch.Stop();

            logger.LogInformation(
                "Dependency check completed: PostgreSQL - {Status} - {ResponseTime}ms",
                HealthStatus.Healthy,
                stopwatch.ElapsedMilliseconds);
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
