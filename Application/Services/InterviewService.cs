using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

public sealed class InterviewService
{
    private readonly InterviewDataService _dataService;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(InterviewDataService dataService, ILogger<InterviewService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating interview with code: {Code}", interview?.Code);
            var result = await _dataService.CreateInterviewAsync(interview, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created interview with ID: {Id}", result.Value?.Id);
            }
            else
            {
                _logger.LogError("Failed to create interview. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating interview");
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.CreateFailed);
        }
    }

    public async Task<Result<Interview>> GetInterviewByIdAsync(InterviewID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving interview with ID: {Id}", id);
            return await _dataService.GetInterviewByIdAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving interview with ID: {Id}", id);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.NotFound);
        }
    }

    public async Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all interviews");
            return await _dataService.GetAllInterviewsAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all interviews");
            return Result<List<Interview>>.Failure<List<Interview>>(DomainErrors.InterviewError.NullOrEmpty);
        }
    }

    public async Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating interview with ID: {Id}", interview?.Id);
            var result = await _dataService.UpdateInterviewAsync(interview, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated interview with ID: {Id}", interview?.Id);
            }
            else
            {
                _logger.LogError("Failed to update interview. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating interview with ID: {Id}", interview?.Id);
            return Result<Interview>.Failure<Interview>(DomainErrors.InterviewError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteInterviewAsync(InterviewID id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deleting interview with ID: {Id}", id);
            var result = await _dataService.DeleteInterviewAsync(id, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted interview with ID: {Id}", id);
            }
            else
            {
                _logger.LogError("Failed to delete interview. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting interview with ID: {Id}", id);
            return Result<bool>.Failure<bool>(DomainErrors.InterviewError.DeleteFailed);
        }
    }
}