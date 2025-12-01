using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Presentation.Pages.System;

/// <summary>
/// Stakeholder User Management - Full CRUD for SMS Stakeholder Users
/// </summary>
public class StakeholderUsersModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<StakeholderUsersModel> _logger;

    public StakeholderUsersModel(IMediator mediator, ILogger<StakeholderUsersModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties

    public IEnumerable<SMSStakeholderUser> StakeholderUsers { get; private set; } = new List<SMSStakeholderUser>();
    public IEnumerable<SMSUserRole> UserRoles { get; private set; } = new List<SMSUserRole>();

    // Predefined stakeholder types
    public static readonly string[] StakeholderTypes = { "SUT-0001", "SUT-0002" ,"SUT-0003"  };

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData] 
    public string? ErrorMessage { get; set; }

    #endregion

    #region Create/Update Handlers

    public async Task<IActionResult> OnPostCreateAsync(
        string firstName, 
        string lastName, 
        string userName, 
        string password,
        string stakeholderType,
        string organization,
        string? userRoleCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(stakeholderType) || string.IsNullOrWhiteSpace(organization))
            {
                ErrorMessage = "All required fields must be filled.";
                await LoadDataAsync();
                return Page();
            }

            // Create user entity
            var userId = new SMSStakeholderUserID($"SU-0000");
            var user = new SMSStakeholderUser(userId);

            user.Code = userId.Value;
            user.FirstName = FirstName.Create(firstName).Value;
            user.LastName = LastName.Create(lastName).Value;
            user.UserName = UserName.Create(userName).Value;
            user.Password = Password.Create(password).Value;
            user.StakeholderType = stakeholderType;
            user.Organization = organization;
            user.IsActive = true;
            user.SMSUserType = "sut-0003";//mEMEBER.. this needs fixing
            ;

            // Assign user role if specified
            if (!string.IsNullOrWhiteSpace(userRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(userRoleCode);
                var roleResult = await _mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    user.UserRole = roleResult.Value;
                }
            }

            var command = new CreateSMSStakeholderUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Stakeholder user '{firstName} {lastName}' created successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to create stakeholder user.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stakeholder user");
            ErrorMessage = "Error creating stakeholder user. Please try again.";
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        string userId,
        string firstName,
        string lastName,
        string stakeholderType,
        string organization,
        string? userRoleCode,
        bool isActive)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(firstName) || 
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(stakeholderType) ||
                string.IsNullOrWhiteSpace(organization))
            {
                ErrorMessage = "All required fields must be filled.";
                await LoadDataAsync();
                return Page();
            }

            var getUserQuery = new GetSMSStakeholderUserByIdQuery(userId);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ErrorMessage = "User not found.";
                await LoadDataAsync();
                return Page();
            }

            var user = userResult.Value;
            user.FirstName = FirstName.Create(firstName).Value;
            user.LastName = LastName.Create(lastName).Value;
            user.StakeholderType = stakeholderType;
            user.Organization = organization;
            user.IsActive = isActive;

            // Update user role if specified
            if (!string.IsNullOrWhiteSpace(userRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(userRoleCode);
                var roleResult = await _mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    user.UserRole = roleResult.Value;
                }
            }

            var updateCommand = new UpdateSMSStakeholderUserCommand(user);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Stakeholder user '{firstName} {lastName}' updated successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update stakeholder user.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stakeholder user: {UserId}", userId);
            ErrorMessage = "Error updating stakeholder user. Please try again.";
        }

        await LoadDataAsync();
        return Page();
    }

    #endregion

    #region Delete Handler

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                ErrorMessage = "User ID is required for deletion.";
                return RedirectToPage();
            }

            var stakeholderUserId = new SMSStakeholderUserID(userId);
            var command = new DeleteSMSStakeholderUserCommand(stakeholderUserId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "Stakeholder user deleted successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to delete stakeholder user.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stakeholder user: {UserId}", userId);
            ErrorMessage = "Error deleting stakeholder user. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Password Management

    public async Task<IActionResult> OnPostUpdatePasswordAsync(string userId, string newPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newPassword))
            {
                ErrorMessage = "User ID and new password are required.";
                return RedirectToPage();
            }

            var command = new UpdateSMSStakeholderUserPasswordCommand(userId, newPassword, "SYSTEM");
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "Password updated successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update password.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password: {UserId}", userId);
            ErrorMessage = "Error updating password. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Role Assignment Handler

    public async Task<IActionResult> OnPostAssignRoleAsync(string userId, string? userRoleCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                ErrorMessage = "User ID is required for role assignment.";
                await LoadDataAsync();
                return Page();
            }

            var getUserQuery = new GetSMSStakeholderUserByIdQuery(userId);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ErrorMessage = "User not found.";
                await LoadDataAsync();
                return Page();
            }

            var user = userResult.Value;

            // Update user role
            if (!string.IsNullOrWhiteSpace(userRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(userRoleCode);
                var roleResult = await _mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    user.UserRole = roleResult.Value;
                    SuccessMessage = $"Role '{roleResult.Value.Name}' assigned to {user.DisplayName} successfully.";
                }
                else
                {
                    ErrorMessage = "Selected role not found.";
                    await LoadDataAsync();
                    return Page();
                }
            }
            else
            {
                // Clear role
                user.UserRole = null;
                SuccessMessage = $"Role removed from {user.DisplayName} successfully.";
            }

            var updateCommand = new UpdateSMSStakeholderUserCommand(user);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsFailure)
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update user role.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user: {UserId}", userId);
            ErrorMessage = "Error assigning role. Please try again.";
        }

        await LoadDataAsync();
        return Page();
    }

    #endregion

    #region Helper Methods

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Stakeholder Users
            var stakeholderUsersQuery = new GetAllSMSStakeholderUsersQuery();
            var stakeholderUsersResult = await _mediator.SendAsync(stakeholderUsersQuery, CancellationToken.None);
            StakeholderUsers = stakeholderUsersResult.IsSuccess ? stakeholderUsersResult.Value ?? new List<SMSStakeholderUser>() : new List<SMSStakeholderUser>();

            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoles = userRolesResult.IsSuccess ? userRolesResult.Value ?? new List<SMSUserRole>() : new List<SMSUserRole>();

            _logger.LogInformation("Loaded {UserCount} stakeholder users and {RoleCount} user roles", 
                StakeholderUsers.Count(), UserRoles.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            throw;
        }
    }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDataAsync();
        return Page();
    }

    #endregion
}