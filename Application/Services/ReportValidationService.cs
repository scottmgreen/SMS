using Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class ReportValidationService : IReportValidationService
{
    private readonly ReportValidationDataService _dataService;
    private readonly ILogger<ReportValidationService> _logger;

    public ReportValidationService(ReportValidationDataService dataService, ILogger<ReportValidationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating report validation with code: {Code}", reportValidation?.Code);
            var result = await _dataService.CreateReportValidationAsync(reportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created report validation with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create report validation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating report validation");
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.CreateFailed);
        }
    }

    public async Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving report validation with ID: {Id}", id);
            return await _dataService.GetReportValidationByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving report validation with ID: {Id}", id);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
        }
    }

    public async Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all report validations");
            return await _dataService.GetAllReportValidationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all report validations");
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }

    public async Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating report validation with ID: {Id}", reportValidation?.Id);
            var result = await _dataService.UpdateReportValidationAsync(reportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated report validation with ID: {Id}", reportValidation?.Id);
            }
            else
            {
                _logger.LogError("Failed to update report validation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating report validation with ID: {Id}", reportValidation?.Id);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting report validation with ID: {Id}", id);
            var result = await _dataService.DeleteReportValidationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted report validation with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete report validation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting report validation with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}