using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.System.UserManagement;

/// <summary>
/// Code-behind for Application Users management page
/// Provides comprehensive user management capabilities for SMS Application Users
/// </summary>
public partial class ApplicationUsers : ComponentBase
{
    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;
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

    // Password Modal Properties for Shared Component
    private bool ShowPasswordModal { get; set; }
    private string PasswordUserCode { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;

    // ?? NEW: Role Assignment Properties
    private bool ShowRoleAssignmentModal { get; set; }
    private string RoleAssignmentUserCode { get; set; } = string.Empty;
    private string RoleAssignmentUserDisplayName { get; set; } = string.Empty;
    private string? CurrentUserRoleCode { get; set; }
    private string? SelectedRoleCode { get; set; }
    private List<SMSUserRole> AvailableRoles { get; set; } = new();

    private static readonly string[] SMSModules =
    {
        "SMS_Assurance", "SMS_Policy", "SMS_Promotion", "SMS_RiskManagement", "SMS_System"
    };

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
    private CreateUserModel NewUser = new();
    private string? EditUserRoleCode { get; set; }

    // Create Modal Properties
    private bool ShowCreateModal { get; set; }
    private bool IsSaving { get; set; }

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

            // ?? NEW: Load User Roles for assignment
            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await Mediator.SendAsync(userRolesQuery, CancellationToken.None);
            AvailableRoles = userRolesResult.IsSuccess ?
                userRolesResult.Value?.ToList() ?? new List<SMSUserRole>() :
                new List<SMSUserRole>();

            Logger.LogInformation("Loaded {UserCount} application users, {GroupCount} groups, and {RoleCount} roles",
                ApplicationUsersList.Count, AllApplicationGroups.Count, AvailableRoles.Count);

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
            var getUserQuery = new GetSMSApplicationUserByCodeQuery(id);
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

            // Set the role code for dropdown binding
            EditUserRoleCode = CurrentUser.UserRole?.Code;

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user for edit: {UserId}", id);
            ShowErrorNotification("Error loading user. Please try again.");
        }
    }

    #endregion

    #region ?? NEW: Role Assignment Methods
    private bool IsPermissionGranted(SMSUserRole role, string module, string action)
    {
        if (role?.Permissions == null) return false;

        var permission = role.Permissions.FirstOrDefault(p => p.SMSModule == module);
        return action switch
        {
            "Create" => permission?.Create == true,
            "Read" => permission?.Read == true,
            "Update" => permission?.Update == true,
            "Delete" => permission?.Delete == true,
            _ => false
        };
    }
    private void OpenRoleAssignmentModal(string userCode, string userDisplayName, string? currentRoleCode = null)
    {
        RoleAssignmentUserCode = userCode;
        RoleAssignmentUserDisplayName = userDisplayName;
        CurrentUserRoleCode = currentRoleCode;
        SelectedRoleCode = currentRoleCode;
        ShowRoleAssignmentModal = true;
        StateHasChanged();
    }

    private void CloseRoleAssignmentModal()
    {
        ShowRoleAssignmentModal = false;
        RoleAssignmentUserCode = string.Empty;
        RoleAssignmentUserDisplayName = string.Empty;
        CurrentUserRoleCode = null;
        SelectedRoleCode = null;
        StateHasChanged();
    }

    private async Task AssignUserRole()
    {
        if (string.IsNullOrEmpty(RoleAssignmentUserCode) || string.IsNullOrEmpty(SelectedRoleCode))
        {
            ShowErrorNotification("Invalid user or role selection.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSApplicationUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await Mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value == null)
            {
                ShowErrorNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Get the selected role
            var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == SelectedRoleCode);
            if (selectedRole == null)
            {
                ShowErrorNotification("Selected role not found.");
                return;
            }
            user.SMSUserType = SMSUserType.Application;
            // Update user role
            user.UserRole = selectedRole;
            user.UpdatedBy = CurrentUserService?.UserDisplayName;
            user.UpdatedDate = DateTime.UtcNow;

            // Update user
            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var updateResult = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                ShowSuccessNotification($"Role '{selectedRole.Name}' successfully assigned to {RoleAssignmentUserDisplayName}.");

                // Refresh data and close modal
                await LoadDataAsync();
                CloseRoleAssignmentModal();
            }
            else
            {
                ShowErrorNotification($"Failed to assign role: {updateResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning role to user {UserCode}", RoleAssignmentUserCode);
            ShowErrorNotification("An error occurred while assigning the role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task RemoveUserRole()
    {
        if (string.IsNullOrEmpty(RoleAssignmentUserCode))
        {
            ShowErrorNotification("Invalid user selection.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get the user
            var userQuery = new GetSMSApplicationUserByCodeQuery(RoleAssignmentUserCode);
            var userResult = await Mediator.SendAsync(userQuery, CancellationToken.None);

            if (userResult.IsFailure || userResult.Value == null)
            {
                ShowErrorNotification("User not found.");
                return;
            }

            var user = userResult.Value;

            // Remove role
            user.UserRole = null;
            user.UpdatedBy = CurrentUserService?.UserDisplayName;
            user.UpdatedDate = DateTime.UtcNow;

            // Update user
            var updateCommand = new UpdateSMSApplicationUserCommand(user);
            var updateResult = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (updateResult.IsSuccess)
            {
                ShowSuccessNotification($"Role successfully removed from {RoleAssignmentUserDisplayName}.");

                // Refresh data and close modal
                await LoadDataAsync();
                CloseRoleAssignmentModal();
            }
            else
            {
                ShowErrorNotification($"Failed to remove role: {updateResult.Error?.Message}");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing role from user {UserCode}", RoleAssignmentUserCode);
            ShowErrorNotification("An error occurred while removing the role. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region CRUD Operations

    private async Task ShowCreateDialog()
    {
        NewUser = new CreateUserModel();
        ShowCreateModal = true;
        StateHasChanged();
    }

    private async Task CreateUser()
    {
        if (!IsCreateFormValid)
        {
            ShowErrorNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // ?? NEW: Get selected role if provided
            SMSUserRole? selectedRole = null;
            if (!string.IsNullOrEmpty(NewUser.UserRoleCode))
            {
                selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == NewUser.UserRoleCode);
            }

            // Create user entity
            var userId = new SMSApplicationUserID($"AU-0000");
            var user = new SMSApplicationUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(NewUser.FirstName).Value,
                LastName = LastName.Create(NewUser.LastName).Value,
                UserName = UserName.Create(NewUser.UserName).Value,
                Password = Password.Create(NewUser.Password).Value,
                UserRole = selectedRole, // ?? NEW: Assign role during creation
                IsActive = true,
                SMSUserType = SMSUserType.Application,
                CreatedBy = CurrentUserService?.UserDisplayName,
                CreatedDate = DateTime.UtcNow
            };

            var command = new CreateSMSApplicationUserCommand(user);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                var roleText = selectedRole != null ? $" with role '{selectedRole.Name}'" : "";
                ShowSuccessNotification($"Application user '{NewUser.FirstName} {NewUser.LastName}' created successfully{roleText}!");
                CloseCreateModal();
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
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewUser = new CreateUserModel();
        StateHasChanged();
    }

    private bool IsCreateFormValid =>
        !string.IsNullOrWhiteSpace(NewUser.FirstName) &&
        !string.IsNullOrWhiteSpace(NewUser.LastName) &&
        !string.IsNullOrWhiteSpace(NewUser.UserName) &&
        !string.IsNullOrWhiteSpace(NewUser.Password);

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
            CurrentUser.SMSUserType = SMSUserType.Application;
            // Update role if changed
            if (!string.IsNullOrEmpty(EditUserRoleCode))
            {
                var selectedRole = AvailableRoles.FirstOrDefault(r => r.Code == EditUserRoleCode);
                CurrentUser.UserRole = selectedRole;
            }
            else
            {
                CurrentUser.UserRole = null;
            }
            // Set the UpdatedBy field to the currently logged-in user's ID
            CurrentUser.UpdatedBy = CurrentUserService?.UserDisplayName;
            CurrentUser.UpdatedDate = DateTime.UtcNow;

            var updateCommand = new UpdateSMSApplicationUserCommand(CurrentUser);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                await LoadDataAsync();
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
        EditUserRoleCode = null;
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
        NotificationHelper.ShowSuccess(NotificationService, message);
    }

    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message);
    }

    private void ShowInfoNotification(string message)
    {
        NotificationHelper.ShowInfo(NotificationService, message);
    }

    #endregion

    #region Models

    public class EditUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
    }

    // ?? UPDATED: CreateUserModel with role assignment support
    public class CreateUserModel
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Password { get; set; } = "";
        public string? UserRoleCode { get; set; } // NEW: Role assignment during creation
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