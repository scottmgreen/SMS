using Microsoft.Extensions.Logging;
using SMS_Application.Queries;
using SMS_Domain.Enums;

namespace SMS_Application.QueryHandlers;

public class GetAllSMSJobTitlesQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSJobTitlesQuery, Result<IEnumerable<SMSJobTitle>>>
{
    private readonly SMSJobTitleService _jobTitleService;
    private readonly ILogger<GetAllSMSJobTitlesQueryHandler> _logger;

    public GetAllSMSJobTitlesQueryHandler(
        SMSJobTitleService jobTitleService,
        ILogger<GetAllSMSJobTitlesQueryHandler> logger)
    {
        _jobTitleService = jobTitleService ?? throw new ArgumentNullException(nameof(jobTitleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSJobTitle>>> HandleAsync(GetAllSMSJobTitlesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllSMSJobTitlesQuery", ApplicationEventIds.Information);
            return await _jobTitleService.GetAllSMSJobTitlesAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetAllSMSJobTitlesQuery operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing GetAllSMSJobTitlesQuery", ApplicationEventIds.Error);
            return Result<IEnumerable<SMSJobTitle>>.Failure<IEnumerable<SMSJobTitle>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetSMSJobTitleByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSJobTitleByCodeQuery, Result<SMSJobTitle>>
{
    private readonly SMSJobTitleService _jobTitleService;
    private readonly ILogger<GetSMSJobTitleByCodeQueryHandler> _logger;

    public GetSMSJobTitleByCodeQueryHandler(
        SMSJobTitleService jobTitleService,
        ILogger<GetSMSJobTitleByCodeQueryHandler> logger)
    {
        _jobTitleService = jobTitleService ?? throw new ArgumentNullException(nameof(jobTitleService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSJobTitle>> HandleAsync(GetSMSJobTitleByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSJobTitleByCodeQuery for title: {Code}", ApplicationEventIds.Information, request.JobTitleCode);
            return await _jobTitleService.GetSMSJobTitleByCodeAsync(request.JobTitleCode, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetSMSJobTitleByCodeQuery operation was cancelled", ApplicationEventIds.Warning);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error processing GetSMSJobTitleByCodeQuery", ApplicationEventIds.Error);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
