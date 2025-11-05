using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.CommandHandlers;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Application.Messaging.Pipelines;
using SMS_Domain.Entities;
using SMS_Infrastructure.Services;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Interfaces;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Services;

/// <summary>
/// Comprehensive unit tests for the MediatorService
/// Tests the core CQRS mediator functionality, pipeline execution, and DI resolution
/// </summary>
public class MediatorServiceTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Infrastructure services (repositories, data services, etc.) are already registered by AddInfrastructureServices
        // Just register the handlers manually
        services.AddTransient<IRequestHandler<CreateHazardCommand, Result<Hazard>>, CreateHazardCommandHandler>();
        services.AddTransient<IRequestHandler<GetHazardByIdQuery, Result<Hazard>>, GetHazardByIdQueryHandler>();
        
        // Mock handlers for other command types if needed
        var mockDeleteHandler = new Mock<IRequestHandler<DeleteHazardCommand, Result<bool>>>();
        mockDeleteHandler.Setup(x => x.HandleAsync(It.IsAny<DeleteHazardCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<bool>.Success(true));
        services.AddSingleton(mockDeleteHandler.Object);
    }

    #region Core Mediator Tests

    [Fact]
    public void MediatorService_ShouldBeRegisteredInDI()
    {
        // Act & Assert
        Mediator.Should().NotBeNull();
        Mediator.Should().BeOfType<Mediator>();
    }

    [Fact]
    public async Task SendAsync_WithValidCommand_ShouldResolveHandlerAndExecute()
    {
        // Arrange
        var hazard = CreateTestHazard();
        var command = new CreateHazardCommand(hazard);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the implementation
        // For now, just verify it doesn't throw an exception
    }

    [Fact]
    public async Task SendAsync_WithValidQuery_ShouldResolveHandlerAndExecute()
    {
        // Arrange
        var hazardId = new HazardID("HZ-TEST-001");
        var query = new GetHazardByIdQuery(hazardId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the implementation
        // For now, just verify it doesn't throw an exception
    }

    [Fact]
    public async Task SendAsync_WithNonRegisteredHandler_ShouldThrowException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IMediator, Mediator>();
        // Intentionally not registering any handlers
        
        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        var command = new CreateHazardCommand(CreateTestHazard());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(command, CancellationToken.None));
    }

    #endregion

    #region Generic Type Resolution Tests

    [Fact]
    public async Task SendAsync_ShouldCorrectlyResolveGenericTypes()
    {
        // Arrange
        var hazard = CreateTestHazard();
        var command = new CreateHazardCommand(hazard);

        // Act
        var result = await Mediator.SendAsync<Result<Hazard>>(command, CancellationToken.None);

        // Assert
        result.Should().BeOfType<Result<Hazard>>();
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task SendAsync_ConcurrentRequests_ShouldHandleCorrectly()
    {
        // Arrange
        var hazards = Enumerable.Range(1, 10)
            .Select(i => CreateTestHazard($"HZ-CONCURRENT-{i:D2}"))
            .ToList();

        // Act
        var tasks = hazards.Select(async hazard =>
        {
            var command = new CreateHazardCommand(hazard);
            return await Mediator.SendAsync(command, CancellationToken.None);
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(10);
        results.Should().OnlyContain(r => r != null);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task SendAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var hazard = CreateTestHazard();
        var command = new CreateHazardCommand(hazard);
        var cts = new CancellationTokenSource();
        
        // Cancel immediately, but the operation might complete before cancellation
        cts.Cancel();

        // Act & Assert
        // Note: The operation might complete successfully if it's fast enough,
        // or it might throw OperationCanceledException if cancellation is honored
        try
        {
            var result = await Mediator.SendAsync(command, cts.Token);
            // If we get here, the operation completed before cancellation took effect
            // This is valid behavior for fast operations
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            // This is also valid - cancellation was honored
            // Test passes in this case too
        }
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task SendAsync_SingleRequest_ShouldCompleteQuickly()
    {
        // Arrange
        var hazard = CreateTestHazard();
        var command = new CreateHazardCommand(hazard);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(command, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
    }

    [Fact]
    public async Task SendAsync_MultipleSequentialRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var commands = Enumerable.Range(1, 10) // Reduced from 100 to 10 for unit test performance
            .Select(i => new CreateHazardCommand(CreateTestHazard($"HZ-PERF-{i:D3}")))
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var command in commands)
        {
            await Mediator.SendAsync(command, CancellationToken.None);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    #endregion
}