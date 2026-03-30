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
            _logger.LogInformation("Creating SMS Stakeholder User with code: {Code}", user?.Code);

            if (user is null)
            {
                _logger.LogError("CreateSMSStakeholderUserAsync received null user");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            // Business validation - ensure user is active by default
            if (!user.IsActive)
            {
                _logger.LogInformation("Activating user during creation: {Code}", user.Code);
                user.Activate();
            }

            // Business validation - validate stakeholder type and organization combination
            await ValidateStakeholderTypeOrganizationCombination(user.StakeholderType, user.Organization);

            // Business validation - set appropriate access level based on stakeholder type
            //await ValidateAndAdjustAccessLevel(user);

            var result = await _dataService.AddAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Stakeholder User with ID: {Id}", result.Value?.UserId);
            }
            else
            {
                _logger.LogError("Failed to create SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS Stakeholder User");
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Stakeholder User by ID
    /// </summary>
    public async Task<Result<SMSStakeholderUser>> GetSMSStakeholderUserByIdAsync(string id, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder User with ID: {Id}", id);
            return await _dataService.GetByCodeAsync(id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder User with ID: {Id}", id);
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
            _logger.LogInformation("Retrieving all SMS Stakeholder Users");
            return await _dataService.GetAllAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Stakeholder Users");
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets stakeholders by type with business logic
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetSMSStakeholderUsersByTypeAsync(string stakeholderType, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Stakeholder Users by type: {StakeholderType}", stakeholderType);

            // Business validation - ensure stakeholder type is valid
            if (!IsValidStakeholderType(stakeholderType))
            {
                _logger.LogWarning("Invalid stakeholder type requested: {StakeholderType}", stakeholderType);
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(GeneralError.UnProcessableRequest);
            }

            return await _dataService.GetByStakeholderTypeAsync(stakeholderType, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Stakeholder Users by type: {StakeholderType}", stakeholderType);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    
    /// <summary>
    /// Gets users requiring AOA access with security validation
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersRequiringAOAAccessAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving users requiring AOA access");

            var result = await _dataService.GetAllAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var aoaUsers = result.Value;
                _logger.LogInformation("Found {Count} users requiring AOA access", aoaUsers.Count());

                // Business analysis - security monitoring
                //foreach (var user in aoaUsers.Where(u => u.AccessLevel == "Full"))
                //{
                //    _logger.LogInformation("Full access AOA user: {UserName} from {Organization}",
                //        user.UserName.Value, user.Organization);
                //}

                // Business rule - log if too many users have AOA access
                if (aoaUsers.Count() > 100)
                {
                    _logger.LogWarning("High number of users with AOA access: {Count}. Review access levels.", aoaUsers.Count());
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving users requiring AOA access");
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
            _logger.LogInformation("Updating SMS Stakeholder User with ID: {Id}", user?.UserId);

            if (user is null)
            {
                _logger.LogError("UpdateSMSStakeholderUserAsync received null user");
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            // Business validation - check if user exists
            var existingUserResult = await _dataService.GetByCodeAsync(user.Code, ct).ConfigureAwait(false);
            if (existingUserResult.IsFailure)
            {
                _logger.LogWarning("Cannot update non-existent SMS Stakeholder User with ID: {Id}", user.UserId);
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }

            var existingUser = existingUserResult.Value;

            // Business rule - log access level changes for security
            //if (existingUser.AccessLevel != user.AccessLevel)
            //{
            //    _logger.LogWarning("Access level change for user {UserName}: {OldLevel} -> {NewLevel}",
            //        user.UserName.Value, existingUser.AccessLevel, user.AccessLevel);
            //}

            // Business validation - validate stakeholder type and organization combination
            await ValidateStakeholderTypeOrganizationCombination(user.StakeholderType, user.Organization);

            var result = await _dataService.UpdateAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Stakeholder User with ID: {Id}", user.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Stakeholder User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Stakeholder User with ID: {Id}", user?.UserId);
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
            _logger.LogInformation("Authenticating SMS Stakeholder User: {UserName}", userName);

            // Business validation
            if (string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogWarning("Authentication failed - empty username");
                return Result<bool>.Failure<bool>(DomainErrors.UserNameError.NullOrEmpty);
            }

            if (string.IsNullOrWhiteSpace(plainTextPassword))
            {
                _logger.LogWarning("Authentication failed - empty password for user: {UserName}", userName);
                return Result<bool>.Failure<bool>(DomainErrors.PasswordError.NullOrEmpty);
            }

            var result = await _dataService.AuthenticateUserAsync(userName, plainTextPassword, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var user = result.Value;

                // Business rule - check if user is active
                //if (!user.IsActive)
                //{
                //    _logger.LogWarning("Authentication failed - user is inactive: {UserName}", userName);
                //    return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.BaseUserError.InactiveUser);
                //}

                // Business rule - security logging for external users
                //_logger.LogInformation("External stakeholder authenticated: {UserName} from {Organization} ({StakeholderType})",
                //    userName, user.Organization, user.StakeholderType);

                //// Business rule - additional validation for high-access users
                //if (user.AccessLevel == "Full")
                //{
                //    _logger.LogInformation("High-privilege stakeholder login: {UserName} with Full access", userName);
                //}
            }
            else
            {
                _logger.LogWarning("Authentication failed for stakeholder user: {UserName}", userName);
            }

            return result.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user: {UserName}", userName);
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
            _logger.LogInformation("Retrieving stakeholder type statistics");

            var result = await _dataService.GetStakeholderTypeStatisticsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var stats = result.Value;
                _logger.LogInformation("Retrieved statistics for {Count} stakeholder types", stats.Count);

                // Business analysis - identify dominant stakeholder types
                var totalStakeholders = stats.Values.Sum();
                foreach (var stat in stats.OrderByDescending(s => s.Value))
                {
                    var percentage = (double)stat.Value / totalStakeholders * 100;
                    _logger.LogInformation("Stakeholder type {Type}: {Count} users ({Percentage:F1}%)",
                        stat.Key, stat.Value, percentage);
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving stakeholder type statistics");
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(GeneralError.UnProcessableRequest);
        }
    }

    #region Private Business Logic Helpers

    /// <summary>
    /// Validates if stakeholder type is valid (business rule)
    /// </summary>
    private bool IsValidStakeholderType(string stakeholderType)
    {
        if (string.IsNullOrWhiteSpace(stakeholderType))
            return false;

        var validTypes = new[]
        {
            "Airline", "Ground Handler", "Contractor", "Tenant", "Government Agency",
            "Service Provider", "Vendor", "Emergency Services", "Regulatory Body"
        };

        return validTypes.Contains(stakeholderType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates stakeholder type and organization combination (business rule)
    /// </summary>
    private async Task ValidateStakeholderTypeOrganizationCombination(string stakeholderType, string organization)
    {
        // Business rule - certain organizations should only have specific stakeholder types
        var knownAirlines = new[] { "Delta", "American", "United", "Southwest", "Alaska", "JetBlue" };
        var knownGroundHandlers = new[] { "Swissport", "Menzies", "DNata", "Signature Flight Support" };

        if (knownAirlines.Any(airline => organization.Contains(airline, StringComparison.OrdinalIgnoreCase)))
        {
            if (!stakeholderType.Equals("Airline", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Organization {Organization} appears to be an airline but stakeholder type is {Type}",
                    organization, stakeholderType);
            }
        }

        if (knownGroundHandlers.Any(handler => organization.Contains(handler, StringComparison.OrdinalIgnoreCase)))
        {
            if (!stakeholderType.Equals("Ground Handler", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Organization {Organization} appears to be a ground handler but stakeholder type is {Type}",
                    organization, stakeholderType);
            }
        }

        await Task.CompletedTask; // Placeholder for potential async validation
    }

    /// <summary>
    /// Validates and adjusts access level based on stakeholder type (business rule)
    /// </summary>
    //private async Task ValidateAndAdjustAccessLevel(SMSStakeholderUser user)
    //{
    //    // Business rule - government agencies typically get extended access
    //    if (user.StakeholderType.Contains("Government", StringComparison.OrdinalIgnoreCase) ||
    //        user.StakeholderType.Contains("Regulatory", StringComparison.OrdinalIgnoreCase))
    //    {
    //        if (user.AccessLevel == "Limited")
    //        {
    //            _logger.LogInformation("Upgrading access level for government/regulatory user: {UserName}", user.UserName.Value);
    //            user.UpdateStakeholderInfo(user.StakeholderType, user.Organization, "Extended");
    //        }
    //    }

    //    // Business rule - vendors typically have limited access
    //    if (user.StakeholderType.Equals("Vendor", StringComparison.OrdinalIgnoreCase) ||
    //        user.StakeholderType.Equals("Service Provider", StringComparison.OrdinalIgnoreCase))
    //    {
    //        if (user.AccessLevel == "Full")
    //        {
    //            _logger.LogWarning("Full access granted to vendor/service provider: {UserName} - requires approval", user.UserName.Value);
    //        }
    //    }

    //    await Task.CompletedTask; // Placeholder for potential async validation
    //}

    #endregion
}
