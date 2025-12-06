using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;
using SMS_Shared.Common;
using SMS3.Components.Pages.System.Components;

namespace SMS3.Components.Pages.System.UserManagement;

public partial class OrganizationalUsers : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalUsers> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    // Data Properties
    private List<SMSOrganizationalUser> OrganizationalUsersList { get; set; } = new();
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Predefined departments
    private static readonly string[] Departments = 
    {
        "Operations", "Safety", "Security", "Maintenance", "Administration",
        "Finance", "IT", "Human Resources", "Facilities", "Emergency Response",
        "Quality Assurance", "Training", "Communications", "Environmental",
        "Legal", "Planning", "Engineering", "Customer Service", "Ground Services", "Management"
    };

    // Component References
    private RadzenDataGrid<SMSOrganizationalUser>? usersGrid;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Organizational Users
            var organizationalUsersQuery = new GetAllSMSOrganizationalUsersQuery();
            var organizationalUsersResult = await Mediator.SendAsync(organizationalUsersQuery, CancellationToken.None);
            OrganizationalUsersList = organizationalUsersResult.IsSuccess ? 
                organizationalUsersResult.Value?.ToList() ?? new List<SMSOrganizationalUser>() : 
                new List<SMSOrganizationalUser>();

            Logger.LogInformation("Loaded {UserCount} organizational users", OrganizationalUsersList.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading organizational users data");
            ShowErrorNotification("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        var createUser = new CreateOrganizationalUserModel();
        
        var result = await DialogService.OpenAsync<CreateOrganizationalUserDialog>("Create Organizational User",
            new Dictionary<string, object>
            {
                { "Model", createUser },
                { "Departments", Departments },
                { "OrganizationLevels", GetOrganizationLevels() }
            },
            new DialogOptions { Width = "800px", Height = "600px", Resizable = true, Draggable = true });

        if (result is CreateOrganizationalUserModel model && model != null)
        {
            await CreateUser(model);
        }
    }

    private async Task CreateUser(CreateOrganizationalUserModel model)
    {
        try
        {
            // Create user entity
            var userId = new SMSOrganizationalUserID($"OU-0000");
            var user = new SMSOrganizationalUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(model.FirstName).Value,
                LastName = LastName.Create(model.LastName).Value,
                UserName = UserName.Create(model.UserName).Value,
                Password = Password.Create(model.Password).Value,
                Department = model.Department,
                Position = model.Position,
                OrganizationLevel = model.OrganizationLevel,
                IsActive = true
            };

            var command = new CreateSMSOrganizationalUserCommand(user);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Organizational user '{model.FirstName} {model.LastName}' created successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create organizational user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating organizational user");
            ShowErrorNotification("Error creating organizational user. Please try again.");
        }
    }

    private async Task ShowEditDialog(SMSOrganizationalUser user)
    {
        var editUser = new EditOrganizationalUserModel
        {
            UserId = user.Code,
            FirstName = user.FirstName?.Value ?? "",
            LastName = user.LastName?.Value ?? "",
            Department = user.Department,
            Position = user.Position,
            OrganizationLevel = user.OrganizationLevel,
            IsActive = user.IsActive
        };
        
        var result = await DialogService.OpenAsync<EditOrganizationalUserDialog>("Edit Organizational User",
            new Dictionary<string, object>
            {
                { "Model", editUser },
                { "Departments", Departments },
                { "OrganizationLevels", GetOrganizationLevels() }
            },
            new DialogOptions { Width = "800px", Height = "600px", Resizable = true, Draggable = true });

        if (result is EditOrganizationalUserModel model && model != null)
        {
            await UpdateUser(model);
        }
    }

    private async Task UpdateUser(EditOrganizationalUserModel model)
    {
        try
        {
            var getUserQuery = new GetSMSOrganizationalUserByIdQuery(model.UserId);
            var userResult = await Mediator.SendAsync(getUserQuery, CancellationToken.None);
            
            if (userResult.IsFailure)
            {
                ShowErrorNotification("User not found.");
                return;
            }

            var user = userResult.Value;
            user.FirstName = FirstName.Create(model.FirstName).Value;
            user.LastName = LastName.Create(model.LastName).Value;
            user.Department = model.Department;
            user.Position = model.Position;
            user.OrganizationLevel = model.OrganizationLevel;
            user.IsActive = model.IsActive;

            var updateCommand = new UpdateSMSOrganizationalUserCommand(user);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Organizational user '{model.FirstName} {model.LastName}' updated successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update organizational user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating organizational user: {UserId}", model.UserId);
            ShowErrorNotification("Error updating organizational user. Please try again.");
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
            var organizationalUserId = new SMSOrganizationalUserID(userId);
            var command = new DeleteSMSOrganizationalUserCommand(organizationalUserId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Organizational user deleted successfully.");
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete organizational user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting organizational user: {UserId}", userId);
            ShowErrorNotification("Error deleting organizational user. Please try again.");
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
            var command = new UpdateSMSOrganizationalUserPasswordCommand(userId, newPassword);
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

    #region UI Helper Methods

    private async Task ExportUsers()
    {
        ShowInfoNotification("Export functionality will be implemented soon.");
    }

    private BadgeStyle GetDepartmentBadgeStyle(string department)
    {
        return department switch
        {
            "Safety" => BadgeStyle.Primary,
            "Operations" => BadgeStyle.Success,
            "Security" => BadgeStyle.Warning,
            "Management" => BadgeStyle.Info,
            "IT" => BadgeStyle.Light,
            _ => BadgeStyle.Secondary
        };
    }

    private string GetOrganizationalRoleDisplay(SMSUserRole role)
    {
        return role.ToString().Replace("_", " ");
    }

    private string GetOrganizationLevelDisplay(string level)
    {
        return level?.Replace("_", " ") ?? "No Level";
    }

    private List<string> GetOrganizationLevels()
    {
        return new List<string>
        {
            "Staff", "Supervisor", "Manager", "Director", "Executive"
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

    public class CreateOrganizationalUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string OrganizationLevel { get; set; } = "";
    }

    public class EditOrganizationalUserModel
    {
        public string UserId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Department { get; set; } = "";
        public string Position { get; set; } = "";
        public string OrganizationLevel { get; set; } = "";
        public bool IsActive { get; set; } = true;
    }

    #endregion
}