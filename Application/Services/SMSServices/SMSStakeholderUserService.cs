//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Stakeholder User service handling user lifecycle and authentication operations.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using static SMS_Domain.Errors.DomainErrors;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for SMS Stakeholder User business operations
/// Provides business logic orchestration and cross-cutting concerns
/// </summary>
public sealed class SMSStakeholderUserService : ISMSStakeholderUserService
{
    private readonly SMSStakeholderUserDataService _dataService;
    private readonly ILogger<SMSStakeholderUserService> _logger;

    public SMSStakeholderUserService(SMSStakeholderUserDataService dataService, ILogger<SMSStakeholderUserService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Stakeholder User with business validation
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> CreateSMSStakeholderUserAsync(SMSStakeholderUser user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Creating SMS Stakeholder User with code: {Code}", user?.Code);

            if (user is null)
            {
                _logger.LogApplicationError("CreateSMSStakeholderUserAsync received null user");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            if (!SMS_Domain.ValueObjects.UserName.IsValidStakeholderUserNameFormat(user.UserName?.Value ?? string.Empty))
            {
                _logger.LogApplicationWarning("CreateSMSStakeholderUserAsync rejected invalid stakeholder username format: {UserName}", user.UserName?.Value);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.UserNameError.InvalidFormat);
            }

            // Business validation - ensure user is active by default
            if (!user.IsActive)
            {
                _logger.LogApplicationInformation("Activating user during creation: {Code}", user.Code);
                user.Activate();
            }

            
            
            var result = await _dataService.AddAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully created SMS Stakeholder User with ID: {Id}", result.Value?.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to create SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error creating SMS Stakeholder User");
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder User by ID
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> GetSMSStakeholderUserByCodeAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving SMS Stakeholder User with ID: {Id}", id);
            return await _dataService.GetByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving SMS Stakeholder User with ID: {Id}", id);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Stakeholder Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllSMSStakeholderUsersAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving all SMS Stakeholder Users");
            return await _dataService.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving all SMS Stakeholder Users");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

   

    /// <summary>
    /// Updates an existing SMS Stakeholder User with business validation
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> UpdateSMSStakeholderUserAsync(SMSStakeholderUser user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Updating SMS Stakeholder User with ID: {Id}", user?.UserId);

            if (user is null)
            {
                _logger.LogApplicationError("UpdateSMSStakeholderUserAsync received null user");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            if (!SMS_Domain.ValueObjects.UserName.IsValidStakeholderUserNameFormat(user.UserName?.Value ?? string.Empty))
            {
                _logger.LogApplicationWarning("UpdateSMSStakeholderUserAsync rejected invalid stakeholder username format: {UserName}", user.UserName?.Value);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.UserNameError.InvalidFormat);
            }

            // Business validation - check if user exists
            var existingUserResult = await _dataService.GetByCodeAsync(user.Code, ct).ConfigureAwait(false);
            if (existingUserResult.IsFailure)
            {
                _logger.LogApplicationWarning("Cannot update non-existent SMS Stakeholder User with ID: {Id}", user.UserId);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }

            var existingUser = existingUserResult.Value;

            var result = await _dataService.UpdateAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogApplicationInformation("Successfully updated SMS Stakeholder User with ID: {Id}", user.UserId);
            }
            else
            {
                _logger.LogApplicationError("Failed to update SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error updating SMS Stakeholder User with ID: {Id}", user?.UserId);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Authenticates an SMS Stakeholder User with comprehensive business logic
    /// </summary>
    public async Task<Result<bool>> AuthenticateSMSStakeholderUserAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Authenticating SMS Stakeholder User: {UserName}", userName);

            // Business validation
            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogApplicationWarning("Authentication failed - empty username");
                return Result<bool>.Failure<bool>(DomainErrors.UserNameError.NullOrEmpty);
            }

            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                _logger.LogApplicationWarning("Authentication failed - empty password for user: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.PasswordError.NullOrEmpty);
            }

            var result = await _dataService.AuthenticateUserAsync(userName, plainTextPassword, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var user = result.Value;
            }
            else
            {
                _logger.LogApplicationWarning("Authentication failed for stakeholder user: {UserName}", userName);
            }

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error during authentication for user: {UserName}", userName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.LoginFailed);
        }
    }

    /// <summary>
    /// Gets stakeholder statistics with business insights
    /// </summary>
    public async Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Retrieving stakeholder type statistics");

            var result = await _dataService.GetStakeholderTypeStatisticsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var stats = result.Value;
                _logger.LogApplicationInformation("Retrieved statistics for {Count} stakeholder types", stats.Count);

                // Business analysis - identify dominant stakeholder types
                var totalStakeholders = stats.Values.Sum();
                foreach (var stat in stats.OrderByDescending(s => s.Value))
                {
                    var percentage = (double)stat.Value / totalStakeholders * 100;
                    _logger.LogApplicationInformation("Stakeholder type {Type}: {Count} users ({Percentage:F1}%)",
                        stat.Key, stat.Value, percentage);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Unexpected error retrieving stakeholder type statistics");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(GeneralError.UnProcessableRequest);
        }
    }

    public Task<Result<bool>> AuthenticateAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        return AuthenticateSMSStakeholderUserAsync(userName, plainTextPassword, ct);
    }

    public Task<Result<SMSStakeholderUser>> CreateAsync(SMSStakeholderUser user, CancellationToken ct = default)
    {
        return CreateSMSStakeholderUserAsync(user, ct);
    }

    public Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllAsync(CancellationToken ct = default)
    {
        return GetAllSMSStakeholderUsersAsync(ct);
    }

    public Task<Result<SMSStakeholderUser>> GetByCodeAsync(string id, CancellationToken ct = default)
    {
        return GetSMSStakeholderUserByCodeAsync(id, ct);
    }

    public Task<Result<SMSStakeholderUser>> UpdateAsync(SMSStakeholderUser user, CancellationToken ct = default)
    {
        return UpdateSMSStakeholderUserAsync(user, ct);
    }

    
}

