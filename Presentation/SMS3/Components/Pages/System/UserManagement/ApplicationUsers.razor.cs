using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS3.Components.Pages.System.UserManagement;

public partial class ApplicationUsers : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationUsers> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private ISMSSessionService SessionService { get; set; } = default!;

    [Parameter] public string? Id { get; set; }

    // Data Properties - renamed to avoid conflict
    private List<SMSApplicationUser> ApplicationUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private SMSApplicationUser? CurrentUser { get; set; }
    private bool IsEditMode { get; set; }
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Password Modal Properties for Shared Component
    private bool ShowPasswordModal { get; set; }
    private string PasswordUserCode { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;

    // Group Management Properties
    private bool ShowGroupsModal { get; set; } = false;
    private string GroupManagementUserCode { get; set; } = string.Empty;
    private string GroupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSApplicationGroup> AllApplicationGroups { get; set; } = new();
    private List<SMSApplicationGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSApplicationGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

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
            var applicationUsersQuery = new GetAllSMSApplicationUsersQuery();
            var applicationUsersResult = await Mediator.SendAsync(applicationUsersQuery, CancellationToken.None);
            ApplicationUsersList = applicationUsersResult.IsSuccess ? 
                applicationUsersResult.Value?.ToList() ?? new List<SMSApplicationUser>() : 
                new List<SMSApplicationUser>();

            // Load Application Groups for group management
            var groupsQuery = new GetAllSMSApplicationGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            AllApplicationGroups = groupsResult.IsSuccess ? 
                groupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() : 
                new List<SMSApplicationGroup>();

            Logger.LogInformation("Loaded {UserCount} application users and {GroupCount} application groups", 
                ApplicationUsersList.Count, AllApplicationGroups.Count);

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
        // TODO: Implement create modal instead of dialog
        ShowInfoNotification("Create user functionality will be implemented with modal interface soon.");
        
        /*
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
        */
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
                SMSUserType = "Application",
                CreatedBy = SessionService.GetCurrentUserId() ?? "SYSTEM",
                CreatedDate = DateTime.UtcNow
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
            
            // Set the UpdatedBy field to the currently logged-in user's ID
            CurrentUser.UpdatedBy = SessionService.GetCurrentUserId() ?? "SYSTEM";
            CurrentUser.UpdatedDate = DateTime.UtcNow;

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

    private void OpenPasswordChangeModal(string userCode, string displayName)
    {
        PasswordUserCode = userCode;
        PasswordUserDisplayName = displayName;
        ShowPasswordModal = true;
        StateHasChanged();
    }

    private void ClosePasswordChangeModal()
    {
        ShowPasswordModal = false;
        PasswordUserCode = string.Empty;
        PasswordUserDisplayName = string.Empty;
        StateHasChanged();
    }

    private async Task OnPasswordChangedSuccess()
    {
        // Password was changed successfully by the modal
        ShowSuccessNotification($"Password updated successfully for {PasswordUserDisplayName}.");
    }

    private async Task ShowPasswordDialog(string userId, string displayName)
    {
        // Legacy method - replaced with OpenPasswordChangeModal
        OpenPasswordChangeModal(userId, displayName);
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

    #region Group Management

    private async Task ManageGroups(string userId, string displayName)
    {
        try
        {
            GroupManagementUserCode = userId;
            GroupManagementUserDisplayName = displayName;
            
            await LoadUserGroups(userId);
            ShowGroupsModal = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening group management for user: {UserId}", userId);
            ShowErrorNotification("Error loading user groups. Please try again.");
        }
    }

    private async Task LoadUserGroups(string userId)
    {
        try
        {
            // Load groups that this user is currently assigned to
            var userGroupsQuery = new GetSMSApplicationGroupsByUserCodeQuery(userId);
            var userGroupsResult = await Mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ? 
                userGroupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() : 
                new List<SMSApplicationGroup>();
            
            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllApplicationGroups
                .Where(g => !currentGroupCodes.Contains(g.Code) && g.IsActive)
                .OrderBy(g => g.Name)
                .ToList();

            // Initialize selection tracking
            SelectedGroups.Clear();
            foreach (var group in AvailableGroups)
            {
                SelectedGroups[group.Code] = false;
            }

            Logger.LogInformation("Loaded {CurrentGroupCount} current groups and {AvailableGroupCount} available groups for user {UserId}", 
                UserCurrentGroups.Count, AvailableGroups.Count, userId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading groups for user: {UserId}", userId);
            UserCurrentGroups = new List<SMSApplicationGroup>();
            AvailableGroups = AllApplicationGroups.Where(g => g.IsActive).ToList();
            
            // Initialize selection tracking even on error
            SelectedGroups.Clear();
            foreach (var group in AvailableGroups)
            {
                SelectedGroups[group.Code] = false;
            }
        }
    }

    private void CloseGroupsModal()
    {
        ShowGroupsModal = false;
        GroupManagementUserCode = string.Empty;
        GroupManagementUserDisplayName = string.Empty;
        UserCurrentGroups.Clear();
        AvailableGroups.Clear();
        SelectedGroups.Clear();
    }

    private async Task RemoveUserFromGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(GroupManagementUserCode))
        {
            ShowErrorNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var command = new RemoveUserFromApplicationGroupCommand(GroupManagementUserCode, groupCode);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("User removed from group successfully.");
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", GroupManagementUserCode, groupCode);
            ShowErrorNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignUserToGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(GroupManagementUserCode))
        {
            ShowErrorNotification("Group code and user code are required.");
            return;
        }

        try
        {
            var command = new AssignUserToApplicationGroupCommand(GroupManagementUserCode, groupCode);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("User assigned to group successfully.");
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", GroupManagementUserCode, groupCode);
            ShowErrorNotification("Error assigning user to group. Please try again.");
        }
    }

    private async Task AssignMultipleGroups()
    {
        if (string.IsNullOrWhiteSpace(GroupManagementUserCode) || !SelectedGroups.Any(s => s.Value))
        {
            ShowErrorNotification("User code and at least one group must be selected.");
            return;
        }

        try
        {
            var selectedGroupCodes = SelectedGroups.Where(s => s.Value).Select(s => s.Key).ToArray();
            int successCount = 0;
            int failureCount = 0;

            foreach (var groupCode in selectedGroupCodes)
            {
                try
                {
                    var command = new AssignUserToApplicationGroupCommand(GroupManagementUserCode, groupCode);
                    var result = await Mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", GroupManagementUserCode, groupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned user to {successCount} group(s).";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                ShowSuccessNotification(message);
                
                await LoadUserGroups(GroupManagementUserCode);
                StateHasChanged();
            }
            else
            {
                ShowErrorNotification("Failed to assign user to groups.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning user {UserCode} to multiple groups", GroupManagementUserCode);
            ShowErrorNotification("Error assigning user to groups. Please try again.");
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeGroupSelection()
    {
        // Called after loading user groups to initialize the selection dictionary
        SelectedGroups.Clear();
        foreach (var group in AvailableGroups)
        {
            SelectedGroups[group.Code] = false;
        }
    }

    #endregion
}