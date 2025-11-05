using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for RiskAnalysis QueryHandlers using the MediatorService
/// </summary>
public class RiskAnalysisQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetRiskAnalysisByIdQuery, Result<RiskAnalysis>>, GetRiskAnalysisByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllRiskAnalysisQuery, Result<List<RiskAnalysis>>>, GetAllRiskAnalysisQueryHandler>();
    }

    [Fact]
    public async Task GetRiskAnalysisByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAnalysisId = new RiskAnalysisID("RA-TEST-001");
        var query = new GetRiskAnalysisByIdQuery(riskAnalysisId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllRiskAnalysisQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllRiskAnalysisQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllRiskAnalysisQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllRiskAnalysisQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}