// <copyright file="TestEmailController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.Controllers;

using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lynx.Abstractions.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controller for test email functionality that forwards to NotificationService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class TestEmailController : ControllerBase
{
    private readonly HttpClient httpClient;
    private readonly ILogger<TestEmailController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEmailController"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for downstream calls.</param>
    /// <param name="logger">The logger instance.</param>
    public TestEmailController(HttpClient httpClient, ILogger<TestEmailController> logger)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Forwards test email request to NotificationService.
    /// </summary>
    /// <param name="request">The test email request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from NotificationService.</returns>
    [HttpPost]
    public async Task<ActionResult<TestEmailResponse>> SendTestEmail(
        [FromBody] TestEmailRequest request,
        CancellationToken cancellationToken)
    {
        using var scope = this.logger.BeginScope("TestEmailForward:{To}", request?.To);
        using var userScope = this.logger.BeginScope("User:{UserId}", this.User.Identity?.Name);

        this.logger.LogInformation("Forwarding test email request to NotificationService");

        if (request is null)
        {
            this.logger.LogWarning("Test email request was null");
            return this.BadRequest("Request cannot be null");
        }

        try
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await this.httpClient.PostAsync("api/testemail", content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<TestEmailResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                this.logger.LogInformation("Successfully forwarded test email request");
                return this.Accepted(result);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                this.logger.LogWarning("NotificationService returned BadRequest: {Error}", errorContent);
                return this.BadRequest(errorContent);
            }
            else
            {
                this.logger.LogError("NotificationService returned error: {StatusCode}", response.StatusCode);
                return this.Problem(
                    title: "Downstream service error",
                    detail: "NotificationService is unavailable",
                    statusCode: 502);
            }
        }
        catch (HttpRequestException ex)
        {
            this.logger.LogError(ex, "HTTP request to NotificationService failed");
            return this.Problem(
                title: "Service unavailable",
                detail: "NotificationService is unavailable",
                statusCode: 502);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            this.logger.LogError(ex, "Request to NotificationService timed out");
            return this.Problem(
                title: "Request timeout",
                detail: "NotificationService request timed out",
                statusCode: 500);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Unexpected error forwarding test email request");
            return this.Problem(
                title: "Internal server error",
                detail: "An unexpected error occurred",
                statusCode: 500);
        }
    }
}
