using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class InvestigationService
{
    private readonly InvestigationDataService _dataService;
    private readonly ILogger<InvestigationService> _logger;

    public InvestigationService(InvestigationDataService dataService, ILogger<InvestigationService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating investigation with code: {Code}", investigation?.Code);
            var result = await _dataService.CreateInvestigationAsync(investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created investigation with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create investigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating investigation");
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.CreateFailed);
        }
    }

    public async Task<Result<Investigation>> GetInvestigationByCodeAsync(InvestigationID code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving investigation with Code: {Code}", code.Value);
            return await _dataService.GetInvestigationByCodeAsync(code.Value, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving investigation with Code: {Code}", code.Value);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
        }
    }

    public async Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all investigations");
            return await _dataService.GetAllInvestigationsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all investigations");
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NullOrEmpty);
        }
    }

    public async Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating investigation with ID: {Id}", investigation?.Id);
            var result = await _dataService.UpdateInvestigationAsync(investigation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated investigation with ID: {Id}", investigation?.Id);
            }
            else
            {
                _logger.LogError("Failed to update investigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating investigation with ID: {Id}", investigation?.Id);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteInvestigationAsync(InvestigationID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting investigation with ID: {Id}", id);
            var result = await _dataService.DeleteInvestigationAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted investigation with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete investigation. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting investigation with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.InvestigationError.DeleteFailed);
        }
    }
}