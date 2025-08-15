// <copyright file="ApiGatewayHealthTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ApiGateway.UnitTests.Health;

using System.Net;
using System.Threading.Tasks;
using ApiGateway.Controllers;
using Lynx.Abstractions.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RichardSzalay.MockHttp;

public sealed class ApiGatewayHealthTests
{
    [Theory]
    [AutoData]
    public void Health_Returns200([Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        var mockConfiguration = Substitute.For<IConfiguration>();
        var sut = new HealthController(mockHttpClientFactory, mockConfiguration, logger);

        // Act
        var result = sut.Health();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Healthy);
    }

    [Theory]
    [AutoData]
    public async Task Ready_Returns200_WhenAllDepsOk(
        [Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When(HttpMethod.Head, "http://localhost:8081/health")
                   .Respond(HttpStatusCode.OK);
        mockHandler.When(HttpMethod.Head, "http://localhost:8082/health")
                   .Respond(HttpStatusCode.OK);

        var httpClient = mockHandler.ToHttpClient();
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        mockHttpClientFactory.CreateClient().Returns(httpClient);

        var mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration.GetConnectionString("DefaultConnection")
                         .Returns("Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example");

        var sut = new HealthController(mockHttpClientFactory, mockConfiguration, logger);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Healthy);
        response.Dependencies.Should().ContainKey("identityService");
        response.Dependencies.Should().ContainKey("notificationService");
        response.Dependencies.Should().ContainKey("postgresql");
    }

    [Theory]
    [AutoData]
    public async Task Ready_Returns503_WhenIdentityDown(
        [Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When(HttpMethod.Head, "http://localhost:8081/health")
                   .Respond(HttpStatusCode.ServiceUnavailable);
        mockHandler.When(HttpMethod.Head, "http://localhost:8082/health")
                   .Respond(HttpStatusCode.OK);

        var httpClient = mockHandler.ToHttpClient();
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        mockHttpClientFactory.CreateClient().Returns(httpClient);

        var mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration.GetConnectionString("DefaultConnection")
                         .Returns("Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example");

        var sut = new HealthController(mockHttpClientFactory, mockConfiguration, logger);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
        var response = statusResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["identityService"].Should().Be(HealthStatus.Unhealthy);
    }

    [Theory]
    [AutoData]
    public async Task Ready_Returns503_WhenNotificationDown(
        [Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When(HttpMethod.Head, "http://localhost:8081/health")
                   .Respond(HttpStatusCode.OK);
        mockHandler.When(HttpMethod.Head, "http://localhost:8082/health")
                   .Respond(HttpStatusCode.ServiceUnavailable);

        var httpClient = mockHandler.ToHttpClient();
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        mockHttpClientFactory.CreateClient().Returns(httpClient);

        var mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration.GetConnectionString("DefaultConnection")
                         .Returns("Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example");

        var sut = new HealthController(mockHttpClientFactory, mockConfiguration, logger);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
        var response = statusResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["notificationService"].Should().Be(HealthStatus.Unhealthy);
    }

    [Theory]
    [AutoData]
    public async Task Ready_Returns503_WhenPostgresDown(
        [Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When(HttpMethod.Head, "http://localhost:8081/health")
                   .Respond(HttpStatusCode.OK);
        mockHandler.When(HttpMethod.Head, "http://localhost:8082/health")
                   .Respond(HttpStatusCode.OK);

        var httpClient = mockHandler.ToHttpClient();
        var mockHttpClientFactory = Substitute.For<IHttpClientFactory>();
        mockHttpClientFactory.CreateClient().Returns(httpClient);

        var mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration.GetConnectionString("DefaultConnection")
                         .Returns("Host=invalid-host;Port=9999;Database=nonexistent;Username=invalid;Password=invalid");

        var sut = new HealthController(mockHttpClientFactory, mockConfiguration, logger);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        var statusResult = result.Should().BeOfType<ObjectResult>().Subject;
        statusResult.StatusCode.Should().Be(503);
        var response = statusResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Unhealthy);
        response.Dependencies["postgresql"].Should().Be(HealthStatus.Unhealthy);
    }
}
