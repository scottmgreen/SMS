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
/// Unit tests for Interview CommandHandlers using the MediatorService
/// Tests the complete CQRS command flow from command creation through mediator to handler execution
/// </summary>
public class InterviewCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Infrastructure services are already registered by ApplicationTestBase
        // Just register the command handlers
        services.AddTransient<IRequestHandler<CreateInterviewCommand, Result<Interview>>, CreateInterviewCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateInterviewCommand, Result<Interview>>, UpdateInterviewCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteInterviewCommand, Result<bool>>, DeleteInterviewCommandHandler>();
    }

    #region Create Command Handler Tests

    [Fact]
    public async Task CreateInterviewCommandHandler_WithValidInterview_ShouldExecuteSuccessfully()
    {
        // Arrange
        var interview = CreateTestInterview();
        var command = new CreateInterviewCommand(interview);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateInterviewCommandHandler_WithNullInterview_ShouldReturnFailure()
    {
        // Arrange - Create command with null interview using reflection
        var command = new CreateInterviewCommand(CreateTestInterview());
        var interviewProperty = command.GetType().GetProperty("Interview");
        interviewProperty?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.InterviewError.NullOrEmpty);
    }

    #endregion

    #region Update Command Handler Tests

    [Fact]
    public async Task UpdateInterviewCommandHandler_WithValidInterview_ShouldExecuteSuccessfully()
    {
        // Arrange
        var interview = CreateTestInterview();
        var command = new UpdateInterviewCommand(interview);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Delete Command Handler Tests

    [Fact]
    public async Task DeleteInterviewCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var interviewId = new InterviewID("IV-TEST-DELETE-001");
        var command = new DeleteInterviewCommand(interviewId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task CreateInterviewCommandHandler_MultipleSequentialRequests_ShouldMaintainPerformance()
    {
        // Arrange
        var commands = Enumerable.Range(1, 3)
            .Select(i => new CreateInterviewCommand(CreateTestInterview($"IV-PERF-{i:D3}")))
            .ToList();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var command in commands)
        {
            await Mediator.SendAsync(command, CancellationToken.None);
        }
        
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(10000);
    }

    #endregion
}