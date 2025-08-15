// <copyright file="TestEmailResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lynx.Abstractions.Notifications;

using System;

/// <summary>
/// Response from sending a test email.
/// </summary>
public sealed record TestEmailResponse
{
    /// <summary>
    /// Gets the unique message identifier.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// Gets the timestamp when the email was sent.
    /// </summary>
    public DateTime SentAt { get; init; } = DateTime.UtcNow;
}
