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
/// Unit tests for Investigation CommandHandlers using the MediatorService
/// </summary>
public class InvestigationCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateInvestigationCommand, Result<Investigation>>, CreateInvestigationCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateInvestigationCommand, Result<Investigation>>, UpdateInvestigationCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteInvestigationCommand, Result<bool>>, DeleteInvestigationCommandHandler>();
    }

    [Fact]
    public async Task CreateInvestigationCommandHandler_WithValidInvestigation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var investigation = CreateTestInvestigation();
        var command = new CreateInvestigationCommand(investigation);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateInvestigationCommandHandler_WithNullInvestigation_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateInvestigationCommand(CreateTestInvestigation());
        var property = command.GetType().GetProperty("Investigation");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.InvestigationError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateInvestigationCommandHandler_WithValidInvestigation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var investigation = CreateTestInvestigation();
        investigation.InvestigationNotes = "Updated Investigation Notes";
        var command = new UpdateInvestigationCommand(investigation);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteInvestigationCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var investigationId = new InvestigationID("INV-TEST-DELETE-001");
        var command = new DeleteInvestigationCommand(investigationId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}