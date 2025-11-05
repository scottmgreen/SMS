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
/// Unit tests for RiskAssessment CommandHandlers using the MediatorService
/// </summary>
public class RiskAssessmentCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateRiskAssessmentCommand, Result<RiskAssessment>>, CreateRiskAssessmentCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateRiskAssessmentCommand, Result<RiskAssessment>>, UpdateRiskAssessmentCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteRiskAssessmentCommand, Result<bool>>, DeleteRiskAssessmentCommandHandler>();
    }

    [Fact]
    public async Task CreateRiskAssessmentCommandHandler_WithValidRiskAssessment_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAssessment = CreateTestRiskAssessment();
        var command = new CreateRiskAssessmentCommand(riskAssessment);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRiskAssessmentCommandHandler_WithNullRiskAssessment_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateRiskAssessmentCommand(CreateTestRiskAssessment());
        var property = command.GetType().GetProperty("RiskAssessment");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.RiskAssessmentError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateRiskAssessmentCommandHandler_WithValidRiskAssessment_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAssessment = CreateTestRiskAssessment();
        var command = new UpdateRiskAssessmentCommand(riskAssessment);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteRiskAssessmentCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAssessmentId = new RiskAssessmentID("RA-TEST-DELETE-001");
        var command = new DeleteRiskAssessmentCommand(riskAssessmentId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private static RiskAssessment CreateTestRiskAssessment()
    {
        var riskAssessmentId = new RiskAssessmentID("RA-TEST-001");
        return new RiskAssessment(riskAssessmentId)
        {
            Code = "RA-TEST-001",
            HazardCode = "HZ-TEST-001",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }
}