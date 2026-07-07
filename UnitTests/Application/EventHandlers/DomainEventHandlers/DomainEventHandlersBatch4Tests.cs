using Microsoft.Extensions.Logging;
using Moq;
using SMS_Application.CommandHandlers;
using SMS_Application.Commands;
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

    [Fact]
    public async Task AddSPIDataPointCommandHandler_Should_PublishComplianceThresholdAndRefreshEvents()
    {
        var logger = new Mock<ILogger<AddSPIDataPointCommandHandler>>();
        var eventBus = new Mock<IBaseEventBus>();
        var spiService = new Mock<ISafetyPerformanceIndicatorService>();

        var spi = new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("PI-0001"),
            "Hazard Rate",
            "Hazard reporting rate",
            SMSSafetyPerformanceIndicatorType.IncidentRate,
            "tester")
        {
            Code = "SPI-001",
            TargetValue = 90m,
            WarningThreshold = 75m,
            CriticalThreshold = 95m
        };

        var dataPoint = new SPIDataPoint(new SPIDataPointID("DP-0001"))
        {
            Code = "DP-0001",
            SPIId = "SPI-001",
            Value = 96m,
            MeasurementDate = DateTime.UtcNow,
            Period = "2026-Q3",
            DataSource = "UnitTest"
        };

        spi.DataPoints.Add(dataPoint);

        spiService
            .Setup(x => x.AddSPIDataPointAsync(dataPoint.SPIId, dataPoint, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SafetyPerformanceIndicator>.Success(spi));

        eventBus
            .Setup(x => x.PublishDomainEventAsync(It.IsAny<SPIComplianceChangedEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        eventBus
            .Setup(x => x.PublishDomainEventAsync(It.IsAny<SPIThresholdExceededEvent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        eventBus
            .Setup(x => x.PublishUIEventAsync(It.IsAny<SPIDashboardRefreshEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new AddSPIDataPointCommandHandler(spiService.Object, eventBus.Object, logger.Object);
        var command = new AddSPIDataPointCommand(dataPoint);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        eventBus.Verify(x => x.PublishDomainEventAsync(It.IsAny<SPIComplianceChangedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        eventBus.Verify(x => x.PublishDomainEventAsync(It.IsAny<SPIThresholdExceededEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        eventBus.Verify(x => x.PublishUIEventAsync(It.IsAny<SPIDashboardRefreshEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ArchiveOldSPIDataCommandHandler_Should_DeleteOldDataPoints_AndPublishRefresh()
    {
        var logger = new Mock<ILogger<ArchiveOldSPIDataCommandHandler>>();
        var eventBus = new Mock<IBaseEventBus>();
        var spiService = new Mock<ISafetyPerformanceIndicatorService>();

        var spi = new SafetyPerformanceIndicator(
            new SafetyPerformanceIndicatorID("PI-0002"),
            "Corrective Action Closure",
            "Closure metric",
            SMSSafetyPerformanceIndicatorType.CorrectiveActionClosure,
            "tester")
        {
            Code = "SPI-ARCHIVE"
        };

        var oldPoint = new SPIDataPoint(new SPIDataPointID("DP-OLD"))
        {
            Code = "DP-OLD",
            SPIId = "SPI-ARCHIVE",
            Value = 10m,
            MeasurementDate = new DateTime(2025, 1, 1),
            Period = "2025-Q1",
            DataSource = "UnitTest"
        };

        var recentPoint = new SPIDataPoint(new SPIDataPointID("DP-NEW"))
        {
            Code = "DP-NEW",
            SPIId = "SPI-ARCHIVE",
            Value = 15m,
            MeasurementDate = new DateTime(2026, 1, 1),
            Period = "2026-Q1",
            DataSource = "UnitTest"
        };

        spi.DataPoints.Add(oldPoint);
        spi.DataPoints.Add(recentPoint);

        spiService
            .Setup(x => x.GetAllSafetyPerformanceIndicatorsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Result<IEnumerable<SafetyPerformanceIndicator>>.Success(new List<SafetyPerformanceIndicator> { spi }.AsEnumerable())));

        spiService
            .Setup(x => x.DeleteSPIDataPointAsync(oldPoint.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SafetyPerformanceIndicator>.Success(spi));

        eventBus
            .Setup(x => x.PublishUIEventAsync(It.IsAny<SPIDashboardRefreshEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var handler = new ArchiveOldSPIDataCommandHandler(spiService.Object, eventBus.Object, logger.Object);
        var command = new ArchiveOldSPIDataCommand(new DateTime(2025, 12, 31), "tester", new List<string> { "SPI-ARCHIVE" });

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
        spiService.Verify(x => x.DeleteSPIDataPointAsync(oldPoint.Code, It.IsAny<CancellationToken>()), Times.Once);
        spiService.Verify(x => x.DeleteSPIDataPointAsync(recentPoint.Code, It.IsAny<CancellationToken>()), Times.Never);
        eventBus.Verify(x => x.PublishUIEventAsync(It.IsAny<SPIDashboardRefreshEvent>(), EventExecutionMode.Manual, It.IsAny<CancellationToken>()), Times.Once);
    }
}
