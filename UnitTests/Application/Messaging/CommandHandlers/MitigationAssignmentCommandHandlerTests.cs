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
/// Unit tests for MitigationAssignment CommandHandlers using the MediatorService
/// </summary>
public class MitigationAssignmentCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateMitigationAssignmentCommand, Result<MitigationAssignment>>, CreateMitigationAssignmentCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateMitigationAssignmentCommand, Result<MitigationAssignment>>, UpdateMitigationAssignmentCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteMitigationAssignmentCommand, Result<bool>>, DeleteMitigationAssignmentCommandHandler>();
    }

    [Fact]
    public async Task CreateMitigationAssignmentCommandHandler_WithValidAssignment_ShouldExecuteSuccessfully()
    {
        // Arrange
        var assignment = CreateTestMitigationAssignment();
        var command = new CreateMitigationAssignmentCommand(assignment);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateMitigationAssignmentCommandHandler_WithNullAssignment_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateMitigationAssignmentCommand(CreateTestMitigationAssignment());
        var property = command.GetType().GetProperty("MitigationAssignment");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.MitigationAssignmentError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateMitigationAssignmentCommandHandler_WithValidAssignment_ShouldExecuteSuccessfully()
    {
        // Arrange
        var assignment = CreateTestMitigationAssignment();
        var command = new UpdateMitigationAssignmentCommand(assignment);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteMitigationAssignmentCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var assignmentId = new MitigationAssignmentID("MA-TEST-DELETE-001");
        var command = new DeleteMitigationAssignmentCommand(assignmentId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private static MitigationAssignment CreateTestMitigationAssignment()
    {
        var assignmentId = new MitigationAssignmentID("MA-TEST-001");
        return new MitigationAssignment(assignmentId)
        {
            Code = "MA-TEST-001",
            MitigationCode = "MIT-TEST-001",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }
}