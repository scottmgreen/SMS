using Domain.Entities;

namespace SMS3.Components.Pages.System.UserManagement;

public partial class OrganizationalUsers : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalUsers> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    #endregion

    #region Properties

    private List<SMSOrganizationalUser> OrganizationalUsersList { get; set; } = new();
    private SMSOrganizationalUser? CurrentUser { get; set; }
    private bool IsSaving { get; set; } = false;

    // Grid reference
    private RadzenDataGrid<SMSOrganizationalUser>? usersGrid;

    private string SuccessMessage { get; set; } = string.Empty;
    private string ErrorMessage { get; set; } = string.Empty;

    #endregion

    #region Modal Properties

    private bool ShowCreateModal { get; set; } = false;
    private bool ShowEditModal { get; set; } = false;
    private bool ShowPasswordModal { get; set; } = false;
    private bool ShowDeleteModal { get; set; } = false;
    private bool ShowGroupsModal { get; set; } = false;

    // Create form fields
    private string NewFirstName { get; set; } = string.Empty;
    private string NewLastName { get; set; } = string.Empty;
    private string NewUserName { get; set; } = string.Empty;
    private string NewPassword { get; set; } = string.Empty;
    private string NewDepartment { get; set; } = string.Empty;
    private string NewPosition { get; set; } = string.Empty;
    private string NewOrganizationLevel { get; set; } = string.Empty;
    private string NewSMSRole { get; set; } = string.Empty;

    // Edit form fields
    private string EditFirstName { get; set; } = string.Empty;
    private string EditLastName { get; set; } = string.Empty;
    private string EditDepartment { get; set; } = string.Empty;
    private string EditPosition { get; set; } = string.Empty;
    private string EditOrganizationLevel { get; set; } = string.Empty;
    private bool EditIsActive { get; set; } = true;

    // Password change fields
    private string PasswordUserId { get; set; } = string.Empty;
    private string PasswordUserDisplayName { get; set; } = string.Empty;
    private string ConfirmPassword { get; set; } = string.Empty;
    private string PasswordValidationMessage { get; set; } = string.Empty;

    // Password Modal Properties for Shared Component
    private string PasswordUserCode { get; set; } = string.Empty;

    // Delete confirmation fields
    private string DeleteUserId { get; set; } = string.Empty;
    private string DeleteUserDisplayName { get; set; } = string.Empty;

    // Group Management Properties
    private string GroupManagementUserCode { get; set; } = string.Empty;
    private string GroupManagementUserDisplayName { get; set; } = string.Empty;
    private List<SMSOrganizationalGroup> AllOrganizationalGroups { get; set; } = new();
    private List<SMSOrganizationalGroup> UserCurrentGroups { get; set; } = new();
    private List<SMSOrganizationalGroup> AvailableGroups { get; set; } = new();
    private Dictionary<string, bool> SelectedGroups { get; set; } = new();

    #endregion

    #region Dropdown Options

    private readonly List<StatusOption> StatusOptions = new()
    {
        new() { Text = "Active", Value = true },
        new() { Text = "Inactive", Value = false }
    };

    private readonly List<DropdownOption> DepartmentOptions = new()
    {
        new() { Text = "Operations", Value = "Operations" },
        new() { Text = "Safety", Value = "Safety" },
        new() { Text = "Security", Value = "Security" },
        new() { Text = "Maintenance", Value = "Maintenance" },
        new() { Text = "Administration", Value = "Administration" },
        new() { Text = "Finance", Value = "Finance" },
        new() { Text = "IT", Value = "IT" },
        new() { Text = "Human Resources", Value = "Human Resources" },
        new() { Text = "Facilities", Value = "Facilities" },
        new() { Text = "Emergency Response", Value = "Emergency Response" },
        new() { Text = "Quality Assurance", Value = "Quality Assurance" },
        new() { Text = "Training", Value = "Training" },
        new() { Text = "Communications", Value = "Communications" },
        new() { Text = "Environmental", Value = "Environmental" },
        new() { Text = "Legal", Value = "Legal" },
        new() { Text = "Planning", Value = "Planning" },
        new() { Text = "Engineering", Value = "Engineering" },
        new() { Text = "Customer Service", Value = "Customer Service" },
        new() { Text = "Ground Services", Value = "Ground Services" },
        new() { Text = "Management", Value = "Management" }
    };

    // Updated to use SMSOrganizationalLevel enum with category grouping
    private List<DropdownOption> OrganizationLevelOptions
    {
        get
        {
            var options = new List<DropdownOption>();

            // Group by category and show them in order of authority
            var categories = new[] { "Executive", "Management", "Operational", "Committee", "External" };

            foreach (var category in categories)
            {
                var categoryRoles = SMSOrganizationalLevel.GetLevelsByCategory(category)
                    .OrderByDescending(level => level.AuthorityLevel);

                if (categoryRoles.Any())
                {
                    // Add category header (disabled option)
                    options.Add(new DropdownOption
                    {
                        Text = $"--- {category} Roles ---",
                        Value = "",
                        IsDisabled = true
                    });

                    // Add roles in category
                    foreach (var level in categoryRoles)
                    {
                        options.Add(new DropdownOption
                        {
                            Text = $"  {level.Name} (Authority {level.AuthorityLevel})",
                            Value = level.Name
                        });
                    }
                }
            }

            return options;
        }
    }

    private readonly List<DropdownOption> SMSRoleOptions = new()
    {
        new() { Text = "SMS Manager", Value = "SMS Manager" },
        new() { Text = "Safety Manager", Value = "Safety Manager" },
        new() { Text = "Quality Assurance Manager", Value = "Quality Assurance Manager" },
        new() { Text = "Operations Manager", Value = "Operations Manager" },
        new() { Text = "SMS Coordinator", Value = "SMS Coordinator" },
        new() { Text = "Safety Officer", Value = "Safety Officer" },
        new() { Text = "Investigator", Value = "Investigator" },
        new() { Text = "Analyst", Value = "Analyst" }
    };

    #endregion

    #region Form Validation Properties

    private bool IsCreateFormValid =>
        !string.IsNullOrWhiteSpace(NewFirstName) &&
        !string.IsNullOrWhiteSpace(NewLastName) &&
        !string.IsNullOrWhiteSpace(NewUserName) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(NewDepartment) &&
        IsValidOrganizationLevel(NewOrganizationLevel);

    private bool IsEditFormValid =>
        !string.IsNullOrWhiteSpace(EditFirstName) &&
        !string.IsNullOrWhiteSpace(EditLastName) &&
        !string.IsNullOrWhiteSpace(EditDepartment) &&
        IsValidOrganizationLevel(EditOrganizationLevel);

    private bool IsPasswordFormValid =>
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        NewPassword == ConfirmPassword &&
        NewPassword.Length >= 8;

    // Helper method to validate organization level
    private bool IsValidOrganizationLevel(string organizationLevel)
    {
        if (string.IsNullOrWhiteSpace(organizationLevel)) return true; // Optional field

        return SMSOrganizationalLevel.GetAllValues()
            .Any(level => level.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #endregion

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

            // Load Organizational Groups for group management
            var groupsQuery = new GetAllSMSOrganizationalGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            AllOrganizationalGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSOrganizationalGroup>() :
                new List<SMSOrganizationalGroup>();

            Logger.LogInformation("Loaded {UserCount} organizational users and {GroupCount} organizational groups",
                OrganizationalUsersList.Count, AllOrganizationalGroups.Count);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading organizational users data");
            ShowErrorNotification("Error loading data. Please refresh the page.");
        }
    }

    #endregion

    #region Create Operations

    private void OpenCreateModal()
    {
        NewFirstName = string.Empty;
        NewLastName = string.Empty;
        NewUserName = string.Empty;
        NewPassword = string.Empty;
        NewDepartment = string.Empty;
        NewPosition = string.Empty;
        NewOrganizationLevel = string.Empty;
        NewSMSRole = string.Empty;
        ShowCreateModal = true;
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewFirstName = string.Empty;
        NewLastName = string.Empty;
        NewUserName = string.Empty;
        NewPassword = string.Empty;
        NewDepartment = string.Empty;
        NewPosition = string.Empty;
        NewOrganizationLevel = string.Empty;
        NewSMSRole = string.Empty;
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
            var userCode = $"OU-0000";
            var userId = new SMSOrganizationalUserID(userCode);
            var user = new SMSOrganizationalUser(userId)
            {
                Code = userCode,
                FirstName = FirstName.Create(NewFirstName).Value,
                LastName = LastName.Create(NewLastName).Value,
                UserName = UserName.Create(NewUserName).Value,
                Password = Password.Create(NewPassword).Value,
                Department = NewDepartment,
                Position = NewPosition,
                OrganizationLevel = NewOrganizationLevel,
                SMSRole = NewSMSRole,
                IsActive = true
            };

            var command = new CreateSMSOrganizationalUserCommand(user);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Organizational user '{NewFirstName} {NewLastName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await usersGrid?.Reload();
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
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Edit Operations

    private async Task EditUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            ShowErrorNotification("User ID is required.");
            return;
        }

        try
        {
            var getUserQuery = new GetSMSOrganizationalUserByCodeQuery(userId);
            var userResult = await Mediator.SendAsync(getUserQuery, CancellationToken.None);

            if (userResult.IsFailure)
            {
                ShowErrorNotification("User not found.");
                return;
            }

            CurrentUser = userResult.Value;

            // Set edit form values
            EditFirstName = CurrentUser.FirstName?.Value ?? string.Empty;
            EditLastName = CurrentUser.LastName?.Value ?? string.Empty;
            EditDepartment = CurrentUser.Department ?? string.Empty;
            EditPosition = CurrentUser.Position ?? string.Empty;
            EditOrganizationLevel = CurrentUser.OrganizationLevel ?? string.Empty;
            EditIsActive = CurrentUser.IsActive;

            // Open edit modal
            ShowEditModal = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user for edit: {UserId}", userId);
            ShowErrorNotification("Error loading user. Please try again.");
        }
    }

    private void CloseEditModal()
    {
        ShowEditModal = false;
        CurrentUser = null;
        EditFirstName = string.Empty;
        EditLastName = string.Empty;
        EditDepartment = string.Empty;
        EditPosition = string.Empty;
        EditOrganizationLevel = string.Empty;
        EditIsActive = true;
    }

    private async Task UpdateUser()
    {
        if (CurrentUser == null || !IsEditFormValid)
        {
            ShowErrorNotification("Please fill in all required fields.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Update user properties
            CurrentUser.FirstName = FirstName.Create(EditFirstName).Value;
            CurrentUser.LastName = LastName.Create(EditLastName).Value;
            CurrentUser.Department = EditDepartment;
            CurrentUser.Position = EditPosition;
            CurrentUser.OrganizationLevel = EditOrganizationLevel;
            CurrentUser.IsActive = EditIsActive;

            var updateCommand = new UpdateSMSOrganizationalUserCommand(CurrentUser);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Organizational user '{EditFirstName} {EditLastName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
                await usersGrid?.Reload();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update organizational user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating organizational user: {UserId}", CurrentUser.Code);
            ShowErrorNotification("Error updating organizational user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Password Management

    private void OpenPasswordModal(string userId, string displayName)
    {
        PasswordUserCode = userId;
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

    // Legacy methods - kept for compatibility
    private void ClosePasswordModal()
    {
        ClosePasswordChangeModal();
    }

    private async Task UpdatePassword()
    {
        // This method is no longer used with the shared component
        // but kept for compatibility if referenced elsewhere
        ShowErrorNotification("Please use the password change modal to update passwords.");
    }

    #endregion

    #region Delete Operations

    private void ConfirmDelete(string userId, string displayName)
    {
        DeleteUserId = userId;
        DeleteUserDisplayName = displayName;
        ShowDeleteModal = true;
    }

    private void CloseDeleteModal()
    {
        ShowDeleteModal = false;
        DeleteUserId = string.Empty;
        DeleteUserDisplayName = string.Empty;
    }

    private async Task DeleteUser()
    {
        if (string.IsNullOrWhiteSpace(DeleteUserId))
        {
            ShowErrorNotification("User ID is required for deletion.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            var organizationalUserId = new SMSOrganizationalUserID(DeleteUserId);
            var command = new DeleteSMSOrganizationalUserCommand(organizationalUserId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Organizational user deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                await usersGrid?.Reload();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete organizational user.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting organizational user: {UserId}", DeleteUserId);
            ShowErrorNotification("Error deleting organizational user. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
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
            var userGroupsQuery = new GetSMSOrganizationalGroupsByUserCodeQuery(userId);
            var userGroupsResult = await Mediator.SendAsync(userGroupsQuery, CancellationToken.None);
            UserCurrentGroups = userGroupsResult.IsSuccess ?
                userGroupsResult.Value?.ToList() ?? new List<SMSOrganizationalGroup>() :
                new List<SMSOrganizationalGroup>();

            // Calculate available groups (groups the user is not currently in)
            var currentGroupCodes = UserCurrentGroups.Select(g => g.Code).ToHashSet();
            AvailableGroups = AllOrganizationalGroups
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
            UserCurrentGroups = new List<SMSOrganizationalGroup>();
            AvailableGroups = AllOrganizationalGroups.Where(g => g.IsActive).ToList();

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
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new RemoveUserFromOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var command = new AssignUserToOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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
                    var groupId = new SMSOrganizationalGroupID(groupCode);
                    var command = new AssignUserToOrganizationalGroupCommand(GroupManagementUserCode, groupId);
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

    #region Utility Methods

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

    // Add method to get organization level badge style
    private BadgeStyle GetOrganizationLevelBadgeStyle(string organizationLevel)
    {
        var level = SMSOrganizationalLevel.GetAllValues()
            .FirstOrDefault(l => l.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));

        if (level == null) return BadgeStyle.Secondary;

        return level.AuthorityLevel switch
        {
            >= 9 => BadgeStyle.Danger,    // Accountable/Responsible Executive
            8 => BadgeStyle.Warning,      // Responsible Manager
            7 => BadgeStyle.Primary,      // SMS Manager
            6 => BadgeStyle.Info,         // SMS Coordinator
            5 => BadgeStyle.Success,      // SMS Team Member
            _ => BadgeStyle.Secondary
        };
    }

    // Add method to get organization level display text with hierarchy info
    private string GetOrganizationLevelDisplayText(string organizationLevel)
    {
        var level = SMSOrganizationalLevel.GetAllValues()
            .FirstOrDefault(l => l.Name.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));

        if (level == null) return organizationLevel;

        return $"{level.Name} - {level.Category} (Authority Level {level.AuthorityLevel})";
    }

    #endregion

    #region Notification Methods

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

    #region Helper Classes

    public class StatusOption
    {
        public string Text { get; set; } = string.Empty;
        public bool Value { get; set; }
    }

    public class DropdownOption
    {
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsDisabled { get; set; } = false; // Add IsDisabled property
    }

    #endregion
}