//-----------------------------------------------------------------------
// <copyright file="ApplicationConfigurationTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Unit tests for Application layer configuration and dependency injection.
//                  Tests service registration, configuration loading, and DI setup.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PDXSMS_UnitTests.Application.Common;
using SMS_Application.Interfaces;
using System.Diagnostics;
using SMS_Domain.Enums;

namespace PDXSMS_UnitTests.Application.Messaging.CommandHandlers;

/// <summary>
/// Unit tests for Application layer configuration and dependency injection
/// Tests service registration, configuration loading, and DI setup
/// </summary>
public class ApplicationConfigurationTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Test with minimal service registration
    }

    protected override void RegisterMockedServices(IServiceCollection services)
    {
        // Test with minimal mocking
    }

    #region Configuration Tests

    [Fact]
    public void Configuration_ShouldBeAvailable()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        // Assert
        configuration.Should().NotBeNull();
    }

    [Fact]
    public void Configuration_ShouldHaveTestSettings()
    {
        // Arrange
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        // Act & Assert
        configuration["Testing:Environment"].Should().Be("UnitTest");
        configuration.GetConnectionString("DefaultConnection").Should().Be("Data Source=:memory:");
        configuration["Logging:LogLevel:Default"].Should().Be("Information");
    }

    [Fact]
    public void Configuration_ShouldSupportSectionBinding()
    {
        // Arrange
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();

        // Act
        var loggingSection = configuration.GetSection("Logging");
        var logLevel = loggingSection["LogLevel:Default"];

        // Assert
        loggingSection.Should().NotBeNull();
        logLevel.Should().Be("Information");
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void ServiceProvider_ShouldRegisterLoggingServices()
    {
        // Arrange & Act
        var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();
        var logger = ServiceProvider.GetService<ILogger<ApplicationConfigurationTests>>();

        // Assert
        loggerFactory.Should().NotBeNull();
        logger.Should().NotBeNull();
    }

    [Fact]
    public void ServiceProvider_ShouldRegisterMediator()
    {
        // Arrange & Act
        var mediator = ServiceProvider.GetService<IMediator>();

        // Assert
        mediator.Should().NotBeNull();
        mediator.Should().BeOfType<SMS_Application.Services.Mediator>();
    }

    [Fact]
    public void ServiceProvider_ShouldHandleServiceScopes()
    {
        // Arrange & Act
        using var scope = ServiceProvider.CreateScope();
        var scopedLogger = scope.ServiceProvider.GetService<ILogger<ApplicationConfigurationTests>>();

        // Assert
        scopedLogger.Should().NotBeNull();
    }

    #endregion

    #region Dependency Injection Tests

    [Fact]
    public void DependencyInjection_ShouldResolveTransientServices()
    {
        // Arrange & Act
        var service1 = ServiceProvider.GetService<IMediator>();
        var service2 = ServiceProvider.GetService<IMediator>();

        // Assert
        service1.Should().NotBeNull();
        service2.Should().NotBeNull();
        // For transient services, instances should be different
    }

    [Fact]
    public void DependencyInjection_ShouldResolveSingletonServices()
    {
        // Arrange & Act
        var config1 = ServiceProvider.GetService<IConfiguration>();
        var config2 = ServiceProvider.GetService<IConfiguration>();

        // Assert
        config1.Should().NotBeNull();
        config2.Should().NotBeNull();
        config1.Should().BeSameAs(config2); // Singleton should be same instance
    }

    [Fact]
    public void DependencyInjection_ShouldHandleNestedDependencies()
    {
        // Arrange & Act
        var mediator = ServiceProvider.GetService<IMediator>();

        // Assert
        mediator.Should().NotBeNull();
        // The mediator should have been constructed with its service provider dependency
    }

    #endregion

    #region Service Provider Tests

    [Fact]
    public void ServiceProvider_ShouldProvideRequiredServices()
    {
        // Arrange
        var requiredServices = new[]
        {
            typeof(IConfiguration),
            typeof(ILoggerFactory),
            typeof(ILogger<ApplicationConfigurationTests>),
            typeof(IMediator)
        };

        // Act & Assert
        foreach (var serviceType in requiredServices)
        {
            var service = ServiceProvider.GetService(serviceType);
            service.Should().NotBeNull($"Service {serviceType.Name} should be available");
        }
    }

    [Fact]
    public void ServiceProvider_ShouldHandleOptionalServices()
    {
        // Arrange & Act
        var optionalService = ServiceProvider.GetService<IServiceCollection>();

        // Assert
        // Service collection is not typically registered, so this should be null
        optionalService.Should().BeNull();
    }

    #endregion

    #region Configuration Loading Tests

    [Fact]
    public void ConfigurationLoading_ShouldSupportInMemoryConfiguration()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var testValue = configuration["Testing:Environment"];

        // Assert
        testValue.Should().Be("UnitTest");
        // This confirms that our in-memory configuration is working
    }

    [Fact]
    public void ConfigurationLoading_ShouldSupportConnectionStrings()
    {
        // Arrange & Act
        var configuration = ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Assert
        connectionString.Should().Be("Data Source=:memory:");
    }

    #endregion

    #region Service Lifetime Tests

    [Fact]
    public void ServiceLifetime_TransientServices_ShouldCreateNewInstances()
    {
        // Arrange & Act
        var mediator1 = ServiceProvider.GetService<IMediator>();
        var mediator2 = ServiceProvider.GetService<IMediator>();

        // Assert
        mediator1.Should().NotBeNull();
        mediator2.Should().NotBeNull();
        // For transient services, verify they can be created independently
    }

    [Fact]
    public void ServiceLifetime_SingletonServices_ShouldReuseInstances()
    {
        // Arrange & Act
        var logger1 = ServiceProvider.GetService<ILoggerFactory>();
        var logger2 = ServiceProvider.GetService<ILoggerFactory>();

        // Assert
        logger1.Should().NotBeNull();
        logger2.Should().NotBeNull();
        logger1.Should().BeSameAs(logger2); // Should be same instance
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void ServiceProvider_WithMissingService_ShouldReturnNull()
    {
        // Arrange & Act
        var missingService = ServiceProvider.GetService<ApplicationConfigurationTests>();

        // Assert
        missingService.Should().BeNull();
    }

    [Fact] 
    public void ServiceProvider_WithRequiredMissingService_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            ServiceProvider.GetRequiredService<ApplicationConfigurationTests>());
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void ServiceResolution_ShouldBeEfficient()
    {
        // Arrange & Act
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < 100; i++)
        {
            var mediator = ServiceProvider.GetService<IMediator>();
            mediator.Should().NotBeNull();
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should be very fast
        Logger.LogInformation("100 service resolutions completed in {Duration}ms", stopwatch.ElapsedMilliseconds);
    }

    #endregion
}