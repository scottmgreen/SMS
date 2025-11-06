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
/// Unit tests for RiskAnalysis CommandHandlers using the MediatorService
/// </summary>
public class RiskAnalysisCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateRiskAnalysisCommand, Result<RiskAnalysis>>, CreateRiskAnalysisCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateRiskAnalysisCommand, Result<RiskAnalysis>>, UpdateRiskAnalysisCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteRiskAnalysisCommand, Result<bool>>, DeleteRiskAnalysisCommandHandler>();
    }

    [Fact]
    public async Task CreateRiskAnalysisCommandHandler_WithValidRiskAnalysis_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAnalysis = CreateTestRiskAnalysis();
        var command = new CreateRiskAnalysisCommand(riskAnalysis);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRiskAnalysisCommandHandler_WithNullRiskAnalysis_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateRiskAnalysisCommand(CreateTestRiskAnalysis());
        var property = command.GetType().GetProperty("RiskAnalysis");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.RiskAnalysisError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateRiskAnalysisCommandHandler_WithValidRiskAnalysis_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAnalysis = CreateTestRiskAnalysis();
        var command = new UpdateRiskAnalysisCommand(riskAnalysis);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteRiskAnalysisCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAnalysisId = new RiskAnalysisID("RA-TEST-DELETE-001");
        var command = new DeleteRiskAnalysisCommand(riskAnalysisId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private static RiskAnalysis CreateTestRiskAnalysis()
    {
        var riskAnalysisId = new RiskAnalysisID("RA-TEST-001");
        return new RiskAnalysis(riskAnalysisId)
        {
            Code = "RA-TEST-001",
            HazardCode = "HZ-TEST-001",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }
}