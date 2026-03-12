using Domain.Entities;
using SMS3.Components.Shared.UIHelpers;
using SMS3.Extensions;

namespace SMS3.Components.Pages.System.UserGroups;

public partial class OrganizationalGroups : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalGroups> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

    #endregion

    #region Parameters

    [Parameter] public string? GroupCode { get; set; }

    #endregion

    #region Properties

    private List<SMSOrganizationalGroup> SMSOrganizationalGroups { get; set; } = new();
    private List<SMSOrganizationalUser> SMSOrganizationalUsers { get; set; } = new();
    private List<SMSOrganizationalUser> GroupMembers { get; set; } = new();
    private List<SMSOrganizationalUser> AvailableUsers { get; set; } = new();
    private SMSOrganizationalGroup? CurrentGroup { get; set; }
    private bool IsEditMode { get; set; }
    private bool IsManagingMembers { get; set; }
    private string? CurrentGroupCode { get; set; }

    // Grid reference
    private RadzenDataGrid<SMSOrganizationalGroup>? groupsGrid;

    // Selection tracking for member management
    private Dictionary<string, bool> SelectedUsers { get; set; } = new();

    private string SuccessMessage { get; set; } = string.Empty;
    private string ErrorMessage { get; set; } = string.Empty;
    private bool IsSaving { get; set; } = false;

    #endregion

    #region Modal Properties

    private bool ShowCreateModal { get; set; } = false;
    private bool ShowEditModal { get; set; } = false;
    private bool ShowMembersModal { get; set; } = false;
    private bool ShowDeleteModal { get; set; } = false;
    private string NewGroupName { get; set; } = string.Empty;
    private string NewDescription { get; set; } = string.Empty;
    private string NewGroupType { get; set; } = string.Empty;
    private string NewAuthorityLevel { get; set; } = string.Empty;
    private string EditGroupName { get; set; } = string.Empty;
    private string EditDescription { get; set; } = string.Empty;
    private string EditGroupType { get; set; } = string.Empty;
    private string EditAuthorityLevel { get; set; } = string.Empty;
    private bool EditIsActive { get; set; } = true;
    private string DeleteGroupCode { get; set; } = string.Empty;
    private string DeleteGroupName { get; set; } = string.Empty;

    #endregion

    #region Dropdown Options

    private List<DropdownOption> GroupTypeOptions { get; set; } = new();
    private List<DropdownOption> AuthorityLevelOptions { get; set; } = new();

    private readonly List<StatusOption> StatusOptions = new()
    {
        new() { Text = "Active", Value = true },
        new() { Text = "Inactive", Value = false }
    };

    

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        InitializeDropdownOptions();
        await LoadDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        // No longer need route parameter handling since we use modals
        await Task.CompletedTask;
    }

    #endregion

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Organizational Groups
            var groupsQuery = new GetAllSMSOrganizationalGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            SMSOrganizationalGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSOrganizationalGroup>() :
                new List<SMSOrganizationalGroup>();

            // Load Organizational Users for potential group assignments
            var usersQuery = new GetAllSMSOrganizationalUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSOrganizationalUsers = usersResult.IsSuccess ?
                usersResult.Value?.ToList() ?? new List<SMSOrganizationalUser>() :
                new List<SMSOrganizationalUser>();

            Logger.LogInformation("Loaded {GroupCount} organizational groups and {UserCount} organizational users",
                SMSOrganizationalGroups.Count, SMSOrganizationalUsers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
            ShowErrorNotification("Error loading data. Please try again.");
        }
    }

    #endregion

    #region Edit Operations

    private async Task EditGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ShowErrorNotification("Group code is required.");
            return;
        }

        try
        {
            var getGroupQuery = new GetSMSOrganizationalGroupByCodeQuery(groupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);

            if (groupResult.IsFailure)
            {
                ShowErrorNotification("Group not found.");
                return;
            }

            CurrentGroup = groupResult.Value;

            // Set edit form values
            EditGroupName = CurrentGroup.Name ?? string.Empty;
            EditDescription = CurrentGroup.Description ?? string.Empty;
            EditGroupType = CurrentGroup.GroupType ?? string.Empty;
            EditAuthorityLevel = CurrentGroup.AuthorityLevel ?? string.Empty;
            EditIsActive = CurrentGroup.IsActive;

            // Open edit modal
            ShowEditModal = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading group for edit: {GroupCode}", groupCode);
            ShowErrorNotification("Error loading group. Please try again.");
        }
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentGroup = null;
        EditGroupName = string.Empty;
        EditDescription = string.Empty;
        EditGroupType = string.Empty;
        EditAuthorityLevel = string.Empty;
        EditIsActive = true;
        Navigation.NavigateToSecure("/System/UserGroups/OrganizationalGroups");
    }

    private void CloseEditModal()
    {
        ShowEditModal = false;
        CurrentGroup = null;
        EditGroupName = string.Empty;
        EditDescription = string.Empty;
        EditGroupType = string.Empty;
        EditAuthorityLevel = string.Empty;
        EditIsActive = true;
    }

    #endregion

    #region CRUD Operations

    private async Task CreateGroup()
    {
        if (string.IsNullOrWhiteSpace(NewGroupName))
        {
            ShowErrorNotification("Group name is required.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Create group entity
            var groupCode = $"OG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var groupId = new SMSOrganizationalGroupID(groupCode);
            var group = new SMSOrganizationalGroup(groupId)
            {
                Code = groupCode,
                Name = NewGroupName,
                Description = NewDescription,
                GroupType = NewGroupType,
                AuthorityLevel = NewAuthorityLevel,
                IsActive = true
            };

            var command = new CreateSMSOrganizationalGroupCommand(group);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Organizational group '{NewGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create organizational group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating organizational group");
            ShowErrorNotification("Error creating organizational group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task UpdateGroup()
    {
        if (CurrentGroup == null || string.IsNullOrWhiteSpace(EditGroupName))
        {
            ShowErrorNotification("Group name is required.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Update group properties
            CurrentGroup.Name = EditGroupName;
            CurrentGroup.Description = EditDescription;
            CurrentGroup.GroupType = EditGroupType;
            CurrentGroup.AuthorityLevel = EditAuthorityLevel;
            CurrentGroup.IsActive = EditIsActive;

            var updateCommand = new UpdateSMSOrganizationalGroupCommand(CurrentGroup);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Organizational group '{EditGroupName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update organizational group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating organizational group: {GroupCode}", CurrentGroup.Code);
            ShowErrorNotification("Error updating organizational group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private async Task DeleteGroup()
    {
        if (string.IsNullOrWhiteSpace(DeleteGroupCode))
        {
            ShowErrorNotification("Group code is required for deletion.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            // Get existing group to pass to delete command
            var getGroupQuery = new GetSMSOrganizationalGroupByCodeQuery(DeleteGroupCode);
            var groupResult = await Mediator.SendAsync(getGroupQuery, CancellationToken.None);

            if (groupResult.IsFailure)
            {
                ShowErrorNotification("Group not found.");
                return;
            }

            var deleteCommand = new DeleteSMSOrganizationalGroupCommand(groupResult.Value);
            var result = await Mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Organizational group deleted successfully.");
                CloseDeleteModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();

                // If we're editing the deleted group, cancel edit mode
                if (CurrentGroup?.Code == DeleteGroupCode)
                {
                    CancelEdit();
                }
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete organizational group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting organizational group: {GroupCode}", DeleteGroupCode);
            ShowErrorNotification("Error deleting organizational group. Please try again.");
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    #endregion

    #region Modal Operations

    private void OpenCreateModal()
    {
        NewGroupName = string.Empty;
        NewDescription = string.Empty;
        NewGroupType = string.Empty;
        NewAuthorityLevel = string.Empty;
        ShowCreateModal = true;
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewGroupName = string.Empty;
        NewDescription = string.Empty;
        NewGroupType = string.Empty;
        NewAuthorityLevel = string.Empty;
    }

    private void ConfirmDelete(string groupCode, string groupName)
    {
        DeleteGroupCode = groupCode;
        DeleteGroupName = groupName;
        ShowDeleteModal = true;
    }

    private void CloseDeleteModal()
    {
        ShowDeleteModal = false;
        DeleteGroupCode = string.Empty;
        DeleteGroupName = string.Empty;
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

    #endregion

    #region Member Management Operations

    private async Task ManageMembers(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ShowErrorNotification("Group code is required to manage members.");
            return;
        }

        try
        {
            CurrentGroupCode = groupCode;
            IsManagingMembers = true;

            // Find the current group
            CurrentGroup = SMSOrganizationalGroups.FirstOrDefault(g => g.Code == groupCode);

            await LoadGroupMembersAsync(groupCode);

            // Show modal instead of navigating
            ShowMembersModal = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error entering manage members mode for group: {GroupCode}", groupCode);
            ShowErrorNotification("Error entering manage members mode. Please try again.");
        }
    }

    private async Task LoadGroupMembersAsync(string groupCode)
    {
        try
        {
            // Get users in this group
            var groupMembersQuery = new GetUsersByOrganizationalGroupCodeQuery(groupCode);
            var membersResult = await Mediator.SendAsync(groupMembersQuery, CancellationToken.None);
            GroupMembers = membersResult.IsSuccess ?
                membersResult.Value?.ToList() ?? new List<SMSOrganizationalUser>() :
                new List<SMSOrganizationalUser>();

            // Load available users (users not in this group)
            if (!SMSOrganizationalUsers.Any())
            {
                await LoadDataAsync();
            }

            var memberCodes = GroupMembers.Select(m => m.Code).ToHashSet();
            AvailableUsers = SMSOrganizationalUsers.Where(u => !memberCodes.Contains(u.Code)).ToList();

            // Initialize selection tracking
            SelectedUsers.Clear();
            foreach (var user in AvailableUsers)
            {
                SelectedUsers[user.Code] = false;
            }

            Logger.LogInformation("Loaded {MemberCount} group members and {AvailableCount} available users for group {GroupCode}",
                GroupMembers.Count, AvailableUsers.Count, groupCode);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading group members for group: {GroupCode}", groupCode);

            // For now, if the query fails, just load empty collections
            GroupMembers = new List<SMSOrganizationalUser>();
            AvailableUsers = SMSOrganizationalUsers?.ToList() ?? new List<SMSOrganizationalUser>();
        }
    }

    private void ExitMemberManagement()
    {
        // Reset member management state
        IsManagingMembers = false;
        CurrentGroupCode = null;
        CurrentGroup = null;
        
        Logger.LogInformation("Exited member management view");
        NotificationHelper.ShowInfo(NotificationService, "Returned to group management", 3000);
        
        Navigation.NavigateToSecure("/System/UserGroups/OrganizationalGroups");
    }

    private void CloseMembersModal()
    {
        ShowMembersModal = false;
        IsManagingMembers = false;
        CurrentGroupCode = null;
        CurrentGroup = null;
        GroupMembers.Clear();
        AvailableUsers.Clear();
        SelectedUsers.Clear();
    }

    private async Task RemoveUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(CurrentGroupCode))
        {
            ShowErrorNotification("User code and group code are required.");
            return;
        }

        try
        {
            var groupId = new SMSOrganizationalGroupID(CurrentGroupCode);
            var command = new RemoveUserFromOrganizationalGroupCommand(userCode, groupId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("User removed from group successfully.");
                await LoadGroupMembersAsync(CurrentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, CurrentGroupCode);
            ShowErrorNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignMultipleUsers()
    {
        if (string.IsNullOrWhiteSpace(CurrentGroupCode) || !SelectedUsers.Any(s => s.Value))
        {
            ShowErrorNotification("Group code and at least one user must be selected.");
            return;
        }

        try
        {
            var selectedUserCodes = SelectedUsers.Where(s => s.Value).Select(s => s.Key).ToArray();
            int successCount = 0;
            int failureCount = 0;

            foreach (var userCode in selectedUserCodes)
            {
                try
                {
                    var groupId = new SMSOrganizationalGroupID(CurrentGroupCode);
                    var command = new AssignUserToOrganizationalGroupCommand(userCode, groupId);
                    var result = await Mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, CurrentGroupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned {successCount} user(s) to group.";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                ShowSuccessNotification(message);

                await LoadGroupMembersAsync(CurrentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                ShowErrorNotification("Failed to assign users to group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning multiple users to group {GroupCode}", CurrentGroupCode);
            ShowErrorNotification("Error assigning users to group. Please try again.");
        }
    }

    private async Task AssignSingleUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(CurrentGroupCode))
        {
            ShowErrorNotification("User code and group code are required.");
            return;
        }

        try
        {
            var groupId = new SMSOrganizationalGroupID(CurrentGroupCode);
            var command = new AssignUserToOrganizationalGroupCommand(userCode, groupId);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("User assigned to group successfully.");
                await LoadGroupMembersAsync(CurrentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, CurrentGroupCode);
            ShowErrorNotification("Error assigning user to group. Please try again.");
        }
    }

    #endregion

    #region Dropdown Initialization

    private void InitializeDropdownOptions()
    {
        // Initialize group type options
        GroupTypeOptions = new List<DropdownOption>
        {
            new("DEPARTMENT", "Department"),
            new("COMMITTEE", "Committee"),
            new("TEAM", "Team"),
            new("DIVISION", "Division"),
            new("EXECUTIVE", "Executive"),
            new("FUNCTIONAL", "Functional Group")
        };

        // Initialize authority level options
        AuthorityLevelOptions = new List<DropdownOption>
        {
            new("EXECUTIVE", "Executive Level"),
            new("SENIOR", "Senior Management"),
            new("MIDDLE", "Middle Management"),
            new("SUPERVISOR", "Supervisory"),
            new("OPERATIONAL", "Operational"),
            new("ADVISORY", "Advisory Only")
        };
    }

    #endregion
}