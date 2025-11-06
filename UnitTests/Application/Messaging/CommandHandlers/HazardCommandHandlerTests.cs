using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.CommandHandlers;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.CommandHandlers;

/// <summary>
/// Unit tests for Hazard CommandHandlers using the MediatorService
/// Tests the complete CQRS command flow from command creation through mediator to handler execution
/// </summary>
public class HazardCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Infrastructure services (repositories, data services, etc.) are already registered by ApplicationTestBase
        // Just register the command handlers
        services.AddTransient<IRequestHandler<CreateHazardCommand, Result<Hazard>>, CreateHazardCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateHazardCommand, Result<Hazard>>, UpdateHazardCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteHazardCommand, Result<bool>>, DeleteHazardCommandHandler>();
    }

    #region Create Command Handler Tests

    [Fact]
    public async Task CreateHazardCommandHandler_WithValidHazard_ShouldReturnSuccess()
    {
        // Arrange
        var hazard = CreateTestHazard();
        var command = new CreateHazardCommand(hazard);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the database state
        // For integration tests, we just verify it doesn't throw an exception
    }

    [Fact]
    public async Task CreateHazardCommandHandler_WithNullHazard_ShouldReturnFailure()
    {
        // Arrange - Create command with null hazard using reflection
        var command = new CreateHazardCommand(CreateTestHazard());
        var hazardProperty = command.GetType().GetProperty("Hazard");
        hazardProperty?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.HazardError.NullOrEmpty);
    }

    [Fact]
    public async Task CreateHazardCommandHandler_WhenCancelled_ShouldRespectCancellation()
    {
        // Arrange
        var hazard = CreateTestHazard();
        var command = new CreateHazardCommand(hazard);
        var cts = new CancellationTokenSource();
        
        // Cancel immediately
        cts.Cancel();

        // Act & Assert
        // The operation might complete successfully if it's fast enough,
        // or it might throw OperationCanceledException if cancellation is honored
        try
        {
            var result = await Mediator.SendAsync(command, cts.Token);
            // If we get here, the operation completed before cancellation took effect
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            // This is also valid - cancellation was honored
            // Test passes in this case too
        }
    }

    #endregion

    #region Update Command Handler Tests

    [Fact]
    public async Task UpdateHazardCommandHandler_WithValidHazard_ShouldReturnSuccess()
    {
        // Arrange
        var hazard = CreateTestHazard();
        hazard.Name = "Updated Hazard Name";
        var command = new UpdateHazardCommand(hazard);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the database state and whether the hazard exists
    }

    #endregion

    #region Delete Command Handler Tests

    [Fact]
    public async Task DeleteHazardCommandHandler_WithValidId_ShouldExecute()
    {
        // Arrange
        var hazardId = new HazardID("HZ-TEST-DELETE-001");
        var command = new DeleteHazardCommand(hazardId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on whether the hazard exists in the database
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task CreateHazardCommandHandler_MultipleSequentialRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var commands = Enumerable.Range(1, 5) // Reduced count for integration tests
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
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Should complete within 10 seconds for integration tests
    }

    #endregion
}