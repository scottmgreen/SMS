using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for ScoringPanel QueryHandlers using the MediatorService
/// </summary>
public class ScoringPanelQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetScoringPanelByIdQuery, Result<ScoringPanel>>, GetScoringPanelByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllScoringPanelsQuery, Result<List<ScoringPanel>>>, GetAllScoringPanelsQueryHandler>();
    }

    [Fact]
    public async Task GetScoringPanelByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var scoringPanelId = new ScoringPanelID("SP-TEST-001");
        var query = new GetScoringPanelByIdQuery(scoringPanelId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllScoringPanelsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllScoringPanelsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllScoringPanelsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllScoringPanelsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}