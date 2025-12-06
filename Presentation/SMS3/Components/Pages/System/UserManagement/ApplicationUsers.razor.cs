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

public partial class ApplicationUsers : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationUsers> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Parameter] public string? Id { get; set; }

    // Data Properties - renamed to avoid conflict
    private List<SMSApplicationUser> ApplicationUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private SMSApplicationUser? CurrentUser { get; set; }
    private bool IsEditMode { get; set; }
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Form Models
    private EditUserModel editUser = new();
    private CreateUserModel createUser = new();

    // Component References
    private RadzenDataGrid<SMSApplicationUser>? usersGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
        
        // Check if we're in edit mode
        if (!string.IsNullOrWhiteSpace(Id))
        {
            await LoadUserForEdit(Id);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!string.IsNullOrWhiteSpace(Id) && !IsEditMode)
        {
            await LoadUserForEdit(Id);
        }
        else if (string.IsNullOrWhiteSpace(Id) && IsEditMode)
        {
            CancelEdit();
        }
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Application Users
            var appUsersQuery = new GetAllSMSApplicationUsersQuery();
            var appUsersResult = await Mediator.SendAsync(appUsersQuery, CancellationToken.None);
            ApplicationUsersList = appUsersResult.IsSuccess ? 
                appUsersResult.Value?.ToList() ?? new List<SMSApplicationUser>() : 
                new List<SMSApplicationUser>();

            // Load User Roles
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await Mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoles = userRolesResult.IsSuccess ? 
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() : 
                new List<SMSUserRole>();

            Logger.LogInformation("Loaded {UserCount} application users and {RoleCount} user roles", 
                ApplicationUsersList.Count, UserRoles.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading application users data");
            ShowErrorNotification("Error loading data. Please refresh the page.");
        }
    }

    private async Task LoadUserForEdit(string id)
    {
        try
        {
            var getUserQuery = new GetSMSApplicationUserByIdQuery(id);
            var userResult = await Mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ShowErrorNotification("User not found.");
                Navigation.NavigateTo("/System/UserManagement/ApplicationUsers");
                return;
            }

            CurrentUser = userResult.Value;
            IsEditMode = true;
            
            // Populate edit form
            editUser = new EditUserModel
            {
                FirstName = CurrentUser.FirstName?.Value ?? "",
                LastName = CurrentUser.LastName?.Value ?? ""
            };

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user for edit: {UserId}", id);
            ShowErrorNotification("Error loading user. Please try again.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        createUser = new CreateUserModel();
        
        var result = await DialogService.OpenAsync<CreateApplicationUserDialog>("Create Application User",
            new Dictionary<string, object>
            {
                { "Model", createUser }
            },
            new DialogOptions { Width = "600px", Height = "500px", Resizable = true, Draggable = true });

        if (result is CreateUserModel model && model != null)
        {
            await CreateUser(model);
        }
    }

    private async Task CreateUser(CreateUserModel model)
    {
        try
        {
            // Create user entity
            var userId = new SMSApplicationUserID($"AU-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
            var user = new SMSApplicationUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(model.FirstName).Value,
                LastName = LastName.Create(model.LastName).Value,
                UserName = UserName.Create(model.UserName).Value,
                Password = Password.Create(model.Password).Value,
                IsActive = true,
                SMSUserType = "Application"
            };

            var command = new CreateSMSApplicationUserCommand(user);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Application user '{model.FirstName} {model.LastName}' created successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create application user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating application user");
            ShowErrorNotification("Error creating application user. Please try again.");
        }
    }

    private async Task EditUser(string userId)
    {
        Navigation.NavigateTo($"/System/UserManagement/ApplicationUsers/Edit/{userId}");
    }

    private async Task UpdateUser(EditUserModel model)
    {
        try
        {
            if (CurrentUser == null)
            {
                ShowErrorNotification("No user selected for update.");
                return;
            }

            // Update user properties using the model parameter
            CurrentUser.FirstName = FirstName.Create(model.FirstName).Value;
            CurrentUser.LastName = LastName.Create(model.LastName).Value;

            var updateCommand = new UpdateSMSApplicationUserCommand(CurrentUser);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Application user '{model.FirstName} {model.LastName}' updated successfully.");
                Navigation.NavigateTo("/System/UserManagement/ApplicationUsers");
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update application user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating application user: {UserId}", CurrentUser?.Code);
            ShowErrorNotification("Error updating application user. Please try again.");
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
            var applicationUserId = new SMSApplicationUserID(userId);
            var command = new DeleteSMSApplicationUserCommand(applicationUserId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Application user deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete application user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting application user: {UserId}", userId);
            ShowErrorNotification("Error deleting application user. Please try again.");
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
            var command = new UpdateSMSApplicationUserPasswordCommand(userId, newPassword);
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

    #region Navigation & UI

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentUser = null;
        editUser = new EditUserModel();
        Navigation.NavigateTo("/System/UserManagement/ApplicationUsers");
    }

    private void NavigateToUserManagement()
    {
        Navigation.NavigateTo("/System/UserManagement");
    }

    private async Task ExportUsers()
    {
        // TODO: Implement export functionality
        ShowInfoNotification("Export functionality will be implemented soon.");
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

    public class EditUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }

    public class CreateUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
    }

    #endregion
}