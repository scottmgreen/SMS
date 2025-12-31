using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Messaging.QueryHandlers;

/// <summary>
/// Query handler for getting all SMS stakeholder groups
/// </summary>
public class GetAllSMSStakeholderGroupsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSStakeholderGroupsQuery, Result<IEnumerable<SMSStakeholderGroup>>>
{
    private readonly SMSStakeholderGroupDataService _dataService;
    private readonly ILogger<GetAllSMSStakeholderGroupsQueryHandler> _logger;

    public GetAllSMSStakeholderGroupsQueryHandler(
        SMSStakeholderGroupDataService dataService,
        ILogger<GetAllSMSStakeholderGroupsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> HandleAsync(GetAllSMSStakeholderGroupsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSStakeholderGroupsQuery");

            var result = await _dataService.GetAllAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS stakeholder groups", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogError("Failed to retrieve SMS stakeholder groups: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllSMSStakeholderGroupsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllSMSStakeholderGroupsQuery");
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting an SMS stakeholder group by code
/// </summary>
public class GetSMSStakeholderGroupByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderGroupByCodeQuery, Result<SMSStakeholderGroup>>
{
    private readonly SMSStakeholderGroupDataService _dataService;
    private readonly ILogger<GetSMSStakeholderGroupByCodeQueryHandler> _logger;

    public GetSMSStakeholderGroupByCodeQueryHandler(
        SMSStakeholderGroupDataService dataService,
        ILogger<GetSMSStakeholderGroupByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderGroup>> HandleAsync(GetSMSStakeholderGroupByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderGroupByCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _dataService.GetByCodeAsync(request.GroupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS stakeholder group: {GroupCode}", request.GroupCode);
            }
            else
            {
                _logger.LogWarning("SMS stakeholder group not found: {GroupCode}", request.GroupCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSStakeholderGroupByCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderGroupByCodeQuery for group: {GroupCode}", request.GroupCode);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting SMS stakeholder groups by user code
/// </summary>
public class GetSMSStakeholderGroupsByUserCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSStakeholderGroupsByUserCodeQuery, Result<IEnumerable<SMSStakeholderGroup>>>
{
    private readonly SMSStakeholderGroupDataService _dataService;
    private readonly ILogger<GetSMSStakeholderGroupsByUserCodeQueryHandler> _logger;

    public GetSMSStakeholderGroupsByUserCodeQueryHandler(
        SMSStakeholderGroupDataService dataService,
        ILogger<GetSMSStakeholderGroupsByUserCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> HandleAsync(GetSMSStakeholderGroupsByUserCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSStakeholderGroupsByUserCodeQuery for user: {UserCode}", request.UserCode);

            var result = await _dataService.GetGroupsByUserCodeAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS stakeholder groups for user: {UserCode}", 
                    result.Value?.Count() ?? 0, request.UserCode);
            }
            else
            {
                _logger.LogError("Failed to retrieve SMS stakeholder groups for user {UserCode}: {Error}", 
                    request.UserCode, result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSStakeholderGroupsByUserCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSStakeholderGroupsByUserCodeQuery for user: {UserCode}", request.UserCode);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}