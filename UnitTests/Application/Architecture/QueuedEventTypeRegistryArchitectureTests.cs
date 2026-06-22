using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Application.Services;
using SMS_Domain.Enums;

namespace PDXSMS_UnitTests.Application.Architecture;

public class QueuedEventTypeRegistryArchitectureTests
{
    [Fact]
    public void Registry_ShouldResolve_CoreKnownEventTypes_ForReplay()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<QueuedEventTypeRegistry>();

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<QueuedEventTypeRegistry>();

        // Act + Assert (Domain)
        registry.TryResolveDomainEventType(SMS_Domain.Enums.EventType.HazardCreated.Value, out var hazardCreatedType)
            .Should().BeTrue();
        hazardCreatedType.Should().NotBeNull();

        registry.TryResolveDomainEventType(SMS_Domain.Enums.EventType.MitigationOverdue.Value, out var mitigationOverdueType)
            .Should().BeTrue();
        mitigationOverdueType.Should().NotBeNull();

        // Act + Assert (Integration)
        registry.TryResolveIntegrationEventType(SMS_Domain.Enums.EventType.EmailNotification.Value, out var emailType)
            .Should().BeTrue();
        emailType.Should().NotBeNull();

        // Act + Assert (UI)
        registry.TryResolveUIEventType(SMS_Domain.Enums.EventType.UINotification.Value, out var uiNotificationType)
            .Should().BeTrue();
        uiNotificationType.Should().NotBeNull();

        registry.TryResolveUIEventType(SMS_Domain.Events.SPIDashboardRefreshEvent.TypeValue, out var spiDashboardRefreshType)
            .Should().BeTrue();
        spiDashboardRefreshType.Should().NotBeNull();
    }

    [Fact]
    public void Registry_ShouldExpose_NonEmptyKnownTypeCollections_ForEachCategory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<QueuedEventTypeRegistry>();

        using var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<QueuedEventTypeRegistry>();

        // Act
        var domainTypes = registry.GetKnownTypes(EventCategory.DomainEvent);
        var integrationTypes = registry.GetKnownTypes(EventCategory.IntegrationEvent);
        var uiTypes = registry.GetKnownTypes(EventCategory.UIEvent);

        // Assert
        domainTypes.Should().NotBeNullOrEmpty();
        integrationTypes.Should().NotBeNullOrEmpty();
        uiTypes.Should().NotBeNullOrEmpty();
    }
}
