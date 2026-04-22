//-----------------------------------------------------------------------
// <copyright file="EventBusIntegrationTests.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Integration tests for EventBus Phase 1 implementation.
//                  Tests event publishing, handler execution, and integration with
//                  existing SMS infrastructure patterns.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Application.EventHandlers;
using SMS_Domain.Events;
using SMS_Domain.Common;
using Xunit;
using Moq;

namespace SMS_Tests.Application.EventBus;

/// <summary>
/// Integration tests for EventBus Phase 1 implementation
/// Tests core functionality with mocked dependencies to ensure EventBus works correctly
/// </summary>
public class EventBusIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventBus _eventBus;
    private readonly Mock<ILogger<EventBusService>> _mockEventBusLogger;
    private readonly Mock<ILogger<SPIThresholdEventHandler>> _mockHandlerLogger;
    private readonly Mock<SMSStakeholderGroupService> _mockStakeholderService;

    public EventBusIntegrationTests()
    {
        // Setup service collection with mocked dependencies
        var services = new ServiceCollection();

        // Mock loggers
        _mockEventBusLogger = new Mock<ILogger<EventBusService>>();
        _mockHandlerLogger = new Mock<ILogger<SPIThresholdEventHandler>>();

        // Mock stakeholder service
        _mockStakeholderService = new Mock<SMSStakeholderGroupService>();

        // Register services
        services.AddSingleton(_mockEventBusLogger.Object);
        services.AddSingleton(_mockHandlerLogger.Object);
        services.AddSingleton(_mockStakeholderService.Object);

        // Register EventBus services
        services.AddSingleton<IEventBus, EventBusService>();
        services.AddSingleton<SPIThresholdEventHandler>();

        _serviceProvider = services.BuildServiceProvider();
        _eventBus = _serviceProvider.GetRequiredService<IEventBus>();
    }

    [Fact]
    public async Task PublishAsync_WithValidSPIEvent_ShouldReturnSuccess()
    {
        // Arrange
        var testEvent = new SPIThresholdExceededEvent(
            spiCode: "TEST-SPI-001",
            spiName: "Test SPI",
            currentValue: 15.0m,
            thresholdValue: 10.0m,
            severity: SPISeverityLevel.High,
            stakeholderGroups: new List<string> { "SAFETY", "MGMT" },
            reportingPeriod: "2024-01",
            aggregateId: Guid.NewGuid().ToString()
        );

        // Act
        var result = await _eventBus.PublishAsync(testEvent);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public async Task PublishAsync_WithNullEvent_ShouldReturnFailure()
    {
        // Act
        var result = await _eventBus.PublishAsync<SPIThresholdExceededEvent>(null!);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal("EVENTBUS_NULL_EVENT", result.Error.Code);
    }

    [Fact]
    public async Task PublishAsync_WithDifferentExecutionModes_ShouldSucceed()
    {
        // Arrange
        var testEvent = new SPIThresholdExceededEvent(
            spiCode: "TEST-SPI-002",
            spiName: "Test SPI for Modes",
            currentValue: 25.0m,
            thresholdValue: 20.0m,
            severity: SPISeverityLevel.Critical,
            stakeholderGroups: new List<string> { "EXEC" },
            reportingPeriod: "2024-01",
            aggregateId: Guid.NewGuid().ToString()
        );

        // Act & Assert - Immediate mode
        var immediateResult = await _eventBus.PublishAsync(testEvent, EventExecutionMode.Immediate);
        Assert.True(immediateResult.IsSuccess);

        // Act & Assert - Queued mode (falls back to immediate in Phase 1)
        var queuedResult = await _eventBus.PublishAsync(testEvent, EventExecutionMode.Queued);
        Assert.True(queuedResult.IsSuccess);

        // Act & Assert - Manual mode (falls back to immediate in Phase 1)  
        var manualResult = await _eventBus.PublishAsync(testEvent, EventExecutionMode.Manual);
        Assert.True(manualResult.IsSuccess);
    }

    [Fact]
    public void Subscribe_WithValidEventAndHandler_ShouldRegisterSuccessfully()
    {
        // Act - Subscribe to event
        _eventBus.Subscribe<SPIThresholdExceededEvent, SPIThresholdEventHandler>();

        // Assert - Check subscriptions
        var subscriptions = _eventBus.GetActiveSubscriptionsAsync().Result;
        Assert.True(subscriptions.ContainsKey("SPIThresholdExceededEvent"));
        Assert.Contains("SPIThresholdEventHandler", subscriptions["SPIThresholdExceededEvent"]);
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_ShouldReturnSubscriptions()
    {
        // Arrange
        _eventBus.Subscribe<SPIThresholdExceededEvent, SPIThresholdEventHandler>();

        // Act
        var subscriptions = await _eventBus.GetActiveSubscriptionsAsync();

        // Assert
        Assert.NotEmpty(subscriptions);
        Assert.True(subscriptions.Count > 0);
    }
}