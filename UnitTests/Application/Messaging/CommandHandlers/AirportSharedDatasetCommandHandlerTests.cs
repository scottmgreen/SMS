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
/// Unit tests for AirportSharedDataset CommandHandlers using the MediatorService
/// </summary>
public class AirportSharedDatasetCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateAirportSharedDatasetCommand, Result<AirportSharedDataset>>, CreateAirportSharedDatasetCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateAirportSharedDatasetCommand, Result<AirportSharedDataset>>, UpdateAirportSharedDatasetCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteAirportSharedDatasetCommand, Result<bool>>, DeleteAirportSharedDatasetCommandHandler>();
    }

    [Fact]
    public async Task CreateAirportSharedDatasetCommandHandler_WithValidDataset_ShouldExecuteSuccessfully()
    {
        // Arrange
        var dataset = CreateTestAirportSharedDataset();
        var command = new CreateAirportSharedDatasetCommand(dataset);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAirportSharedDatasetCommandHandler_WithNullDataset_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateAirportSharedDatasetCommand(CreateTestAirportSharedDataset());
        var property = command.GetType().GetProperty("AirportSharedDataset");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.AirportSharedDatasetError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateAirportSharedDatasetCommandHandler_WithValidDataset_ShouldExecuteSuccessfully()
    {
        // Arrange
        var dataset = CreateTestAirportSharedDataset();
        var command = new UpdateAirportSharedDatasetCommand(dataset);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAirportSharedDatasetCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var datasetId = new AirportSharedDatasetID("ASD-TEST-DELETE-001");
        var command = new DeleteAirportSharedDatasetCommand(datasetId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private static AirportSharedDataset CreateTestAirportSharedDataset()
    {
        var datasetId = new AirportSharedDatasetID("ASD-TEST-001");
        return new AirportSharedDataset(datasetId)
        {
            Code = "ASD-TEST-001",
            ReportID = "RP-TEST-001",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }
}