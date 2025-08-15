// <copyright file="TestEmailTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.UnitTests.Notifications;

using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using ApiGateway.Controllers;
using Lynx.Abstractions.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tests for test email functionality in ApiGateway.
/// </summary>
public class TestEmailTests
{
    private readonly Fixture fixture;
    private readonly TestEmailController controller;
    private readonly ILogger<TestEmailController> mockLogger;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEmailTests"/> class.
    /// </summary>
    public TestEmailTests()
    {
        this.fixture = new Fixture();
        this.fixture.Customize(new AutoNSubstituteCustomization());

        this.mockLogger = this.fixture.Create<ILogger<TestEmailController>>();
        this.httpClient = new HttpClient();
        this.controller = new TestEmailController(this.httpClient, this.mockLogger);
    }

    /// <summary>
    /// Test that test email endpoint returns server error when service is unavailable.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestEmail_WhenServiceUnavailable_ReturnsServerError()
    {
        // Arrange
        var request = new TestEmailRequest
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
        };
        this.SetupAuthenticatedUser("admin");

        // Act
        var result = await this.controller.SendTestEmail(request, CancellationToken.None);

        // Assert - When the NotificationService is not running, we expect a server error
        var actionResult = result.Result as ObjectResult;
        actionResult.Should().NotBeNull();
        actionResult!.StatusCode.Should().BeOneOf(500, 502); // Server error or Bad Gateway
    }

    /// <summary>
    /// Test that controller exists and can be instantiated.
    /// </summary>
    [Fact]
    public void TestEmailController_CanBeInstantiated()
    {
        // Assert
        this.controller.Should().NotBeNull();
        this.controller.Should().BeOfType<TestEmailController>();
    }

    /// <summary>
    /// Test that SendTestEmail method exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SendTestEmail_MethodExists()
    {
        // Arrange
        var request = new TestEmailRequest
        {
            To = "test@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
        };
        this.SetupAuthenticatedUser("admin");

        // Act & Assert - Should not throw
        var result = await this.controller.SendTestEmail(request, CancellationToken.None);
        result.Should().NotBeNull();
    }

    private void SetupAuthenticatedUser(string role)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, role),
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal,
            },
        };
    }
}
