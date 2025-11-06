using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for RiskAssessment QueryHandlers using the MediatorService
/// </summary>
public class RiskAssessmentQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetRiskAssessmentByIdQuery, Result<RiskAssessment>>, GetRiskAssessmentByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllRiskAssessmentsQuery, Result<List<RiskAssessment>>>, GetAllRiskAssessmentsQueryHandler>();
    }

    [Fact]
    public async Task GetRiskAssessmentByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var riskAssessmentId = new RiskAssessmentID("RA-TEST-001");
        var query = new GetRiskAssessmentByIdQuery(riskAssessmentId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllRiskAssessmentsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllRiskAssessmentsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllRiskAssessmentsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllRiskAssessmentsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}