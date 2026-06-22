//-----------------------------------------------------------------------
// <copyright file="ReportService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Report management service handling report creation, validation, and processing.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Events;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class ReportService : IReportService
{
    private readonly ReportDataService _dataService;
    private readonly IBaseEventBus _eventBus;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        ReportDataService dataService,
        IBaseEventBus eventBus,
        ILogger<ReportService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating report with code: {Code}", report?.Code);
            var result = await _dataService.CreateReportAsync(report, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created report with ID: {Id}", result.Value?.Id);

                if (result.Value is not null)
                {
                    var reportCreatedEvent = new ReportCreatedEvent(
                        id: new SMSEventID(Guid.NewGuid().ToString()),
                        reportId: result.Value.Code,
                        createdBy: result.Value.CreatedBy ?? "SYSTEM",
                        createdDate: result.Value.CreatedDate ?? DateTime.UtcNow);

                    var eventResult = await _eventBus.PublishDomainEventAsync(reportCreatedEvent, EventExecutionMode.Immediate, ct).ConfigureAwait(false);
                    if (eventResult.IsFailure)
                    {
                        _logger.LogApplicationWarning("Failed to publish report created event for report {ReportCode}: {Error}", result.Value.Code, eventResult.Error?.Message);
                    }
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to create report. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating report");
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.CreateFailed);
        }
    }

    public async Task<Result<Report>> GetReportByIdAsync(ReportID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving report with ID: {Id}", id);
            return await _dataService.GetReportByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving report with ID: {Id}", id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<Report>> GetReportByCodeAsync(ReportID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving report with Code: {Id}", code);
            return await _dataService.GetReportByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving report with Code: {Id}", code);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all reports");
            return await _dataService.GetAllReportsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all reports");
            return Result<List<Report>>.Failure<List<Report>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }

    public async Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default)
    {
        try
        {
            ReportStatus? previousStatus = null;
            string? previousStage = null;
            if (report is not null && !string.IsNullOrWhiteSpace(report.Code))
            {
                var existingResult = await _dataService.GetReportByCodeAsync(new ReportID(report.Code), ct).ConfigureAwait(false);
                if (existingResult.IsSuccess && existingResult.Value is not null)
                {
                    if (ReportStatus.TryFromValue(existingResult.Value.Status, out var existingStatus))
                    {
                        previousStatus = existingStatus;
                    }

                    previousStage = existingResult.Value.Stage;
                }
            }

            _logger.LogApplicationInformation("Updating report with ID: {Id}", report?.Id);
            var result = await _dataService.UpdateReportAsync(report, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated report with ID: {Id}", report?.Id);

                if (report is not null)
                {
                    if (ReportStatus.TryFromValue(report.Status, out var currentStatus))
                    {
                        await TransitionEventPublisher.PublishIfChangedAsync(
                            _eventBus,
                            previousStatus,
                            currentStatus,
                            () => new ReportStatusChangedEvent(
                                id: new SMSEventID(Guid.NewGuid().ToString()),
                                reportId: report.Id.Value,
                                reportCode: report.Code,
                                previousStatus: previousStatus!,
                                newStatus: currentStatus!,
                                changedBy: report.UpdatedBy ?? "SYSTEM",
                                changedDate: report.UpdatedDate ?? DateTime.UtcNow),
                            ct).ConfigureAwait(false);
                    }

                    await TransitionEventPublisher.PublishIfChangedAsync(
                        _eventBus,
                        previousStage,
                        report.Stage,
                        () => new ReportStageChangedEvent(
                            id: new SMSEventID(Guid.NewGuid().ToString()),
                            reportId: report.Id.Value,
                            reportCode: report.Code,
                            previousStage: previousStage ?? string.Empty,
                            newStage: report.Stage ?? string.Empty,
                            changedBy: report.UpdatedBy ?? "SYSTEM",
                            changedDate: report.UpdatedDate ?? DateTime.UtcNow),
                        ct).ConfigureAwait(false);
                }
            }
            else
            {
                _logger.LogApplicationError("Failed to update report. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating report with ID: {Id}", report?.Id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Deleting report with ID: {Id}", id);
            var result = await _dataService.DeleteReportAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully deleted report with ID: {Id}", id);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete report. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error deleting report with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> UpdateReportStatusAsync(string reportCode, ReportStatus status, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating report status for Code: {Code} to {Status}", reportCode, status);
            
            // Get the current report
            var reportResult = await _dataService.GetReportByCodeAsync(new ReportID(reportCode), ct);
            if (reportResult.IsFailure || reportResult.Value == null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NotFound);
            }

            var report = reportResult.Value;
            report.Status = status;
            report.UpdatedBy = updatedBy;
            report.UpdatedDate = DateTime.UtcNow;

            var updateResult = await _dataService.UpdateReportAsync(report, ct);
            
            if (updateResult.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated report status for Code: {Code}", reportCode);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogApplicationError("Failed to update report status. Error: {Error}", updateResult.Error?.Message);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating report status for Code: {Code}", reportCode);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<Report>> SubmitReportAsync(ReportID id, string submittedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Submitting report with ID: {Id}", id);
            
            var reportResult = await _dataService.GetReportByCodeAsync(id, ct);
            if (reportResult.IsFailure || reportResult.Value == null)
            {
                return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
            }

            var report = reportResult.Value;
            report.Status = ReportStatus.Created;
            report.UpdatedBy = submittedBy;
            report.UpdatedDate = DateTime.UtcNow;

            var result = await _dataService.UpdateReportAsync(report, ct);
            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully submitted report with ID: {Id}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error submitting report with ID: {Id}", id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }
}

