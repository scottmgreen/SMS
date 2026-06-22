using Microsoft.Extensions.Logging;
using Moq;
using SMS_Application.EventHandlers;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;

namespace PDXSMS_UnitTests.Application.EventHandlers.DomainEventHandlers;

public class DomainEventHandlersBatch4Tests
{
    [Fact]
    public async Task HazardCreatedEventHandler_Should_RunSpiAndPublishUiAndIntegration_OnSuccess()
    {
        var logger = new Mock<ILogger<HazardCreatedEventHandler>>();
        var eventBus = new Mock<IBaseEventBus>();
        var spiAutomation = new Mock<ISPIAutomationService>();

        spiAutomation
            .Setup(x => x.UpdateHazardReportRateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        eventBus
            .Setup(x => x.PublishIntegrationEventAsync(It.IsAny<EmailNotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        eventBus
            .Setup(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new HazardCreatedEventHandler(logger.Object, eventBus.Object, spiAutomation.Object);

        var domainEvent = new HazardCreatedEvent(new SMSEventID("evt-hazard-created"))
        {
            HazardId = "HZ-001",
            HazardCode = "HZ-001",
            HazardType = "Runway",
            HazardCategory = "Ops",
            Description = "Test hazard",
            LocationArea = "Area A",
            ReportCode = "RP-001",
            CreatedBy = "tester",
            CreatedDate = DateTime.UtcNow,
            Priority = HazardPriority.High
        };

        var result = await handler.HandleAsync(domainEvent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        spiAutomation.Verify(x => x.UpdateHazardReportRateAsync(domainEvent.CreatedDate, It.IsAny<CancellationToken>()), Times.Once);
        eventBus.Verify(x => x.PublishIntegrationEventAsync(It.IsAny<EmailNotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()), Times.Once);
        eventBus.Verify(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HazardCreatedEventHandler_Should_StopPipeline_WhenSpiFails()
    {
        var logger = new Mock<ILogger<HazardCreatedEventHandler>>();
        var eventBus = new Mock<IBaseEventBus>();
        var spiAutomation = new Mock<ISPIAutomationService>();

        spiAutomation
            .Setup(x => x.UpdateHazardReportRateAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure<bool>(new Error("SPI_FAIL", "SPI failed")));

        var handler = new HazardCreatedEventHandler(logger.Object, eventBus.Object, spiAutomation.Object);

        var domainEvent = new HazardCreatedEvent(new SMSEventID("evt-hazard-created-fail"))
        {
            HazardCode = "HZ-002",
            CreatedDate = DateTime.UtcNow,
            Priority = HazardPriority.Medium
        };

        var result = await handler.HandleAsync(domainEvent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        eventBus.Verify(x => x.PublishIntegrationEventAsync(It.IsAny<EmailNotificationEvent>(), It.IsAny<EventExecutionMode>(), It.IsAny<CancellationToken>()), Times.Never);
        eventBus.Verify(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), It.IsAny<EventExecutionMode>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MitigationOverdueEventHandler_Should_UpdateCorrectiveActionClosureSpi()
    {
        var logger = new Mock<ILogger<MitigationOverdueEventHandler>>();
        var eventBus = new Mock<IBaseEventBus>();
        var spiAutomation = new Mock<ISPIAutomationService>();

        spiAutomation
            .Setup(x => x.UpdateCorrectiveActionClosureAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var handler = new MitigationOverdueEventHandler(logger.Object, eventBus.Object, spiAutomation.Object);

        var domainEvent = new MitigationOverdueEvent(
            new SMSEventID("evt-mit-overdue"),
            "MIT-001",
            "MIT-CODE-001",
            "HZ-001",
            DateTime.UtcNow.Date,
            4);

        var result = await handler.HandleAsync(domainEvent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        spiAutomation.Verify(x => x.UpdateCorrectiveActionClosureAsync(domainEvent.DueDate, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SPIComplianceChangedEventHandler_Should_PublishUiNotification()
    {
        var logger = new Mock<ILogger<SPIComplianceChangedEventHandler>>();
        var eventBus = new Mock<IBaseEventBus>();

        eventBus
            .Setup(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new SPIComplianceChangedEventHandler(logger.Object, eventBus.Object);

        var domainEvent = new SPIComplianceChangedEvent(
            new SMSEventID("evt-spi-compliance"),
            "SPI-001",
            "Hazard Rate",
            SMS_Domain.Events.SPIComplianceStatus.Compliant,
            SMS_Domain.Events.SPIComplianceStatus.NonCompliant,
            70m,
            90m,
            "Part-139",
            "2026-Q2",
            DateTime.UtcNow,
            requiresRegulatoryReporting: true,
            regulatoryBody: "FAA");

        var result = await handler.HandleAsync(domainEvent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        eventBus.Verify(
            x => x.PublishUIEventAsync(
                It.Is<UINotificationEvent>(e => e.Category == "SPI" && e.ReportId == domainEvent.ReportId),
                EventExecutionMode.Manual,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ReportClosedEventHandler_Should_PublishUiNotification()
    {
        var logger = new Mock<ILogger<ReportClosedEventHandler>>();
        var eventBus = new Mock<IBaseEventBus>();

        eventBus
            .Setup(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ReportClosedEventHandler(logger.Object, eventBus.Object);
        var domainEvent = new ReportClosedEvent(new SMSEventID("evt-report-closed"), "RP-100", "tester", DateTime.UtcNow);

        var result = await handler.HandleAsync(domainEvent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        eventBus.Verify(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReportUpdatedEventHandler_Should_PublishUiNotification()
    {
        var logger = new Mock<ILogger<ReportUpdatedEventHandler>>();
        var eventBus = new Mock<IBaseEventBus>();

        eventBus
            .Setup(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ReportUpdatedEventHandler(logger.Object, eventBus.Object);
        var domainEvent = new ReportUpdatedEvent(new SMSEventID("evt-report-updated"), "RP-101", "tester", DateTime.UtcNow);

        var result = await handler.HandleAsync(domainEvent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        eventBus.Verify(x => x.PublishUIEventAsync(It.IsAny<UINotificationEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()), Times.Once);
    }
}
