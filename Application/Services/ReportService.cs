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
}