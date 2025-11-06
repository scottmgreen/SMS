using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for Investigation QueryHandlers using the MediatorService
/// </summary>
public class InvestigationQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetInvestigationByIdQuery, Result<Investigation>>, GetInvestigationByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllInvestigationsQuery, Result<List<Investigation>>>, GetAllInvestigationsQueryHandler>();
    }

    [Fact]
    public async Task GetInvestigationByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var investigationId = new InvestigationID("INV-TEST-001");
        var query = new GetInvestigationByIdQuery(investigationId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllInvestigationsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllInvestigationsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllInvestigationsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllInvestigationsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}