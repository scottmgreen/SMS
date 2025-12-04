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
/// Application User Management - Full CRUD for SMS Application Users
/// </summary>
public class ApplicationUsersModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationUsersModel> _logger;

    public ApplicationUsersModel(IMediator mediator, ILogger<ApplicationUsersModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties

    public IEnumerable<SMSApplicationUser> ApplicationUsers { get; private set; } = new List<SMSApplicationUser>();
    public IEnumerable<SMSUserRole> UserRoles { get; private set; } = new List<SMSUserRole>();
    public SMSApplicationUser? CurrentUser { get; set; }
    public bool IsEditMode { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData] 
    public string? ErrorMessage { get; set; }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync(string? id = null, string? action = null)
    {
        await LoadDataAsync();
        
        // If id is provided, go into edit mode
        if (!string.IsNullOrWhiteSpace(id))
        {
            return await OnGetEditAsync(id);
        }
        
        if (action == "create")
        {
            IsEditMode = false;
            CurrentUser = null;
        }
        
        return Page();
    }

    public async Task<IActionResult> OnGetEditAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            ErrorMessage = "User ID is required.";
            return RedirectToPage();
        }

        try
        {
            var getUserQuery = new GetSMSApplicationUserByIdQuery(id);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ErrorMessage = "User not found.";
                return RedirectToPage();
            }

            CurrentUser = userResult.Value;
            IsEditMode = true;
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user for edit: {UserId}", id);
            ErrorMessage = "Error loading user. Please try again.";
            return RedirectToPage();
        }
    }

    #endregion

    #region Create/Update Handlers

    public async Task<IActionResult> OnPostCreateAsync(
        string firstName, 
        string lastName, 
        string userName, 
        string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "All required fields must be filled.";
                await LoadDataAsync();
                return Page();
            }

            // Create user entity
            var userId = new SMSApplicationUserID($"AU-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            var user = new SMSApplicationUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(firstName).Value,
                LastName = LastName.Create(lastName).Value,
                UserName = UserName.Create(userName).Value,
                Password = Password.Create(password).Value,
                IsActive = true,
                SMSUserType = "Application"
            };

            var command = new CreateSMSApplicationUserCommand(user);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Application user '{firstName} {lastName}' created successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to create application user.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application user");
            ErrorMessage = "Error creating application user. Please try again.";
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        string userId,
        string firstName,
        string lastName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                ErrorMessage = "All required fields must be filled.";
                return await OnGetEditAsync(userId);
            }

            var getUserQuery = new GetSMSApplicationUserByIdQuery(userId);
            var userResult = await _mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ErrorMessage = "User not found.";
                return RedirectToPage();
            }

            var user = userResult.Value;
            user.FirstName = FirstName.Create(firstName).Value;
            user.LastName = LastName.Create(lastName).Value;

            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Application user '{firstName} {lastName}' updated successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update application user.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application user: {UserId}", userId);
            ErrorMessage = "Error updating application user. Please try again.";
        }

        return await OnGetEditAsync(userId);
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

            var applicationUserId = new SMSApplicationUserID(userId);
            var command = new DeleteSMSApplicationUserCommand(applicationUserId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "Application user deleted successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to delete application user.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application user: {UserId}", userId);
            ErrorMessage = "Error deleting application user. Please try again.";
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

            var command = new UpdateSMSApplicationUserPasswordCommand(userId, newPassword);
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

    #region Helper Methods

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Application Users
            var appUsersQuery = new GetAllSMSApplicationUsersQuery();
            var appUsersResult = await _mediator.SendAsync(appUsersQuery, CancellationToken.None);
            ApplicationUsers = appUsersResult.IsSuccess ? appUsersResult.Value ?? new List<SMSApplicationUser>() : new List<SMSApplicationUser>();

            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoles = userRolesResult.IsSuccess ? userRolesResult.Value ?? new List<SMSUserRole>() : new List<SMSUserRole>();

            _logger.LogInformation("Loaded {UserCount} application users and {RoleCount} user roles", 
                ApplicationUsers.Count(), UserRoles.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            throw;
        }
    }

    #endregion
}