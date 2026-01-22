namespace SMS3.Components.Pages.System.UserGroups;

public partial class ApplicationGroups : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationGroups> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;
    [Inject] private DialogService DialogService { get; set; } = default!;

    #endregion

    #region Parameters

    [Parameter] public string? GroupCode { get; set; }
    [Parameter] public string? Action { get; set; } // For handling different actions like "members"

    #endregion

    #region Properties

    private List<SMSApplicationGroup> SMSApplicationGroups { get; set; } = new();
    private List<SMSApplicationUser> SMSApplicationUsers { get; set; } = new();
    private List<SMSApplicationUser> GroupMembers { get; set; } = new();
    private List<SMSApplicationUser> AvailableUsers { get; set; } = new();
    private SMSApplicationGroup? CurrentGroup { get; set; }
    private bool IsEditMode { get; set; }
    private bool IsManagingMembers { get; set; }
    private string? CurrentGroupCode { get; set; }

    // Grid reference
    private RadzenDataGrid<SMSApplicationGroup>? groupsGrid;

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
    private string EditGroupName { get; set; } = string.Empty;
    private string EditDescription { get; set; } = string.Empty;
    private bool EditIsActive { get; set; } = true;
    private string DeleteGroupCode { get; set; } = string.Empty;
    private string DeleteGroupName { get; set; } = string.Empty;

    #endregion

    #region Dropdown Options

    private readonly List<StatusOption> StatusOptions = new()
    {
        new() { Text = "Active", Value = true },
        new() { Text = "Inactive", Value = false }
    };

    #endregion

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
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
            // Load Application Groups
            var groupsQuery = new GetAllSMSApplicationGroupsQuery();
            var groupsResult = await Mediator.SendAsync(groupsQuery, CancellationToken.None);
            SMSApplicationGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() :
                new List<SMSApplicationGroup>();

            // Load Application Users for potential group assignments
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await Mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSApplicationUsers = usersResult.IsSuccess ?
                usersResult.Value?.ToList() ?? new List<SMSApplicationUser>() :
                new List<SMSApplicationUser>();

            Logger.LogInformation("Loaded {GroupCount} application groups and {UserCount} application users",
                SMSApplicationGroups.Count, SMSApplicationUsers.Count);
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
            var getGroupQuery = new GetSMSApplicationGroupByCodeQuery(groupCode);
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
        EditIsActive = true;
        Navigation.NavigateTo("/System/UserGroups/ApplicationGroups");
    }

    private void CloseEditModal()
    {
        ShowEditModal = false;
        CurrentGroup = null;
        EditGroupName = string.Empty;
        EditDescription = string.Empty;
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
            var groupCode = $"AG-0000";
            var groupId = new SMSApplicationGroupID(groupCode);
            var group = new SMSApplicationGroup(groupId)
            {
                Code = groupCode,
                Name = NewGroupName,
                Description = NewDescription,
                IsActive = true
            };

            var command = new CreateSMSApplicationGroupCommand(group);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Application group '{NewGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to create application group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating application group");
            ShowErrorNotification("Error creating application group. Please try again.");
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
            CurrentGroup.IsActive = EditIsActive;

            var updateCommand = new UpdateSMSApplicationGroupCommand(CurrentGroup);
            var result = await Mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification($"Application group '{EditGroupName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();
            }
            else
            {
                ShowErrorNotification(result.Error?.Message ?? "Failed to update application group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error updating application group: {GroupCode}", CurrentGroup.Code);
            ShowErrorNotification("Error updating application group. Please try again.");
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

            var command = new DeleteSMSApplicationGroupCommand(DeleteGroupCode);
            var result = await Mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessNotification("Application group deleted successfully.");
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
                ShowErrorNotification(result.Error?.Message ?? "Failed to delete application group.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting application group: {GroupCode}", DeleteGroupCode);
            ShowErrorNotification("Error deleting application group. Please try again.");
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
        ShowCreateModal = true;
    }

    private void CloseCreateModal()
    {
        ShowCreateModal = false;
        NewGroupName = string.Empty;
        NewDescription = string.Empty;
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
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message
        });
    }

    private void ShowSuccessNotification(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message
        });
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
            CurrentGroup = SMSApplicationGroups.FirstOrDefault(g => g.Code == groupCode);

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
            // Get users in this group using the enhanced repository method
            var groupMembersQuery = new GetUsersByApplicationGroupCodeQuery(groupCode);
            var membersResult = await Mediator.SendAsync(groupMembersQuery, CancellationToken.None);
            GroupMembers = membersResult.IsSuccess ?
                membersResult.Value?.ToList() ?? new List<SMSApplicationUser>() :
                new List<SMSApplicationUser>();

            // Load available users (users not in this group)
            if (!SMSApplicationUsers.Any())
            {
                await LoadDataAsync();
            }

            var memberCodes = GroupMembers.Select(m => m.Code).ToHashSet();
            AvailableUsers = SMSApplicationUsers.Where(u => !memberCodes.Contains(u.Code)).ToList();

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
            GroupMembers = new List<SMSApplicationUser>();
            AvailableUsers = SMSApplicationUsers?.ToList() ?? new List<SMSApplicationUser>();
        }
    }

    private void ExitMemberManagement()
    {
        IsManagingMembers = false;
        CurrentGroupCode = null;
        CurrentGroup = null;
        GroupMembers.Clear();
        AvailableUsers.Clear();
        SelectedUsers.Clear();
        Navigation.NavigateTo("/System/UserGroups/ApplicationGroups");
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
            var command = new RemoveUserFromApplicationGroupCommand(userCode, CurrentGroupCode);
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
                    var command = new AssignUserToApplicationGroupCommand(userCode, CurrentGroupCode);
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
            var command = new AssignUserToApplicationGroupCommand(userCode, CurrentGroupCode);
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

    #region Helper Classes

    public class StatusOption
    {
        public string Text { get; set; } = string.Empty;
        public bool Value { get; set; }
    }

    #endregion
}