using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for AirportSharedDataset QueryHandlers using the MediatorService
/// </summary>
public class AirportSharedDatasetQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetAirportSharedDatasetByIdQuery, Result<AirportSharedDataset>>, GetAirportSharedDatasetByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllAirportSharedDatasetsQuery, Result<List<AirportSharedDataset>>>, GetAllAirportSharedDatasetsQueryHandler>();
    }

    [Fact]
    public async Task GetAirportSharedDatasetByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var datasetId = new AirportSharedDatasetID("ASD-TEST-001");
        var query = new GetAirportSharedDatasetByIdQuery(datasetId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAirportSharedDatasetsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllAirportSharedDatasetsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAirportSharedDatasetsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllAirportSharedDatasetsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}