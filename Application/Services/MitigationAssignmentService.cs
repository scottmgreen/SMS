using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class MitigationAssignmentService
{
    private readonly MitigationAssignmentDataService _dataService;
    private readonly ILogger<MitigationAssignmentService> _logger;

    public MitigationAssignmentService(MitigationAssignmentDataService dataService, ILogger<MitigationAssignmentService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> CreateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating mitigation assignment with code: {Code}", mitigationAssignment?.Code);
            var result = await _dataService.CreateMitigationAssignmentAsync(mitigationAssignment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created mitigation assignment with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create mitigation assignment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating mitigation assignment");
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.CreateFailed);
        }
    }

    public async Task<Result<MitigationAssignment>> GetMitigationAssignmentByIdAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving mitigation assignment with ID: {Id}", id);
            return await _dataService.GetMitigationAssignmentByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving mitigation assignment with ID: {Id}", id);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NotFound);
        }
    }

    public async Task<Result<List<MitigationAssignment>>> GetAllMitigationAssignmentsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all mitigation assignments");
            return await _dataService.GetAllMitigationAssignmentsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all mitigation assignments");
            return Result<List<MitigationAssignment>>.Failure<List<MitigationAssignment>>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
        }
    }

    public async Task<Result<MitigationAssignment>> UpdateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating mitigation assignment with ID: {Id}", mitigationAssignment?.Id);
            var result = await _dataService.UpdateMitigationAssignmentAsync(mitigationAssignment, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated mitigation assignment with ID: {Id}", mitigationAssignment?.Id);
            }
            else
            {
                _logger.LogError("Failed to update mitigation assignment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating mitigation assignment with ID: {Id}", mitigationAssignment?.Id);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteMitigationAssignmentAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting mitigation assignment with ID: {Id}", id);
            var result = await _dataService.DeleteMitigationAssignmentAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted mitigation assignment with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete mitigation assignment. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting mitigation assignment with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationAssignmentError.DeleteFailed);
        }
    }
}