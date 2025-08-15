// <copyright file="SampleTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.UnitTests;

using System.Net;
using ApiGateway.Controllers;
using Lynx.Abstractions.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;

public class HealthControllerTests
{
    private readonly IFixture fixture;
    private readonly MockHttpMessageHandler mockHttpHandler;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;
    private readonly ILogger<HealthController> logger;

    public HealthControllerTests()
    {
        this.fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        this.mockHttpHandler = new MockHttpMessageHandler();

        var httpClient = this.mockHttpHandler.ToHttpClient();
        this.httpClientFactory = Substitute.For<IHttpClientFactory>();
        this.httpClientFactory.CreateClient().Returns(httpClient);

        this.configuration = Substitute.For<IConfiguration>();
        this.configuration.GetConnectionString("DefaultConnection")
            .Returns("Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example");

        this.logger = Substitute.For<ILogger<HealthController>>();
    }

    [Fact]
    public void ApiGateway_Health_Returns200_WhenServiceIsRunning()
    {
        // Arrange
        var sut = new HealthController(this.httpClientFactory, this.configuration, this.logger);

        // Act
        var result = sut.Health();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task ApiGateway_Ready_Returns200_WhenAllDependenciesHealthy()
    {
        // Arrange
        var sut = new HealthController(this.httpClientFactory, this.configuration, this.logger);

        this.mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.OK);
        this.mockHttpHandler.When("http://localhost:8082/health")
                       .Respond(HttpStatusCode.OK);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var response = okResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Healthy);
        response.Dependencies.Should().ContainKey("identityService");
        response.Dependencies.Should().ContainKey("notificationService");
    }

    [Fact]
    public async Task ApiGateway_Ready_Returns503_WhenIdentityServiceDown()
    {
        // Arrange
        var sut = new HealthController(this.httpClientFactory, this.configuration, this.logger);

        this.mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);
        this.mockHttpHandler.When("http://localhost:8082/health")
                       .Respond(HttpStatusCode.OK);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
        var response = statusResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["identityService"].Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task ApiGateway_Ready_Returns503_WhenNotificationServiceDown()
    {
        // Arrange
        var sut = new HealthController(this.httpClientFactory, this.configuration, this.logger);

        this.mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.OK);
        this.mockHttpHandler.When("http://localhost:8082/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
        var response = statusResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["notificationService"].Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task ApiGateway_Ready_Returns503_WhenMultipleDependenciesDown()
    {
        // Arrange
        var sut = new HealthController(this.httpClientFactory, this.configuration, this.logger);

        this.mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);
        this.mockHttpHandler.When("http://localhost:8082/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
        var response = statusResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["identityService"].Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["notificationService"].Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task ApiGateway_Ready_LogsStructuredData_OnDependencyFailure()
    {
        // Arrange
        var sut = new HealthController(this.httpClientFactory, this.configuration, this.logger);

        this.mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);
        this.mockHttpHandler.When("http://localhost:8082/health")
                       .Respond(HttpStatusCode.OK);

        // Act
        await sut.ReadyAsync(CancellationToken.None);

        // Assert
        this.logger.Received().BeginScope("DependencyCheck:{Dependency}", "IdentityService");
        this.logger.Received().LogWarning(
            "Dependency check completed: {Service} - {Status} - {ResponseTime}ms",
            "IdentityService", HealthStatus.Unhealthy, Arg.Any<long>());
    }
}
