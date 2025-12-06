using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using SMS3.Components.Pages.System.Components;

namespace SMS3.Components.Pages.System.UserManagement;

public partial class StakeholderUsers : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<StakeholderUsers> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    // Data Properties
    private List<SMSStakeholderUser> StakeholderUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Predefined stakeholder types
    private static readonly string[] StakeholderTypes = { "SUT-0001", "SUT-0002", "SUT-0003" };

    // Component References
    private RadzenDataGrid<SMSStakeholderUser>? usersGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Stakeholder Users
            var stakeholderUsersQuery = new GetAllSMSStakeholderUsersQuery();
            var stakeholderUsersResult = await Mediator.SendAsync(stakeholderUsersQuery, CancellationToken.None);
            StakeholderUsersList = stakeholderUsersResult.IsSuccess ? 
                stakeholderUsersResult.Value?.ToList() ?? new List<SMSStakeholderUser>() : 
                new List<SMSStakeholderUser>();

            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await Mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoles = userRolesResult.IsSuccess ? 
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() : 
                new List<SMSUserRole>();

            Logger.LogInformation("Loaded {UserCount} stakeholder users and {RoleCount} user roles", 
                StakeholderUsersList.Count, UserRoles.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading stakeholder users data");
            ShowErrorNotification("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        var createUser = new CreateStakeholderUserModel();
        
        var result = await DialogService.OpenAsync<CreateStakeholderUserDialog>("Create Stakeholder User",
            new Dictionary<string, object>
            {
                { "Model", createUser },
                { "UserRoles", UserRoles },
                { "StakeholderTypes", StakeholderTypes }
            },
            new DialogOptions { Width = "800px", Height = "600px", Resizable = true, Draggable = true });

        if (result is CreateStakeholderUserModel model && model != null)
        {
            await CreateUser(model);
        }
    }

    private async Task CreateUser(CreateStakeholderUserModel model)
    {
        try
        {
            // Create user entity
            var userId = new SMSStakeholderUserID($"SU-0000");
            var user = new SMSStakeholderUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(model.FirstName).Value,
                LastName = LastName.Create(model.LastName).Value,
                UserName = UserName.Create(model.UserName).Value,
                Password = Password.Create(model.Password).Value,
                StakeholderType = model.StakeholderType,
                Organization = model.Organization,
                IsActive = true,
                SMSUserType = "sut-0003" // MEMBER - this needs fixing as noted in original
            };

            // Assign user role if specified
            if (!string.IsNullOrWhiteSpace(model.UserRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(model.UserRoleCode);
                var roleResult = await Mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    user.UserRole = roleResult.Value;
                }
            }

            var command = new CreateSMSStakeholderUserCommand(user);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Stakeholder user '{model.FirstName} {model.LastName}' created successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating stakeholder user");
            ShowErrorNotification("Error creating stakeholder user. Please try again.");
        }
    }

    private async Task ShowEditDialog(SMSStakeholderUser user)
    {
        var editUser = new EditStakeholderUserModel
        {
            UserId = user.Code,
            FirstName = user.FirstName?.Value ?? "",
            LastName = user.LastName?.Value ?? "",
            StakeholderType = user.StakeholderType,
            Organization = user.Organization,
            UserRoleCode = user.UserRole?.Code ?? "",
            IsActive = user.IsActive
        };
        
        var result = await DialogService.OpenAsync<EditStakeholderUserDialog>("Edit Stakeholder User",
            new Dictionary<string, object>
            {
                { "Model", editUser },
                { "UserRoles", UserRoles },
                { "StakeholderTypes", StakeholderTypes }
            },
            new DialogOptions { Width = "800px", Height = "600px", Resizable = true, Draggable = true });

        if (result is EditStakeholderUserModel model && model != null)
        {
            await UpdateUser(model);
        }
    }

    private async Task UpdateUser(EditStakeholderUserModel model)
    {
        try
        {
            var getUserQuery = new GetSMSStakeholderUserByIdQuery(model.UserId);
            var userResult = await Mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ShowErrorNotification("User not found.");
                return;
            }

            var user = userResult.Value;
            user.FirstName = FirstName.Create(model.FirstName).Value;
            user.LastName = LastName.Create(model.LastName).Value;
            user.StakeholderType = model.StakeholderType;
            user.Organization = model.Organization;
            user.IsActive = model.IsActive;

            // Update user role if specified
            if (!string.IsNullOrWhiteSpace(model.UserRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(model.UserRoleCode);
                var roleResult = await Mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    user.UserRole = roleResult.Value;
                }
            }
            else
            {
                user.UserRole = null;
            }

            var updateCommand = new UpdateSMSStakeholderUserCommand(user);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Stakeholder user '{model.FirstName} {model.LastName}' updated successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating stakeholder user: {UserId}", model.UserId);
            ShowErrorNotification("Error updating stakeholder user. Please try again.");
        }
    }

    private async Task ShowDeleteDialog(string userId, string displayName)
    {
        var result = await DialogService.Confirm($"Are you sure you want to delete the user '{displayName}'?", 
            "Confirm Delete", 
            new ConfirmOptions 
            { 
                OkButtonText = "Delete", 
                CancelButtonText = "Cancel",
                AutoFocusFirstElement = true
            });

        if (result == true)
        {
            await DeleteUser(userId);
        }
    }

    private async Task DeleteUser(string userId)
    {
        try
        {
            var stakeholderUserId = new SMSStakeholderUserID(userId);
            var command = new DeleteSMSStakeholderUserCommand(stakeholderUserId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Stakeholder user deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting stakeholder user: {UserId}", userId);
            ShowErrorNotification("Error deleting stakeholder user. Please try again.");
        }
    }

    #endregion

    #region Password Management

    private async Task ShowPasswordDialog(string userId, string displayName)
    {
        var result = await DialogService.OpenAsync<ChangePasswordDialog>("Change Password",
            new Dictionary<string, object>
            {
                { "UserId", userId },
                { "DisplayName", displayName }
            },
            new DialogOptions { Width = "400px", Height = "300px", Resizable = true, Draggable = true });

        if (result is string newPassword && !string.IsNullOrWhiteSpace(newPassword))
        {
            await UpdatePassword(userId, newPassword);
        }
    }

    private async Task UpdatePassword(string userId, string newPassword)
    {
        try
        {
            var command = new UpdateSMSStakeholderUserPasswordCommand(userId, newPassword);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Password updated successfully.");
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update password.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating password: {UserId}", userId);
            ShowErrorNotification("Error updating password. Please try again.");
        }
    }

    #endregion

    #region Role Assignment

    private async Task ShowRoleDialog(string userId, string displayName, string? currentRoleCode)
    {
        var result = await DialogService.OpenAsync<AssignRoleDialog>("Assign User Role",
            new Dictionary<string, object>
            {
                { "UserId", userId },
                { "DisplayName", displayName },
                { "CurrentRoleCode", currentRoleCode ?? "" },
                { "UserRoles", UserRoles }
            },
            new DialogOptions { Width = "500px", Height = "400px", Resizable = true, Draggable = true });

        if (result is string roleCode)
        {
            await AssignRole(userId, displayName, roleCode);
        }
    }

    private async Task AssignRole(string userId, string displayName, string? userRoleCode)
    {
        try
        {
            var getUserQuery = new GetSMSStakeholderUserByIdQuery(userId);
            var userResult = await Mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ShowErrorNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Update user role
            if (!string.IsNullOrWhiteSpace(userRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(userRoleCode);
                var roleResult = await Mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    user.UserRole = roleResult.Value;
                    ShowSuccessNotification($"Role '{roleResult.Value.Name}' assigned to {displayName} successfully.");
                }
                else
                {
                    ShowErrorNotification("Selected role not found.");
                    return;
                }
            }
            else
            {
                // Clear role
                user.UserRole = null;
                ShowSuccessNotification($"Role removed from {displayName} successfully.");
            }

            var updateCommand = new UpdateSMSStakeholderUserCommand(user);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update user role.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning role to user: {UserId}", userId);
            ShowErrorNotification("Error assigning role. Please try again.");
        }
    }

    #endregion

    #region UI Helper Methods

    private async Task ExportUsers()
    {
        ShowInfoNotification("Export functionality will be implemented soon.");
    }

    private BadgeStyle GetStakeholderTypeBadgeStyle(string stakeholderType)
    {
        return stakeholderType switch
        {
            "SUT-0001" => BadgeStyle.Primary,   // Airline
            "SUT-0002" => BadgeStyle.Success,   // Ground Handler  
            "SUT-0003" => BadgeStyle.Warning,   // Contractor
            _ => BadgeStyle.Secondary
        };
    }

    private string GetStakeholderTypeDisplay(string stakeholderType)
    {
        return stakeholderType switch
        {
            "SUT-0001" => "Airline",
            "SUT-0002" => "Ground Handler",
            "SUT-0003" => "Contractor",
            _ => stakeholderType
        };
    }

    #endregion

    #region Notifications

    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000
        });
    }

    private void ShowErrorNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 6000
        });
    }

    private void ShowInfoNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = "Information",
            Detail = message,
            Duration = 4000
        });
    }

    #endregion

    #region Models

    public class CreateStakeholderUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string StakeholderType { get; set; } = "";
        public string Organization { get; set; } = "";
        public string UserRoleCode { get; set; } = "";
    }

    public class EditStakeholderUserModel
    {
        public string UserId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string StakeholderType { get; set; } = "";
        public string Organization { get; set; } = "";
        public string UserRoleCode { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    #endregion
}