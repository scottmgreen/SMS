//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS Organizational Group operations using CQRS pattern with MediatR.
//                  Application layer query handlers providing clean separation
//                  between presentation and business logic with validation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

/// <summary>
/// Query handler for getting all SMS organizational groups
/// </summary>
public class GetAllSMSOrganizationalGroupsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSOrganizationalGroupsQuery, Result<IEnumerable<SMSOrganizationalGroup>>>
{
    private readonly SMSOrganizationalGroupService _organizationalGroupDataService;
    private readonly ILogger<GetAllSMSOrganizationalGroupsQueryHandler> _logger;

    public GetAllSMSOrganizationalGroupsQueryHandler(
        SMSOrganizationalGroupService organizationalGroupDataService,
        ILogger<GetAllSMSOrganizationalGroupsQueryHandler> logger)
    {
        _organizationalGroupDataService = organizationalGroupDataService ?? throw new ArgumentNullException(nameof(organizationalGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> HandleAsync(GetAllSMSOrganizationalGroupsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSOrganizationalGroupsQuery");

            var result = await _organizationalGroupDataService.GetAllSMSOrganizationalGroupsAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS organizational groups", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS organizational groups: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllSMSOrganizationalGroupsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSOrganizationalGroupsQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting an SMS organizational group by code
/// </summary>
public class GetSMSOrganizationalGroupByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalGroupByCodeQuery, Result<SMSOrganizationalGroup>>
{
    private readonly SMSOrganizationalGroupDataService _organizationalGroupDataService;
    private readonly ILogger<GetSMSOrganizationalGroupByCodeQueryHandler> _logger;

    public GetSMSOrganizationalGroupByCodeQueryHandler(
        SMSOrganizationalGroupDataService organizationalGroupDataService,
        ILogger<GetSMSOrganizationalGroupByCodeQueryHandler> logger)
    {
        _organizationalGroupDataService = organizationalGroupDataService ?? throw new ArgumentNullException(nameof(organizationalGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSOrganizationalGroup>> HandleAsync(GetSMSOrganizationalGroupByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalGroupByCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _organizationalGroupDataService.GetByCodeAsync(request.GroupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved SMS organizational group: {GroupCode}", request.GroupCode);
            }
            else
            {
                _logger.LogWarning("SMS organizational group not found: {GroupCode}", request.GroupCode);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSOrganizationalGroupByCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalGroupByCodeQuery for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting SMS organizational groups by user code
/// </summary>
public class GetSMSOrganizationalGroupsByUserCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSOrganizationalGroupsByUserCodeQuery, Result<IEnumerable<SMSOrganizationalGroup>>>
{
    private readonly SMSOrganizationalGroupDataService _organizationalGroupDataService;
    private readonly ILogger<GetSMSOrganizationalGroupsByUserCodeQueryHandler> _logger;

    public GetSMSOrganizationalGroupsByUserCodeQueryHandler(
        SMSOrganizationalGroupDataService organizationalGroupDataService,
        ILogger<GetSMSOrganizationalGroupsByUserCodeQueryHandler> logger)
    {
        _organizationalGroupDataService = organizationalGroupDataService ?? throw new ArgumentNullException(nameof(organizationalGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> HandleAsync(GetSMSOrganizationalGroupsByUserCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSOrganizationalGroupsByUserCodeQuery for user: {UserCode}", request.UserCode);

            var result = await _organizationalGroupDataService.GetGroupsByUserCodeAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} SMS organizational groups for user: {UserCode}",
                    result.Value?.Count() ?? 0, request.UserCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve SMS organizational groups for user {UserCode}: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetSMSOrganizationalGroupsByUserCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSOrganizationalGroupsByUserCodeQuery for user: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

/// <summary>
/// Query handler for getting users by organizational group code
/// </summary>
public class GetUsersByOrganizationalGroupCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetUsersByOrganizationalGroupCodeQuery, Result<IEnumerable<SMSOrganizationalUser>>>
{
    private readonly SMSOrganizationalGroupDataService _organizationalGroupDataService;
    private readonly ILogger<GetUsersByOrganizationalGroupCodeQueryHandler> _logger;

    public GetUsersByOrganizationalGroupCodeQueryHandler(
        SMSOrganizationalGroupDataService organizationalGroupDataService,
        ILogger<GetUsersByOrganizationalGroupCodeQueryHandler> logger)
    {
        _organizationalGroupDataService = organizationalGroupDataService ?? throw new ArgumentNullException(nameof(organizationalGroupDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> HandleAsync(GetUsersByOrganizationalGroupCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetUsersByOrganizationalGroupCodeQuery for group: {GroupCode}", request.GroupCode);

            var result = await _organizationalGroupDataService.GetUsersByGroupCodeAsync(request.GroupCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} users for organizational group: {GroupCode}",
                    result.Value?.Count() ?? 0, request.GroupCode);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve users for organizational group {GroupCode}: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetUsersByOrganizationalGroupCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetUsersByOrganizationalGroupCodeQuery for group: {GroupCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
