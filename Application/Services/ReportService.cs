//-----------------------------------------------------------------------
// <copyright file="ReportService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Report management service handling report creation, validation, and processing.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class ReportService : IReportService
{
    private readonly ReportDataService _dataService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ReportDataService dataService, ILogger<ReportService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating report with code: {Code}", report?.Code);
            var result = await _dataService.CreateReportAsync(report, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created report with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create report. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating report");
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.CreateFailed);
        }
    }

    public async Task<Result<Report>> GetReportByIdAsync(ReportID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving report with ID: {Id}", id);
            return await _dataService.GetReportByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving report with ID: {Id}", id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<Report>> GetReportByCodeAsync(ReportID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving report with Code: {Id}", code);
            return await _dataService.GetReportByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving report with Code: {Id}", code);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all reports");
            return await _dataService.GetAllReportsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all reports");
            return Result<List<Report>>.Failure<List<Report>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }

    public async Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating report with ID: {Id}", report?.Id);
            var result = await _dataService.UpdateReportAsync(report, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated report with ID: {Id}", report?.Id);
            }
            else
            {
                _logger.LogError("Failed to update report. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating report with ID: {Id}", report?.Id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting report with ID: {Id}", id);
            var result = await _dataService.DeleteReportAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted report with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete report. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting report with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> UpdateReportStatusAsync(string reportCode, ReportStatus status, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating report status for Code: {Code} to {Status}", reportCode, status);
            
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
                _logger.LogInformation("Successfully updated report status for Code: {Code}", reportCode);
                return Result<bool>.Success(true);
            }
            else
            {
                _logger.LogError("Failed to update report status. Error: {Error}", updateResult.Error?.Message);
                return Result<bool>.Failure<bool>(updateResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating report status for Code: {Code}", reportCode);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<Report>> SubmitReportAsync(ReportID id, string submittedBy, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Submitting report with ID: {Id}", id);
            
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
                _logger.LogInformation("Successfully submitted report with ID: {Id}", id);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error submitting report with ID: {Id}", id);
            return Result<Report>.Failure<Report>(DomainErrors.ReportError.UpdateFailed);
        }
    }
}
