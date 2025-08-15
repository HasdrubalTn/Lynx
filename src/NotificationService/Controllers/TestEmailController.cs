// <copyright file="TestEmailController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace NotificationService.Controllers;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Lynx.Abstractions.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Controller for test email functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestEmailController : ControllerBase
{
    private readonly ILogger<TestEmailController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEmailController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TestEmailController(ILogger<TestEmailController> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends a test email with the specified details.
    /// </summary>
    /// <param name="request">The test email request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A response containing the message ID and timestamp.</returns>
    [HttpPost]
    public async Task<ActionResult<TestEmailResponse>> SendTestEmail(
        [FromBody] TestEmailRequest request,
        CancellationToken cancellationToken)
    {
        using var _ = this.logger.BeginScope("TestEmail:{To}", request?.To);

        if (request is null)
        {
            this.logger.LogWarning("Test email request was null");
            return this.BadRequest("Request cannot be null");
        }

        if (!this.IsValidEmailFormat(request.To))
        {
            this.logger.LogWarning("Invalid email format: {Email}", request.To);
            return this.BadRequest("Invalid email format");
        }

        if (string.IsNullOrWhiteSpace(request.Subject))
        {
            this.logger.LogWarning("Test email subject was empty");
            return this.BadRequest("Subject cannot be empty");
        }

        if (string.IsNullOrWhiteSpace(request.Body))
        {
            this.logger.LogWarning("Test email body was empty");
            return this.BadRequest("Body cannot be empty");
        }

        // Generate unique message ID
        var messageId = Guid.NewGuid().ToString("N")[..12];
        var sentAt = DateTime.UtcNow;

        // Log email details (stub implementation - no actual sending)
        this.logger.LogInformation(
            "Test email prepared - MessageId: {MessageId}, To: {To}, Subject: {Subject}, BodyLength: {BodyLength}",
            messageId,
            request.To,
            request.Subject,
            request.Body.Length);

        // Simulate async operation
        await Task.Delay(50, cancellationToken);

        var response = new TestEmailResponse
        {
            MessageId = messageId,
            SentAt = sentAt,
        };

        this.logger.LogInformation("Test email completed successfully - MessageId: {MessageId}", messageId);

        return this.Accepted(response);
    }

    private bool IsValidEmailFormat(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
