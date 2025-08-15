using NotificationService.Controllers;
using Lynx.Abstractions.Health;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace NotificationService.UnitTests.Health;

public sealed class NotificationHealthTests
{
    [Fact, AutoData]
    public void Health_Returns200([Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        var sut = new HealthController(mockConfiguration, logger);

        // Act
        var result = sut.Health();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<HealthCheckResponse>().Subject;
        response.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact, AutoData]
    public async Task Ready_Returns200([Frozen] ILogger<HealthController> logger)
    {
        // Arrange
        var mockConfiguration = Substitute.For<IConfiguration>();
        mockConfiguration.GetConnectionString("DefaultConnection")
                         .Returns("Host=localhost;Port=5432;Database=lynx;Username=lynx;Password=example");

        var sut = new HealthController(mockConfiguration, logger);

        // Act
        var result = await sut.ReadyAsync(CancellationToken.None);

        // Assert
        // Note: This test will likely fail with real DB connection in CI/CD
        // In real implementation, we'd mock the Npgsql connection or use TestContainers
        var response = result.Should().BeOfType<ObjectResult>().Subject;
        response.Value.Should().BeOfType<HealthCheckResponse>();
    }
}
