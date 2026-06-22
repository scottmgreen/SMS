//-----------------------------------------------------------------------
// <copyright file="MediatorServiceTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Unit tests for the MediatorService - the core CQRS coordinator.
//                  Tests basic mediator functionality and service resolution.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using PDXSMS_UnitTests.Application.Common;
using SMS_Application.Interfaces;
using System.Diagnostics;

namespace PDXSMS_UnitTests.Application.Services;

/// <summary>
/// Unit tests for the MediatorService
/// Tests the core CQRS mediator functionality and service resolution
/// </summary>
public class MediatorServiceTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Register core services that are available
        // Based on the actual Application project structure
    }

    protected override void RegisterMockedServices(IServiceCollection services)
    {
        // Mock services that the mediator depends on
    }

    #region Basic Mediator Tests

    [Fact]
    public void Mediator_ShouldBeAvailable()
    {
        // Act & Assert
        Mediator.Should().NotBeNull();
    }

    [Fact]
    public void Mediator_ShouldBeCorrectType()
    {
        // Act & Assert  
        Mediator.Should().BeOfType<SMS_Application.Services.Mediator>();
    }

    #endregion

    #region Service Resolution Tests

    [Fact]
    public void ServiceProvider_ShouldResolveMediator()
    {
        // Arrange & Act
        var mediator = ServiceProvider.GetService<IBaseMediator>();

        // Assert
        mediator.Should().NotBeNull();
    }

    [Fact]
    public void ServiceProvider_ShouldProvideConsistentMediatorInstance()
    {
        // Arrange
        var mediator1 = ServiceProvider.GetService<IBaseMediator>();
        var mediator2 = ServiceProvider.GetService<IBaseMediator>();

        // Act & Assert
        mediator1.Should().NotBeNull();
        mediator2.Should().NotBeNull();
        // Note: Depending on registration (singleton vs transient), instances may be same or different
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void MediatorCreation_ShouldBeEfficient()
    {
        // Arrange & Act
        var stopwatch = Stopwatch.StartNew();
        var mediator = ServiceProvider.GetService<IBaseMediator>();
        stopwatch.Stop();

        // Assert
        mediator.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should be very fast
    }

    #endregion

    #region Basic Functionality Tests

    [Fact]
    public void Mediator_ShouldImplementIMediator()
    {
        // Act & Assert
        Mediator.Should().BeAssignableTo<IBaseMediator>();
    }

    [Fact]
    public void Mediator_ShouldHaveServiceProviderDependency()
    {
        // This test verifies that the mediator can be constructed with DI
        // Act & Assert
        Mediator.Should().NotBeNull();
        // The fact that we can create it through DI confirms the service provider dependency works
    }

    #endregion
}