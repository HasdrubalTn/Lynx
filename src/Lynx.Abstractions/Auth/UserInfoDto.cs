// <copyright file="UserInfoDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Lynx.Abstractions.Auth;

/// <summary>
/// Represents user information returned by the /me endpoint.
/// </summary>
public sealed record UserInfoDto
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the roles assigned to the user.
    /// </summary>
    public required string[] Roles { get; init; }
}
