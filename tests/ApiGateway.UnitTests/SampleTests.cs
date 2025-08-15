using ApiGateway.Controllers;
using Lynx.Abstractions.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RichardSzalay.MockHttp;
using System.Net;

namespace ApiGateway.UnitTests;

public class HealthControllerTests
{
    private readonly IFixture _fixture;
    private readonly MockHttpMessageHandler _mockHttpHandler;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HealthController> _logger;

    public HealthControllerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        _mockHttpHandler = new MockHttpMessageHandler();
        
        var httpClient = _mockHttpHandler.ToHttpClient();
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClientFactory.CreateClient().Returns(httpClient);
        
        _configuration = Substitute.For<IConfiguration>();
        _configuration.GetConnectionString("DefaultConnection")
            .Returns("Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example");
        
        _logger = Substitute.For<ILogger<HealthController>>();
    }

    [Fact]
    public void ApiGateway_Health_Returns200_WhenServiceIsRunning()
    {
        // Arrange
        var sut = new HealthController(_httpClientFactory, _configuration, _logger);

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
        var sut = new HealthController(_httpClientFactory, _configuration, _logger);
        
        _mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.OK);
        _mockHttpHandler.When("http://localhost:8082/health")
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
        var sut = new HealthController(_httpClientFactory, _configuration, _logger);
        
        _mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);
        _mockHttpHandler.When("http://localhost:8082/health")
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
        var sut = new HealthController(_httpClientFactory, _configuration, _logger);
        
        _mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.OK);
        _mockHttpHandler.When("http://localhost:8082/health")
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
        var sut = new HealthController(_httpClientFactory, _configuration, _logger);
        
        _mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);
        _mockHttpHandler.When("http://localhost:8082/health")
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
        var sut = new HealthController(_httpClientFactory, _configuration, _logger);
        
        _mockHttpHandler.When("http://localhost:8081/health")
                       .Respond(HttpStatusCode.ServiceUnavailable);
        _mockHttpHandler.When("http://localhost:8082/health")
                       .Respond(HttpStatusCode.OK);

        // Act
        await sut.ReadyAsync(CancellationToken.None);

        // Assert
        _logger.Received().BeginScope("DependencyCheck:{Dependency}", "IdentityService");
        _logger.Received().LogWarning("Dependency check completed: {Service} - {Status} - {ResponseTime}ms", 
            "IdentityService", HealthStatus.Unhealthy, Arg.Any<long>());
    }
}
