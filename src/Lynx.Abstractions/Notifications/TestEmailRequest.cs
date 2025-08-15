// <copyright file="TestEmailRequest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lynx.Abstractions.Notifications;

/// <summary>
/// Request to send a test email for admin verification.
/// </summary>
public sealed record TestEmailRequest
{
    /// <summary>
    /// Gets the recipient email address.
    /// </summary>
    public required string To { get; init; }

    /// <summary>
    /// Gets the email subject line.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Gets the email body content.
    /// </summary>
    public required string Body { get; init; }
}
