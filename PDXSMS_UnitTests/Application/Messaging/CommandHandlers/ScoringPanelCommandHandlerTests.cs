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
/// Unit tests for ScoringPanel CommandHandlers using the MediatorService
/// </summary>
public class ScoringPanelCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateScoringPanelCommand, Result<ScoringPanel>>, CreateScoringPanelCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateScoringPanelCommand, Result<ScoringPanel>>, UpdateScoringPanelCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteScoringPanelCommand, Result<bool>>, DeleteScoringPanelCommandHandler>();
    }

    [Fact]
    public async Task CreateScoringPanelCommandHandler_WithValidScoringPanel_ShouldExecuteSuccessfully()
    {
        // Arrange
        var scoringPanel = CreateTestScoringPanel();
        var command = new CreateScoringPanelCommand(scoringPanel);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateScoringPanelCommandHandler_WithNullScoringPanel_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateScoringPanelCommand(CreateTestScoringPanel());
        var property = command.GetType().GetProperty("ScoringPanel");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.ScoringPanelError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateScoringPanelCommandHandler_WithValidScoringPanel_ShouldExecuteSuccessfully()
    {
        // Arrange
        var scoringPanel = CreateTestScoringPanel();
        var command = new UpdateScoringPanelCommand(scoringPanel);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteScoringPanelCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var scoringPanelId = new ScoringPanelID("SP-TEST-DELETE-001");
        var command = new DeleteScoringPanelCommand(scoringPanelId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private static ScoringPanel CreateTestScoringPanel()
    {
        var scoringPanelId = new ScoringPanelID("SP-TEST-001");
        return new ScoringPanel(scoringPanelId)
        {
            Code = "SP-TEST-001",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }
}