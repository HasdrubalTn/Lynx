// <copyright file="TestEmailEndpointTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace NotificationService.UnitTests.Notifications;

using System;
using System.Threading;
using System.Threading.Tasks;
using Lynx.Abstractions.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Tests for test email endpoint in NotificationService.
/// </summary>
public class TestEmailEndpointTests
{
    private readonly Fixture fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEmailEndpointTests"/> class.
    /// </summary>
    public TestEmailEndpointTests()
    {
        this.fixture = new Fixture();
        this.fixture.Customize(new AutoNSubstituteCustomization());
    }

    /// <summary>
    /// Test that test email endpoint returns 202 with message ID for valid payload.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestEmail_WithValidPayload_Returns202WithMessageId()
    {
        // Arrange
        var logger = Substitute.For<ILogger<NotificationService.Controllers.TestEmailController>>();
        var controller = new NotificationService.Controllers.TestEmailController(logger);
        var request = new TestEmailRequest
        {
            To = "test@example.com",
            Subject = "Test Email",
            Body = "This is a test email body.",
        };

        // Act
        var actionResult = await controller.SendTestEmail(request, CancellationToken.None);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().NotBeNull();

        // Check that we got an ObjectResult (Accepted is a subclass)
        var objectResult = actionResult.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(202);

        var response = objectResult.Value as TestEmailResponse;
        response.Should().NotBeNull();
        response!.MessageId.Should().NotBeNullOrEmpty();
        response.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Test that test email endpoint returns 400 for invalid payload.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestEmail_WithInvalidPayload_Returns400()
    {
        // Arrange
        var logger = Substitute.For<ILogger<NotificationService.Controllers.TestEmailController>>();
        var controller = new NotificationService.Controllers.TestEmailController(logger);
        var request = new TestEmailRequest
        {
            To = "invalid-email",
            Subject = "Test",
            Body = "Test body",
        };

        // Act
        var actionResult = await controller.SendTestEmail(request, CancellationToken.None);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    /// <summary>
    /// Test that test email endpoint generates message ID with stub sender.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestEmail_WithStubSender_GeneratesMessageId()
    {
        // Arrange
        var logger = Substitute.For<ILogger<NotificationService.Controllers.TestEmailController>>();
        var controller = new NotificationService.Controllers.TestEmailController(logger);
        var request = new TestEmailRequest
        {
            To = "test@example.com",
            Subject = "Test Email",
            Body = "Test body for stub sender.",
        };

        // Act
        var actionResult = await controller.SendTestEmail(request, CancellationToken.None);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().NotBeNull();

        var objectResult = actionResult.Result as ObjectResult;
        objectResult.Should().NotBeNull();

        var response = objectResult!.Value as TestEmailResponse;
        response.Should().NotBeNull();
        response!.MessageId.Should().NotBeNullOrEmpty();
        response.MessageId.Length.Should().Be(12); // Our implementation generates 12-char IDs
    }

    /// <summary>
    /// Test that test email endpoint logs email details.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestEmail_LogsEmailDetails()
    {
        // Arrange
        var logger = Substitute.For<ILogger<NotificationService.Controllers.TestEmailController>>();
        var controller = new NotificationService.Controllers.TestEmailController(logger);
        var request = new TestEmailRequest
        {
            To = "test@example.com",
            Subject = "Test Email",
            Body = "Test body for logging.",
        };

        // Act
        var actionResult = await controller.SendTestEmail(request, CancellationToken.None);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().NotBeNull();

        // Verify that logging occurred (check that logger was called)
        // Use Received to check for any LogInformation call
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    /// <summary>
    /// Test that test email endpoint validates email format.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task TestEmail_ValidatesEmailFormat()
    {
        // Arrange
        var logger = Substitute.For<ILogger<NotificationService.Controllers.TestEmailController>>();
        var controller = new NotificationService.Controllers.TestEmailController(logger);
        var request = new TestEmailRequest
        {
            To = "not-an-email",
            Subject = "Test Email",
            Body = "Test body.",
        };

        // Act
        var actionResult = await controller.SendTestEmail(request, CancellationToken.None);

        // Assert
        actionResult.Should().NotBeNull();
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
    }
}
