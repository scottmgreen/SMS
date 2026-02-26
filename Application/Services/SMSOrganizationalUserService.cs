//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS User management service handling user lifecycle and authentication operations.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// High-level application service for SMS Organizational User business operations
/// Provides business logic orchestration and cross-cutting concerns
/// </summary>
public sealed class SMSOrganizationalUserService : ISMSOrganizationalUserService
{
    private readonly SMSOrganizationalUserDataService _dataService;
    private readonly ILogger<SMSOrganizationalUserService> _logger;

    public SMSOrganizationalUserService(SMSOrganizationalUserDataService dataService, ILogger<SMSOrganizationalUserService> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new SMS Organizational User with business validation
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> CreateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Creating SMS Organizational User with code: {Code}", user?.Code);

            if (user is null)
            {
                _logger.LogError("CreateSMSOrganizationalUserAsync received null user");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            // Business validation - ensure user is active by default
            if (!user.IsActive)
            {
                _logger.LogInformation("Activating user during creation: {Code}", user.Code);
                user.Activate();
            }

            // Business validation - validate department and position combination
            await ValidateDepartmentPositionCombination(user.Department, user.Position);

            var result = await _dataService.CreateSMSOrganizationalUserAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created SMS Organizational User with ID: {Id}", result.Value?.UserId);
            }
            else
            {
                _logger.LogError("Failed to create SMS Organizational User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating SMS Organizational User");
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.CreateFailed);
        }
    }

    /// <summary>
    /// Gets SMS Organizational User by ID
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> GetSMSOrganizationalUserByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Organizational User with Code: {Code}", code);
            return await _dataService.GetSMSOrganizationalUserByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Organizational User with Code: {Code}", code);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets all SMS Organizational Users
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllSMSOrganizationalUsersAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving all SMS Organizational Users");
            return await _dataService.GetAllSMSOrganizationalUsersAsync(ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all SMS Organizational Users");
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets SMS Organizational Users by department with business logic
    /// </summary>
    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetSMSOrganizationalUsersByDepartmentAsync(string department, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Retrieving SMS Organizational Users by department: {Department}", department);

            // Business validation - ensure department is valid
            if (!IsValidDepartment(department))
            {
                _logger.LogWarning("Invalid department requested: {Department}", department);
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.GeneralError.UnProcessableRequest);
            }

            return await _dataService.GetSMSOrganizationalUsersByDepartmentAsync(department, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving SMS Organizational Users by department: {Department}", department);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    /// <summary>
    /// Gets department supervisors with hierarchy validation
    /// </summary>
    //public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetDepartmentSupervisorsAsync(string department, CancellationToken ct = default)
    //{
    //    try
    //    {
    //        _logger.LogInformation("Retrieving supervisors for department: {Department}", department);

    //        var result = await _dataService.GetDepartmentSupervisorsAsync(department, ct).ConfigureAwait(false);

    //        if (result.IsSuccess)
    //        {
    //            var supervisors = result.Value;
    //            _logger.LogInformation("Found {Count} supervisors for department: {Department}", 
    //                supervisors.Count(), department);

    //            // Business analysis - warn if department has no supervisors
    //            if (!supervisors.Any())
    //            {
    //                _logger.LogWarning("Department {Department} has no supervisors assigned", department);
    //            }
    //        }

    //        return result;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Unexpected error retrieving supervisors for department: {Department}", department);
    //        return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
    //    }
    //}

    /// <summary>
    /// Updates an existing SMS Organizational User with business validation
    /// </summary>
    public async Task<Result<SMSOrganizationalUser>> UpdateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Updating SMS Organizational User with ID: {Id}", user?.UserId);

            if (user is null)
            {
                _logger.LogError("UpdateSMSOrganizationalUserAsync received null user");
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            // Business validation - check if user exists
            var existingUserResult = await _dataService.GetSMSOrganizationalUserByCodeAsync(user.UserId.Value, ct).ConfigureAwait(false);
            if (existingUserResult.IsFailure)
            {
                _logger.LogWarning("Cannot update non-existent SMS Organizational User with ID: {Id}", user.UserId);
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
            }

            // Business validation - validate department and position combination
            await ValidateDepartmentPositionCombination(user.Department, user.Position);

            var result = await _dataService.UpdateSMSOrganizationalUserAsync(user, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated SMS Organizational User with ID: {Id}", user.UserId);
            }
            else
            {
                _logger.LogError("Failed to update SMS Organizational User. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating SMS Organizational User with ID: {Id}", user?.UserId);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Authenticates an SMS Organizational User with comprehensive business logic
    /// </summary>
    public async Task<Result<bool>> AuthenticateSMSOrganizationalUserAsync(string userName, string plainTextPassword, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Authenticating SMS Organizational User: {UserName}", userName);

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

            var result = await _dataService.AuthenticateSMSOrganizationalUserAsync(userName, plainTextPassword, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                var user = result.Value;

                // Business rule - check if user is active
                //if (!user.IsActive)
                //{
                //    _logger.LogWarning("Authentication failed - user is inactive: {UserName}", userName);
                //    return Result<bool>.Failure<bool>(DomainErrors.BaseUserError.InactiveUser);
                //}

                //_logger.LogInformation("Successfully authenticated SMS Organizational User: {UserName} from department: {Department}",
                //    userName, user.Department);
            }
            else
            {
                _logger.LogWarning("Authentication failed for user: {UserName}", userName);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during authentication for user: {UserName}", userName);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.LoginFailed);
        }
    }



    #region Private Business Logic Helpers

    /// <summary>
    /// Validates if department is valid (business rule)
    /// </summary>
    private bool IsValidDepartment(string department)
    {
        if (string.IsNullOrWhiteSpace(department))
            return false;

        // Business rule - validate against known departments
        var validDepartments = new[]
        {
            "Operations", "Safety", "Security", "Maintenance", "Administration",
            "Finance", "IT", "Human Resources", "Facilities", "Emergency Response",
            "Quality Assurance", "Training", "Communications", "Environmental",
            "Legal", "Planning", "Engineering", "Customer Service", "Ground Services", "Management"
        };

        return validDepartments.Contains(department, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates department and position combination (business rule)
    /// </summary>
    private async Task ValidateDepartmentPositionCombination(string department, string position)
    {
        // Business rule validation - certain positions only valid in certain departments
        var restrictedCombinations = new Dictionary<string, string[]>
        {
            ["Safety"] = new[] { "Safety Manager", "Safety Officer", "Safety Inspector", "Safety Analyst" },
            ["Security"] = new[] { "Security Manager", "Security Officer", "Security Supervisor" },
            ["Operations"] = new[] { "Operations Manager", "Operations Supervisor", "Dispatcher", "Coordinator" }
        };

        if (restrictedCombinations.ContainsKey(department))
        {
            var validPositions = restrictedCombinations[department];
            if (!validPositions.Contains(position, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Position {Position} in department {Department} requires validation", position, department);
                // Could implement additional validation logic here
            }
        }

        await Task.CompletedTask; // Placeholder for potential async validation
    }

    #endregion
}
