using SMS3.Components.Shared.UIHelpers;

namespace SMS3.Components.Pages.System.UserManagement;

/// <summary>
/// Code-behind for Stakeholder Users management page  
/// Handles external stakeholder user management
/// </summary>
public partial class StakeholderUsers : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<StakeholderUsers> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    [Inject] private AuthenticationService AuthService { get; set; } = default!;

    // Data Properties
    private List<SMSStakeholderUser> StakeholderUsersList { get; set; } = new();
    private List<SMSUserRole> UserRoles { get; set; } = new();
    private string? SuccessMessage { get; set; }
    private string? ErrorMessage { get; set; }

    // Predefined stakeholder types
    //private static readonly string[] StakeholderTypes = { "SUT-0001", "SUT-0002", "SUT-0003" };
    private string[] StakeholderTypes { get; set; } = Array.Empty<string>();

    // Form Models
    private EditStakeholderUserModel editUser = new();
    private CreateStakeholderUserModel NewUser = new();

    // Create Modal Properties  
    private bool ShowCreateModal { get; set; }
    private bool IsSaving { get; set; }

    // Edit Modal Properties
    private bool ShowEditModal { get; set; }
    private SMSStakeholderUser? CurrentEditUser { get; set; }

    // Role Assignment Modal Properties
    private bool ShowRoleModal { get; set; }
    private string RoleUserCode { get; set; } = string.Empty;
    private string RoleUserDisplayName { get; set; } = string.Empty;
    private string CurrentRoleCode { get; set; } = string.Empty;
    private static readonly string[] SMSModules =
    {
        "SMS_Assurance", "SMS_Policy", "SMS_Promotion", "SMS_RiskManagement", "SMS_System"
    };
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

            // Load Stakeholder Groups for group management
            var groupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            AllStakeholderGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>() :
                new List<SMSStakeholderGroup>();


            StakeholderTypes = SMSStakeholderType.GetAllValuesAsStringArray();


            Logger.LogInformation("Loaded {UserCount} stakeholder users, {RoleCount} user roles, and {GroupCount} stakeholder groups",
                StakeholderUsersList.Count, UserRoles.Count, AllStakeholderGroups.Count);

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
        NewUser = new CreateStakeholderUserModel();
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

            // Create user entity
            var userId = new SMSStakeholderUserID($"SU-0000");
            var user = new SMSStakeholderUser(userId)
            {
                Code = userId.Value,
                FirstName = FirstName.Create(NewUser.FirstName).Value,
                LastName = LastName.Create(NewUser.LastName).Value,
                UserName = UserName.Create(NewUser.UserName).Value,
                Password = Password.Create(NewUser.Password).Value,
                StakeholderType = NewUser.StakeholderType,
                Organization = NewUser.Organization,
                IsActive = NewUser.IsActive,
                IsPOPEmployee = NewUser.IsPOPEmployee,
                SMSUserType = SMSUserType.Stakeholder
            };

            // Assign user role if specified
            if (!string.IsNullOrWhiteSpace(NewUser.UserRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(NewUser.UserRoleCode);
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
                ShowSuccessNotification($"Stakeholder user '{NewUser.FirstName} {NewUser.LastName}' created successfully.");
                CloseCreateModal();
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
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewUser = new CreateStakeholderUserModel();
        StateHasChanged();
    }

    private bool IsCreateFormValid =>
        !string.IsNullOrWhiteSpace(NewUser.FirstName) &&
        !string.IsNullOrWhiteSpace(NewUser.LastName) &&
        !string.IsNullOrWhiteSpace(NewUser.UserName) &&
        !string.IsNullOrWhiteSpace(NewUser.Password) &&
        !string.IsNullOrWhiteSpace(NewUser.StakeholderType) &&
        !string.IsNullOrWhiteSpace(NewUser.Organization);

    // Transform stakeholder types for dropdown
    private IEnumerable<object> StakeholderTypesForDropdown => StakeholderTypes.Select(type => new
    {
        Value = type,
        Text = GetStakeholderTypeDisplay(type)
    });

    private async Task ShowEditDialog(SMSStakeholderUser user)
    {
        CurrentEditUser = user;
        editUser = new EditStakeholderUserModel
        {
            UserId = user.Code,
            FirstName = user.FirstName?.Value ?? "",
            LastName = user.LastName?.Value ?? "",
            StakeholderType = user.StakeholderType,
            Organization = user.Organization,
            UserRoleCode = user.UserRole?.Code ?? "",
            IsActive = user.IsActive,
            IsPOPEmployee = user.IsPOPEmployee
        };
        ShowEditModal = true;
        StateHasChanged();
    }

    private async Task UpdateUser()
    {
        if (!IsEditFormValid)
        {
            ShowErrorNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            if (CurrentEditUser == null)
            {
                ShowErrorNotification("No user selected for update.");
                return;
            }

            CurrentEditUser.FirstName = FirstName.Create(editUser.FirstName).Value;
            CurrentEditUser.LastName = LastName.Create(editUser.LastName).Value;
            CurrentEditUser.StakeholderType = editUser.StakeholderType;
            CurrentEditUser.Organization = editUser.Organization;
            CurrentEditUser.IsActive = editUser.IsActive;
            CurrentEditUser.IsPOPEmployee = editUser.IsPOPEmployee;
            CurrentEditUser.UpdatedBy = AuthService.CurrentUser.Code;
            CurrentEditUser.SMSUserType = SMSUserType.Stakeholder;
            // Update user role if specified
            if (!string.IsNullOrWhiteSpace(editUser.UserRoleCode))
            {
                var roleQuery = new GetSMSUserRoleByIdQuery(editUser.UserRoleCode);
                var roleResult = await Mediator.SendAsync(roleQuery, CancellationToken.None);
                if (roleResult.IsSuccess && roleResult.Value != null)
                {
                    CurrentEditUser.UserRole = roleResult.Value;
                }
            }
            else
            {
                CurrentEditUser.UserRole = null;
            }

            var updateCommand = new UpdateSMSStakeholderUserCommand(CurrentEditUser);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Stakeholder user '{editUser.FirstName} {editUser.LastName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update stakeholder user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating stakeholder user: {UserId}", editUser.UserId);
            ShowErrorNotification("Error updating stakeholder user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void CloseEditModal()
    {
        ShowEditModal = false;
        CurrentEditUser = null;
        editUser = new EditStakeholderUserModel();
        StateHasChanged();
    }

    private bool IsEditFormValid =>
        !string.IsNullOrWhiteSpace(editUser.FirstName) &&
        !string.IsNullOrWhiteSpace(editUser.LastName) &&
        !string.IsNullOrWhiteSpace(editUser.StakeholderType) &&
        !string.IsNullOrWhiteSpace(editUser.Organization);

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

    // Password Modal Properties for Shared Component
    private bool ShowPasswordModal { get; set; }
    private string PasswordUserCode { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;

    // Group Management Properties
    private bool ShowGroupsModal { get; set; } = false;
    private string GroupManagementUserCode { get; set; } = string.Empty;
    private string GroupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSStakeholderGroup> AllStakeholderGroups { get; set; } = new();
    private List<SMSStakeholderGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSStakeholderGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

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

    // Legacy password methods - kept for compatibility
    private async Task ShowPasswordDialog(string userId, string displayName)
    {
        OpenPasswordChangeModal(userId, displayName);
    }

    private async Task UpdatePassword(string userId, string newPassword)
    {
        // Legacy method - now handled by shared component
        ShowInfoNotification("Please use the password change modal to update passwords.");
    }

    #endregion

    #region Role Assignment

    private async Task ShowRoleDialog(string userId, string displayName, string? currentRoleCode)
    {
        RoleUserCode = userId;
        RoleUserDisplayName = displayName;
        CurrentRoleCode = currentRoleCode ?? "";
        ShowRoleModal = true;
        StateHasChanged();
    }

    private void CloseRoleModal()
    {
        ShowRoleModal = false;
        RoleUserCode = string.Empty;
        RoleUserDisplayName = string.Empty;
        CurrentRoleCode = string.Empty;
        StateHasChanged();
    }

    private async Task AssignRoleFromModal(string? userRoleCode)
    {
        await AssignRole(RoleUserCode, RoleUserDisplayName, userRoleCode);
        CloseRoleModal();
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
            user.UpdatedBy = AuthService.CurrentUserDisplayName;
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
    private readonly List<StatusOption> IsActiveOptions = StatusOptions.ActiveInactiveOptions;

    private readonly List<StatusOption> IsPOPEmployeeOptions = StatusOptions.YesNoOptions;
   
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
            var userGroupsQuery = new GetSMSStakeholderGroupsByUserCodeQuery(userId);
            var userGroupsResult = await Mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ?
                userGroupsResult.Value?.ToList() ?? new List<SMSStakeholderGroup>() :
                new List<SMSStakeholderGroup>();

            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllStakeholderGroups
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
            UserCurrentGroups = new List<SMSStakeholderGroup>();
            AvailableGroups = AllStakeholderGroups.Where(g => g.IsActive).ToList();

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
            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new RemoveUserFromStakeholderGroupCommand(GroupManagementUserCode, groupId);
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
            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new AssignUserToStakeholderGroupCommand(GroupManagementUserCode, groupId);
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
                    var groupId = new SMSStakeholderGroupID(groupCode);
                    var command = new AssignUserToStakeholderGroupCommand(GroupManagementUserCode, groupId);
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

    #region UI Helper Methods

    private async Task ExportUsers()
    {
        ShowInfoNotification("Export functionality will be implemented soon.");
    }

    private BadgeStyle GetStakeholderTypeBadgeStyle(string stakeholderType)
    {
        var stakeholderTypeEnum = SMSStakeholderType.FromValue(stakeholderType);
        return stakeholderTypeEnum?.Value switch
        {
            "SUT-0001" => BadgeStyle.Primary,   // Airline
            "SUT-0002" => BadgeStyle.Success,   // Inspector  
            "SUT-0003" => BadgeStyle.Warning,   // Contractor
            _ => BadgeStyle.Secondary
        };
    }

    private string GetStakeholderTypeDisplay(string stakeholderType)
    {
        var stakeholderTypeEnum = SMSStakeholderType.FromValue(stakeholderType);
        return stakeholderTypeEnum?.Name ?? stakeholderType;
    }

    #endregion

    #region Notification Methods

    private void ShowErrorNotification(string message)
    {
        NotificationHelper.ShowError(NotificationService, message);
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationHelper.ShowSuccess(NotificationService, message);
    }

    private void ShowInfoNotification(string message)
    {
        NotificationHelper.ShowInfo(NotificationService, message);
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
        public bool IsPOPEmployee { get; set; } = false;

        public bool IsActive { get; set; } = false;
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

        public bool IsPOPEmployee { get; set; } = false;
    }

    #endregion
}