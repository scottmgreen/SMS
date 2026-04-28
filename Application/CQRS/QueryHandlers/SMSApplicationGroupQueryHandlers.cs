//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroupQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS read operations.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

/// <summary>
/// Query handler for getting all SMS application groups
/// </summary>
public class GetAllSMSApplicationGroupsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSApplicationGroupsQuery, Result<IEnumerable<SMSApplicationGroup>>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<GetAllSMSApplicationGroupsQueryHandler> _logger;

    public GetAllSMSApplicationGroupsQueryHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<GetAllSMSApplicationGroupsQueryHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationGroup>>> HandleAsync(GetAllSMSApplicationGroupsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSApplicationGroupsQuery");

            var result = await _applicationGroupService.GetAllSMSApplicationGroupsAsync();

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS application groups", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS application groups: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllSMSApplicationGroupsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSApplicationGroupsQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting an SMS application group by code
/// </summary>
public class GetSMSApplicationGroupByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationGroupByCodeQuery, Result<SMSApplicationGroup>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<GetSMSApplicationGroupByCodeQueryHandler> _logger;

    public GetSMSApplicationGroupByCodeQueryHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<GetSMSApplicationGroupByCodeQueryHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSApplicationGroup>> HandleAsync(GetSMSApplicationGroupByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationGroupByCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _applicationGroupService.GetSMSApplicationGroupByCodeAsync(request.GroupCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS application group: {GroupCode}", request.GroupCode);
            }
            else
            {
                _logger.LogWarning("SMS application group not found: {GroupCode}", request.GroupCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSApplicationGroupByCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSApplicationGroupByCodeQuery for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting SMS application groups by user code
/// </summary>
public class GetSMSApplicationGroupsByUserCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSApplicationGroupsByUserCodeQuery, Result<IEnumerable<SMSApplicationGroup>>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<GetSMSApplicationGroupsByUserCodeQueryHandler> _logger;

    public GetSMSApplicationGroupsByUserCodeQueryHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<GetSMSApplicationGroupsByUserCodeQueryHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationGroup>>> HandleAsync(GetSMSApplicationGroupsByUserCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSApplicationGroupsByUserCodeQuery for user: {UserCode}", request.UserCode);

            var result = await _applicationGroupService.GetSMSApplicationGroupsByUserCodeAsync(request.UserCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS application groups for user: {UserCode}",
                    result.Value?.Count() ?? 0, request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS application groups for user {UserCode}: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSApplicationGroupsByUserCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSApplicationGroupsByUserCodeQuery for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting users by application group code
/// </summary>
public class GetUsersByApplicationGroupCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetUsersByApplicationGroupCodeQuery, Result<IEnumerable<SMSApplicationUser>>>
{
    private readonly SMSApplicationGroupService _applicationGroupService;
    private readonly ILogger<GetUsersByApplicationGroupCodeQueryHandler> _logger;

    public GetUsersByApplicationGroupCodeQueryHandler(
        SMSApplicationGroupService applicationGroupService,
        ILogger<GetUsersByApplicationGroupCodeQueryHandler> logger)
    {
        _applicationGroupService = applicationGroupService ?? throw new ArgumentNullException(nameof(applicationGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> HandleAsync(GetUsersByApplicationGroupCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetUsersByApplicationGroupCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _applicationGroupService.GetUsersByGroupCodeAsync(request.GroupCode);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} users for application group: {GroupCode}",
                    result.Value?.Count() ?? 0, request.GroupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve users for application group {GroupCode}: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetUsersByApplicationGroupCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetUsersByApplicationGroupCodeQuery for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
