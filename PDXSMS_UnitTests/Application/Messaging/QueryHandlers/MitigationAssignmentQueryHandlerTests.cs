using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for MitigationAssignment QueryHandlers using the MediatorService
/// </summary>
public class MitigationAssignmentQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetMitigationAssignmentByIdQuery, Result<MitigationAssignment>>, GetMitigationAssignmentByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllMitigationAssignmentsQuery, Result<List<MitigationAssignment>>>, GetAllMitigationAssignmentsQueryHandler>();
    }

    [Fact]
    public async Task GetMitigationAssignmentByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var assignmentId = new MitigationAssignmentID("MA-TEST-001");
        var query = new GetMitigationAssignmentByIdQuery(assignmentId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllMitigationAssignmentsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllMitigationAssignmentsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllMitigationAssignmentsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllMitigationAssignmentsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}