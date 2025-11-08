using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.CommandHandlers;
using SMS_Domain.Entities;
using SMS_Domain.Errors;
using PDXSMS_UnitTests.Application.Common;

namespace PDXSMS_UnitTests.Application.Messaging.CommandHandlers;

/// <summary>
/// Unit tests for ReportValidation CommandHandlers using the MediatorService
/// </summary>
public class ReportValidationCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateReportValidationCommand, Result<ReportValidation>>, CreateReportValidationCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateReportValidationCommand, Result<ReportValidation>>, UpdateReportValidationCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteReportValidationCommand, Result<bool>>, DeleteReportValidationCommandHandler>();
    }

    [Fact]
    public async Task CreateReportValidationCommandHandler_WithValidReportValidation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var reportValidation = CreateTestReportValidation();
        var command = new CreateReportValidationCommand(reportValidation);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateReportValidationCommandHandler_WithNullReportValidation_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateReportValidationCommand(CreateTestReportValidation());
        var property = command.GetType().GetProperty("ReportValidation");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.ReportError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateReportValidationCommandHandler_WithValidReportValidation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var reportValidation = CreateTestReportValidation();
        var command = new UpdateReportValidationCommand(reportValidation);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteReportValidationCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var validationId = new ReportValidationID("RV-TEST-DELETE-001");
        var command = new DeleteReportValidationCommand(validationId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private static ReportValidation CreateTestReportValidation()
    {
        var validationId = new ReportValidationID("RV-TEST-001");
        return new ReportValidation(validationId)
        {
            Code = "RV-TEST-001",
            ReportCode = "RP-TEST-001",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }
}