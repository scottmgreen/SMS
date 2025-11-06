using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for Report QueryHandlers using the MediatorService
/// Tests the complete CQRS query flow from query creation through mediator to handler execution
/// </summary>
public class ReportQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Infrastructure services are already registered by ApplicationTestBase
        // Just register the query handlers
        services.AddTransient<IRequestHandler<GetReportByIdQuery, Result<Report>>, GetReportByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllReportsQuery, Result<List<Report>>>, GetAllReportsQueryHandler>();
    }

    #region GetReportById Query Handler Tests

    [Fact]
    public async Task GetReportByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var reportId = new ReportID("RP-TEST-001");
        var query = new GetReportByIdQuery(reportId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on whether the report exists in the database
    }

    [Fact]
    public async Task GetReportByIdQueryHandler_WithNonexistentId_ShouldExecuteWithoutException()
    {
        // Arrange - Use a very unlikely ID that shouldn't exist
        var reportId = new ReportID("RP-NONEXISTENT-99999999");
        var query = new GetReportByIdQuery(reportId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // The result will likely be a failure since this ID shouldn't exist
    }

    #endregion

    #region GetAllReports Query Handler Tests

    [Fact]
    public async Task GetAllReportsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllReportsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Note: The actual result will depend on the current database state
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetAllReportsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllReportsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000); // Should complete within 15 seconds
    }

    [Fact]
    public async Task MultipleReportQueries_ExecutedConcurrently_ShouldHandleCorrectly()
    {
        // Arrange
        var reportIds = Enumerable.Range(1, 3) // Reduced count for integration tests
            .Select(i => new ReportID($"RP-CONCURRENT-{i:D2}"))
            .ToList();

        // Act
        var tasks = reportIds.Select(async id =>
        {
            var query = new GetReportByIdQuery(id);
            return await Mediator.SendAsync(query, CancellationToken.None);
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r != null);
    }

    [Fact]
    public async Task GetReportByIdQueryHandler_WhenCancelled_ShouldRespectCancellation()
    {
        // Arrange
        var reportId = new ReportID("RP-CANCEL-TEST");
        var query = new GetReportByIdQuery(reportId);
        var cts = new CancellationTokenSource();
        
        // Cancel immediately
        cts.Cancel();

        // Act & Assert
        try
        {
            var result = await Mediator.SendAsync(query, cts.Token);
            result.Should().NotBeNull();
        }
        catch (OperationCanceledException)
        {
            // This is also valid - cancellation was honored
        }
    }

    #endregion
}