// <copyright file="HealthCheckResponse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lynx.Abstractions.Health;

using System;
using System.Collections.Generic;

public sealed class HealthCheckResponse
{
    public HealthStatus Status { get; init; } = HealthStatus.Healthy;

    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public Dictionary<string, HealthStatus> Dependencies { get; init; } = new();

    public string? Error { get; init; }
}
