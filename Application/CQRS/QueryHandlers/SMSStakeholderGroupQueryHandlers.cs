//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS Stakeholder Group operations using CQRS pattern with MediatR.
//                  Application layer query handlers providing clean separation
//                  between presentation and business logic with validation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Queries;

namespace SMS_Application.QueryHandlers;

/// <summary>
/// Query handler for getting all SMS stakeholder groups
/// </summary>
public class GetAllSMSStakeholderGroupsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSStakeholderGroupsQuery, Result<IEnumerable<SMSStakeholderGroup>>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<GetAllSMSStakeholderGroupsQueryHandler> _logger;

    public GetAllSMSStakeholderGroupsQueryHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<GetAllSMSStakeholderGroupsQueryHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> HandleAsync(GetAllSMSStakeholderGroupsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllSMSStakeholderGroupsQuery");

            var result = await _stakeholderGroupService.GetAllSMSStakeholderGroupsAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} SMS stakeholder groups", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS stakeholder groups: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetAllSMSStakeholderGroupsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSStakeholderGroupsQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting an SMS stakeholder group by code
/// </summary>
public class GetSMSStakeholderGroupByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSStakeholderGroupByCodeQuery, Result<SMSStakeholderGroup>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<GetSMSStakeholderGroupByCodeQueryHandler> _logger;

    public GetSMSStakeholderGroupByCodeQueryHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<GetSMSStakeholderGroupByCodeQueryHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderGroup>> HandleAsync(GetSMSStakeholderGroupByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSStakeholderGroupByCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _stakeholderGroupService.GetSMSStakeholderGroupByCodeAsync(request.GroupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved SMS stakeholder group: {GroupCode}", request.GroupCode);
            }
            else
            {
                _logger.LogApplicationWarning("SMS stakeholder group not found: {GroupCode}", request.GroupCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetSMSStakeholderGroupByCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSStakeholderGroupByCodeQuery for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting SMS stakeholder groups by user code
/// </summary>
public class GetSMSStakeholderGroupsByUserCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSStakeholderGroupsByUserCodeQuery, Result<IEnumerable<SMSStakeholderGroup>>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<GetSMSStakeholderGroupsByUserCodeQueryHandler> _logger;

    public GetSMSStakeholderGroupsByUserCodeQueryHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<GetSMSStakeholderGroupsByUserCodeQueryHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> HandleAsync(GetSMSStakeholderGroupsByUserCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSStakeholderGroupsByUserCodeQuery for user: {UserCode}", request.UserCode);

            var result = await _stakeholderGroupService.GetSMSStakeholderGroupsByUserCodeAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} SMS stakeholder groups for user: {UserCode}",
                    result.Value?.Count() ?? 0, request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS stakeholder groups for user {UserCode}: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetSMSStakeholderGroupsByUserCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSStakeholderGroupsByUserCodeQuery for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting users by stakeholder group code
/// </summary>
public class GetUsersByStakeholderGroupCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetUsersByStakeholderGroupCodeQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly SMSStakeholderGroupService _stakeholderGroupService;
    private readonly ILogger<GetUsersByStakeholderGroupCodeQueryHandler> _logger;

    public GetUsersByStakeholderGroupCodeQueryHandler(
        SMSStakeholderGroupService stakeholderGroupService,
        ILogger<GetUsersByStakeholderGroupCodeQueryHandler> logger)
    {
        _stakeholderGroupService = stakeholderGroupService ?? throw new ArgumentNullException(nameof(stakeholderGroupService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetUsersByStakeholderGroupCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetUsersByStakeholderGroupCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _stakeholderGroupService.GetUsersByGroupCodeAsync(request.GroupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} users for stakeholder group: {GroupCode}",
                    result.Value?.Count() ?? 0, request.GroupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve users for stakeholder group {GroupCode}: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetUsersByStakeholderGroupCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetUsersByStakeholderGroupCodeQuery for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
