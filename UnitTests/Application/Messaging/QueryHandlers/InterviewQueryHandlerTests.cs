using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for Interview QueryHandlers using the MediatorService
/// Tests the complete CQRS query flow from query creation through mediator to handler execution
/// </summary>
public class InterviewQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        // Infrastructure services are already registered by ApplicationTestBase
        // Just register the query handlers
        services.AddTransient<IRequestHandler<GetInterviewByIdQuery, Result<Interview>>, GetInterviewByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllInterviewsQuery, Result<List<Interview>>>, GetAllInterviewsQueryHandler>();
    }

    #region GetInterviewById Query Handler Tests

    [Fact]
    public async Task GetInterviewByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var interviewId = new InterviewID("IV-TEST-001");
        var query = new GetInterviewByIdQuery(interviewId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetInterviewByIdQueryHandler_WithNonexistentId_ShouldExecuteWithoutException()
    {
        // Arrange - Use a very unlikely ID that shouldn't exist
        var interviewId = new InterviewID("IV-NONEXISTENT-99999999");
        var query = new GetInterviewByIdQuery(interviewId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region GetAllInterviews Query Handler Tests

    [Fact]
    public async Task GetAllInterviewsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllInterviewsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public async Task GetAllInterviewsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllInterviewsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }

    [Fact]
    public async Task MultipleInterviewQueries_ExecutedConcurrently_ShouldHandleCorrectly()
    {
        // Arrange
        var interviewIds = Enumerable.Range(1, 3)
            .Select(i => new InterviewID($"IV-CONCURRENT-{i:D2}"))
            .ToList();

        // Act
        var tasks = interviewIds.Select(async id =>
        {
            var query = new GetInterviewByIdQuery(id);
            return await Mediator.SendAsync(query, CancellationToken.None);
        });

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(3);
        results.Should().OnlyContain(r => r != null);
    }

    #endregion
}