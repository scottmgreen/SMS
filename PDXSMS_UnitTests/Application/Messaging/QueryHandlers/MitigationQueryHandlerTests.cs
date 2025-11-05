using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for Mitigation QueryHandlers using the MediatorService
/// </summary>
public class MitigationQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetMitigationByIdQuery, Result<Mitigation>>, GetMitigationByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllMitigationsQuery, Result<List<Mitigation>>>, GetAllMitigationsQueryHandler>();
    }

    [Fact]
    public async Task GetMitigationByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mitigationId = new MitigationID("MIT-TEST-001");
        var query = new GetMitigationByIdQuery(mitigationId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllMitigationsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllMitigationsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllMitigationsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllMitigationsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}