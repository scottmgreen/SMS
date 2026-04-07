//-----------------------------------------------------------------------
// <copyright file="BasicServiceTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Basic unit tests for core Application services.
//                  Tests basic service functionality and integration.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PDXSMS_UnitTests.Application.Common;
using SMS_Application.Interfaces;
using System.Diagnostics;

namespace PDXSMS_UnitTests.Application.Services;

/// <summary>
/// Basic unit tests for core Application services
/// Tests basic service functionality and integration
/// </summary>
public class BasicServiceTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Register basic services that exist in the Application project
        // Focus on services we know exist based on the project structure
    }

    protected override void RegisterMockedServices(IServiceCollection services)
    {
        // Mock external dependencies as needed
    }

    #region Service Availability Tests

    [Fact]
    public void ApplicationServices_ShouldBeAvailableInTestBase()
    {
        // Act & Assert
        ServiceProvider.Should().NotBeNull();
        Mediator.Should().NotBeNull();
        Logger.Should().NotBeNull();
    }

    [Fact]
    public void BasicServices_ShouldResolveFromServiceProvider()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
        var logger = ServiceProvider.GetService<ILogger<BasicServiceTests>>();

        // Assert
        configuration.Should().NotBeNull();
        loggerFactory.Should().NotBeNull();
        logger.Should().NotBeNull();
    }

    #endregion

    #region Mediator Service Tests

    [Fact]
    public void MediatorService_ShouldBeCorrectType()
    {
        // Act & Assert
        Mediator.Should().BeOfType<SMS_Application.Services.Mediator>();
    }

    [Fact]
    public void MediatorService_ShouldImplementInterface()
    {
        // Act & Assert
        Mediator.Should().BeAssignableTo<IMediator>();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void Logger_ShouldBeConfiguredCorrectly()
    {
        // Act
        Logger.LogInformation("Test log message from BasicServiceTests");

        // Assert
        Logger.Should().NotBeNull();
        Logger.Should().BeAssignableTo<ILogger<ApplicationTestBase>>();
    }

    [Fact]
    public void LoggerFactory_ShouldCreateLoggers()
    {
        // Arrange
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

        // Act
        var testLogger = loggerFactory.CreateLogger("TestLogger");

        // Assert
        testLogger.Should().NotBeNull();
        testLogger.Should().BeAssignableTo<ILogger>();
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void Configuration_ShouldHaveTestValues()
    {
        // Arrange
        var configuration = ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

        // Act & Assert
        configuration["Testing:Environment"].Should().Be("UnitTest");
        configuration.GetConnectionString("DefaultConnection").Should().NotBeEmpty();
    }

    [Fact]
    public void Configuration_ShouldSupportSections()
    {
        // Arrange
        var configuration = ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

        // Act
        var loggingSection = configuration.GetSection("Logging");

        // Assert
        loggingSection.Should().NotBeNull();
        loggingSection.Exists().Should().BeTrue();
    }

    #endregion

    #region Service Integration Tests

    [Fact]
    public void Services_ShouldWorkTogetherInTestEnvironment()
    {
        // Arrange
        var configuration = ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = ServiceProvider.GetRequiredService<ILogger<BasicServiceTests>>();

        // Act
        var testEnvironment = configuration["Testing:Environment"];
        logger.LogInformation("Running in environment: {Environment}", testEnvironment);

        // Assert
        testEnvironment.Should().Be("UnitTest");
        // The fact that logging works without exceptions indicates proper integration
    }

    [Fact]
    public void ServiceProvider_ShouldCreateScopes()
    {
        // Act
        using var scope = ServiceProvider.CreateScope();
        var scopedConfiguration = scope.ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();

        // Assert
        scope.Should().NotBeNull();
        scopedConfiguration.Should().NotBeNull();
    }

    #endregion

    #region Test Infrastructure Validation

    [Fact]
    public void TestBase_ShouldProvideWorkingInfrastructure()
    {
        // Act
        var hazard = CreateTestHazard();
        var report = CreateTestReport();
        var uniqueCode = GenerateUniqueCode("TEST");

        // Assert
        hazard.Should().NotBeNull();
        report.Should().NotBeNull();
        uniqueCode.Should().NotBeEmpty();
        
        Logger.LogInformation("Test infrastructure validation completed successfully");
    }

    [Fact]
    public async Task TestBase_ShouldSupportAsyncOperations()
    {
        // Act
        var (result, duration) = await MeasureAsync(async () =>
        {
            await Task.Delay(1);
            return "async test completed";
        });

        // Assert
        result.Should().Be("async test completed");
        duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void ServiceProvider_ShouldHandleMissingServices()
    {
        // Act
        var missingService = ServiceProvider.GetService<BasicServiceTests>();

        // Assert
        missingService.Should().BeNull();
    }

    [Fact]
    public void Services_ShouldNotThrowUnexpectedExceptions()
    {
        // Act & Assert - These operations should not throw
        var config = ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        var logger = ServiceProvider.GetService<ILogger<BasicServiceTests>>();
        var mediator = ServiceProvider.GetService<IMediator>();

        config.Should().NotBeNull();
        logger.Should().NotBeNull();
        mediator.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void ServiceResolution_ShouldBePerformant()
    {
        // Arrange & Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 50; i++)
        {
            var config = ServiceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
            var logger = ServiceProvider.GetService<ILogger<BasicServiceTests>>();
            var mediator = ServiceProvider.GetService<IMediator>();
            
            config.Should().NotBeNull();
            logger.Should().NotBeNull();
            mediator.Should().NotBeNull();
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200); // Should be very fast
        
        Logger.LogInformation("50 service resolutions completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
    }

    #endregion
}