// <copyright file="AuthorizationPolicyTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.UnitTests.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tests for authorization policies in ApiGateway.
/// </summary>
public sealed class AuthorizationPolicyTests
{
    private readonly IFixture fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationPolicyTests"/> class.
    /// </summary>
    public AuthorizationPolicyTests()
    {
        this.fixture = new Fixture();
        this.fixture.Customize(new AutoNSubstituteCustomization());
    }

    /// <summary>
    /// Tests that admin policy allows access when user has admin role.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="username">The username.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [AutoData]
    public async Task AdminPolicy_WithAdminRole_ShouldAllowAccess(string userId, string username)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("admin"));
        });

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "admin"),
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await authorizationService.AuthorizeAsync(principal, "AdminPolicy");

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    /// <summary>
    /// Tests that admin policy denies access when user lacks admin role.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="username">The username.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Theory]
    [AutoData]
    public async Task AdminPolicy_WithoutAdminRole_ShouldDenyAccess(string userId, string username)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("admin"));
        });

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, "user"),
        };
        var identity = new ClaimsIdentity(claims, "test");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await authorizationService.AuthorizeAsync(principal, "AdminPolicy");

        // Assert
        result.Succeeded.Should().BeFalse();
    }

    /// <summary>
    /// Tests that admin policy denies access for unauthenticated users.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task AdminPolicy_WithUnauthenticatedUser_ShouldDenyAccess()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminPolicy", policy =>
                policy.RequireRole("admin"));
        });

        var serviceProvider = services.BuildServiceProvider();
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();

        var principal = new ClaimsPrincipal(); // Unauthenticated

        // Act
        var result = await authorizationService.AuthorizeAsync(principal, "AdminPolicy");

        // Assert
        result.Succeeded.Should().BeFalse();
    }
}
