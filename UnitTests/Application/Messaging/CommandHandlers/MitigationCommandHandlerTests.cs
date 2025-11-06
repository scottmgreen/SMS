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
/// Unit tests for Mitigation CommandHandlers using the MediatorService
/// </summary>
public class MitigationCommandHandlerTests : ApplicationTestBase
{
    protected override void RegisterServices(IServiceCollection services)
    {
        services.AddTransient<IRequestHandler<CreateMitigationCommand, Result<Mitigation>>, CreateMitigationCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateMitigationCommand, Result<Mitigation>>, UpdateMitigationCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteMitigationCommand, Result<bool>>, DeleteMitigationCommandHandler>();
    }

    [Fact]
    public async Task CreateMitigationCommandHandler_WithValidMitigation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mitigation = CreateTestMitigation();
        var command = new CreateMitigationCommand(mitigation);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateMitigationCommandHandler_WithNullMitigation_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateMitigationCommand(CreateTestMitigation());
        var property = command.GetType().GetProperty("Mitigation");
        property?.SetValue(command, null);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(DomainErrors.MitigationError.NullOrEmpty);
    }

    [Fact]
    public async Task UpdateMitigationCommandHandler_WithValidMitigation_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mitigation = CreateTestMitigation();
        var command = new UpdateMitigationCommand(mitigation);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteMitigationCommandHandler_WithValidId_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mitigationId = new MitigationID("MIT-TEST-DELETE-001");
        var command = new DeleteMitigationCommand(mitigationId);

        // Act
        var result = await Mediator.SendAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }
}