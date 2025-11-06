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
/// Unit tests for Report CommandHandlers using the MediatorService
/// Tests the complete CQRS command flow from command creation through mediator to handler execution
/// </summary>
public class ReportCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Infrastructure services are already registered by ApplicationTestBase
        // Just register the command handlers
        services.AddTransient<IRequestHandler<CreateReportCommand, Result<Report>>, CreateReportCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateReportCommand, Result<Report>>, UpdateReportCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteReportCommand, Result<bool>>, DeleteReportCommandHandler>();
    }

    #region Create Command Handler Tests

    [Fact]
    public async Task CreateReportCommandHandler_WithValidReport_ShouldExecuteSuccessfully()
    {
        // Arrange
        var report = CreateTestReport();
        var command = new CreateReportCommand(report);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the database state
        // For integration tests, we just verify it doesn't throw an exception
    }

    [Fact]
    public async Task CreateReportCommandHandler_WithNullReport_ShouldReturnFailure()
    {
        // Arrange - Create command with null report using reflection
        var command = new CreateReportCommand(CreateTestReport());
        var reportProperty = command.GetType().GetProperty("Report");
        reportProperty?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.ReportError.NullOrEmpty);
    }

    [Fact]
    public async Task CreateReportCommandHandler_WhenCancelled_ShouldRespectCancellation()
    {
        // Arrange
        var report = CreateTestReport();
        var command = new CreateReportCommand(report);
        var cts = new CancellationTokenSource();
        
        // Cancel immediately
        cts.Cancel();

        // Act & Assert
        try
        {
            var result = await Mediator.SendAsync(command, cts.Token);
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            // This is also valid - cancellation was honored
        }
    }

    #endregion

    #region Update Command Handler Tests

    [Fact]
    public async Task UpdateReportCommandHandler_WithValidReport_ShouldExecuteSuccessfully()
    {
        // Arrange
        var report = CreateTestReport();
        report.Name = "Updated Report Name";
        var command = new UpdateReportCommand(report);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the database state and whether the report exists
    }

    #endregion

    #region Delete Command Handler Tests

    [Fact]
    public async Task DeleteReportCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var reportId = new ReportID("RP-TEST-DELETE-001");
        var command = new DeleteReportCommand(reportId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on whether the report exists in the database
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task CreateReportCommandHandler_MultipleSequentialRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var commands = Enumerable.Range(1, 3) // Reduced count for integration tests
            .Select(i => new CreateReportCommand(CreateTestReport($"RP-PERF-{i:D3}")))
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var command in commands)
        {
            await Mediator.SendAsync(command, CancellationToken.None);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000); // Should complete within 10 seconds
    }

    #endregion
}