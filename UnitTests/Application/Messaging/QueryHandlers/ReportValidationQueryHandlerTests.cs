using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Domain.Entities;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.QueryHandlers;

/// <summary>
/// Unit tests for ReportValidation QueryHandlers using the MediatorService
/// </summary>
public class ReportValidationQueryHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<GetReportValidationByIdQuery, Result<ReportValidation>>, GetReportValidationByIdQueryHandler>();
        services.AddTransient<IRequestHandler<GetAllReportValidationsQuery, Result<List<ReportValidation>>>, GetAllReportValidationsQueryHandler>();
    }

    [Fact]
    public async Task GetReportValidationByIdQueryHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var validationId = new ReportValidationID("RV-TEST-001");
        var query = new GetReportValidationByIdQuery(validationId);

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllReportValidationsQueryHandler_ShouldExecuteSuccessfully()
    {
        // Arrange
        var query = new GetAllReportValidationsQuery();

        // Act
        var result = await Mediator.SendAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllReportValidationsQueryHandler_ShouldCompleteWithinReasonableTime()
    {
        // Arrange
        var query = new GetAllReportValidationsQuery();

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await Mediator.SendAsync(query, CancellationToken.None);
        stopwatch.Stop();

        // Assert
        result.Should().NotBeNull();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(15000);
    }
}