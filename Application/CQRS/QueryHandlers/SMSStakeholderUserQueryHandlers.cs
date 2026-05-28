//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS Stakeholder User operations using CQRS pattern with MediatR.
//                  Application layer query handlers providing clean separation
//                  between presentation and business logic with validation.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;
using SMS_Application.Queries;

namespace SMS_Application.QueryHandlers;

/// <summary>
/// Query handler for getting all SMS stakeholder users
/// </summary>
public class GetAllSMSStakeholderUsersQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSStakeholderUsersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<GetAllSMSStakeholderUsersQueryHandler> _logger;

    public GetAllSMSStakeholderUsersQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<GetAllSMSStakeholderUsersQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetAllSMSStakeholderUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllSMSStakeholderUsersQuery");
            var result = await _stakeholderUserService.GetAllSMSStakeholderUsersAsync(ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved {Count} SMS Stakeholder Users", result.Value?.Count() ?? 0);
            }
            else
            {
                _logger.LogApplicationWarning("Failed to retrieve SMS Stakeholder Users");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSStakeholderUsersQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS stakeholder user by code
/// </summary>
public class GetSMSStakeholderUserByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSStakeholderUserByCodeQuery, Result<SMSStakeholderUser>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<GetSMSStakeholderUserByCodeQueryHandler> _logger;

    public GetSMSStakeholderUserByCodeQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<GetSMSStakeholderUserByCodeQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(GetSMSStakeholderUserByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSStakeholderUserByCodeQuery for Code: {UserCode}", request.UserCode);
            var result = await _stakeholderUserService.GetSMSStakeholderUserByCodeAsync(request.UserCode, ct);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully retrieved SMS Stakeholder User with Code: {UserCode}", request.UserCode);
            }
            else
            {
                _logger.LogApplicationWarning("SMS Stakeholder User not found with Code: {UserCode}", request.UserCode);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSStakeholderUserByCodeQuery for Code: {UserCode}", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS stakeholder user by username
/// </summary>
public class GetSMSStakeholderUserByUserNameQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSStakeholderUserByUserNameQuery, Result<SMSStakeholderUser>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<GetSMSStakeholderUserByUserNameQueryHandler> _logger;

    public GetSMSStakeholderUserByUserNameQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<GetSMSStakeholderUserByUserNameQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSStakeholderUser>> HandleAsync(GetSMSStakeholderUserByUserNameQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSStakeholderUserByUserNameQuery for UserName: {UserName}", request.UserName);

            // Get all users and filter by username (as the service doesn't have a direct method)
            var allUsersResult = await _stakeholderUserService.GetAllSMSStakeholderUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(allUsersResult.Error);
            }

            var user = allUsersResult.Value?.FirstOrDefault(u => u.UserName.Value.Equals(request.UserName, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                _logger.LogApplicationInformation("Successfully retrieved SMS Stakeholder User with UserName: {UserName}", request.UserName);
                return Result<SMSStakeholderUser>.Success(user);
            }
            else
            {
                _logger.LogApplicationWarning("SMS Stakeholder User not found with UserName: {UserName}", request.UserName);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSStakeholderUserByUserNameQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting active SMS stakeholder users
/// </summary>
public class GetActiveSMSStakeholderUsersQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetActiveSMSStakeholderUsersQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<GetActiveSMSStakeholderUsersQueryHandler> _logger;

    public GetActiveSMSStakeholderUsersQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<GetActiveSMSStakeholderUsersQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetActiveSMSStakeholderUsersQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetActiveSMSStakeholderUsersQuery");

            // Get all users and filter for active ones
            var allUsersResult = await _stakeholderUserService.GetAllSMSStakeholderUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(allUsersResult.Error);
            }

            var activeUsers = allUsersResult.Value?.Where(u => u.IsActive) ?? Enumerable.Empty<SMSStakeholderUser>();

            _logger.LogApplicationInformation("Successfully retrieved {Count} active SMS Stakeholder Users", activeUsers.Count());
            return Result<IEnumerable<SMSStakeholderUser>>.Success(activeUsers);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetActiveSMSStakeholderUsersQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS stakeholder users by type
/// </summary>
public class GetSMSStakeholderUsersByTypeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSStakeholderUsersByTypeQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<GetSMSStakeholderUsersByTypeQueryHandler> _logger;

    public GetSMSStakeholderUsersByTypeQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<GetSMSStakeholderUsersByTypeQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByTypeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSStakeholderUsersByTypeQuery for Type: {StakeholderType}", request.StakeholderType);

            var result = await _stakeholderUserService.GetSMSStakeholderUsersByTypeAsync(request.StakeholderType, ct);

            _logger.LogApplicationInformation("Successfully retrieved {Count} SMS Stakeholder Users for Type: {StakeholderType}", result.Value?.Count() ?? 0, request.StakeholderType);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSStakeholderUsersByTypeQuery for Type: {StakeholderType}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for getting SMS stakeholder users by organization
/// </summary>
public class GetSMSStakeholderUsersByOrganizationQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSStakeholderUsersByOrganizationQuery, Result<IEnumerable<SMSStakeholderUser>>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<GetSMSStakeholderUsersByOrganizationQueryHandler> _logger;

    public GetSMSStakeholderUsersByOrganizationQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<GetSMSStakeholderUsersByOrganizationQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> HandleAsync(GetSMSStakeholderUsersByOrganizationQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetSMSStakeholderUsersByOrganizationQuery for Organization: {Organization}", request.Organization);

            // Get all users and filter by organization
            var allUsersResult = await _stakeholderUserService.GetAllSMSStakeholderUsersAsync(ct);
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value?.Where(u => u.Organization.Equals(request.Organization, StringComparison.OrdinalIgnoreCase)) ?? Enumerable.Empty<SMSStakeholderUser>();

            _logger.LogApplicationInformation("Successfully retrieved {Count} SMS Stakeholder Users for Organization: {Organization}", filteredUsers.Count(), request.Organization);
            return Result<IEnumerable<SMSStakeholderUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSStakeholderUsersByOrganizationQuery for Organization: {Organization}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for validating SMS stakeholder user credentials
/// </summary>
public class ValidateSMSStakeholderUsersCredentialsQueryHandler : BaseQueryBundle, IBaseRequestHandler<ValidateSMSStakeholderUserCredentialsQuery, Result<bool>>
{
    private readonly SMSStakeholderUserService _stakeholderUserService;
    private readonly ILogger<ValidateSMSStakeholderUsersCredentialsQueryHandler> _logger;

    public ValidateSMSStakeholderUsersCredentialsQueryHandler(SMSStakeholderUserService stakeholderUserService, ILogger<ValidateSMSStakeholderUsersCredentialsQueryHandler> logger)
    {
        _stakeholderUserService = stakeholderUserService ?? throw new ArgumentNullException(nameof(stakeholderUserService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(ValidateSMSStakeholderUserCredentialsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing ValidateSMSStakeholderUsersCredentialsQuery for UserName: {UserName}", request.UserName);

            var result = await _stakeholderUserService.AuthenticateSMSStakeholderUserAsync(request.UserName, request.Password, ct);

            _logger.LogApplicationInformation("Credential validation for {UserName}: {IsValid}", request.UserName, result.IsSuccess && result.Value);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing ValidateSMSStakeholderUserCredentialsQuery for UserName: {UserName}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
        }
    }
}
