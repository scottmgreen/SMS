using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Domain.ValueObjects;

using SMS_Shared.Common;
using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;
using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSSystem.UserGroups;

public partial class ApplicationGroups : ComponentBase
{
    #region Dependency Injection

    [Inject] private IMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<ApplicationGroups> _logger { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private INotificationHelper _notificationHelper { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;

    [Inject] private ICurrentUserService CurrentUserService { get; set; } = default!;

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

    private readonly List<StatusOption> ActiveInactiveStatusOptions = StatusOptions.ActiveInactiveOptions;

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
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            SMSApplicationGroups = groupsResult.IsSuccess ?
                groupsResult.Value?.ToList() ?? new List<SMSApplicationGroup>() :
                new List<SMSApplicationGroup>();

            // Load Application Users for potential group assignments
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            SMSApplicationUsers = usersResult.IsSuccess ?
                usersResult.Value?.ToList() ?? new List<SMSApplicationUser>() :
                new List<SMSApplicationUser>();

            _logger.LogInformation("Loaded {GroupCount} application groups and {UserCount} application users",
                SMSApplicationGroups.Count, SMSApplicationUsers.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            ShowErrorAsyncNotification("Error loading data. Please try again.");
        }
    }

    #endregion

    #region Edit Operations

    private async Task EditGroup(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ShowErrorAsyncNotification("Group code is required.");
            return;
        }

        try
        {
            var getGroupQuery = new GetSMSApplicationGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);

            if (groupResult.IsFailure)
            {
                ShowErrorAsyncNotification("Group not found.");
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
            _logger.LogError(ex, "Error loading group for edit: {GroupCode}", groupCode);
            ShowErrorAsyncNotification("Error loading group. Please try again.");
        }
    }

    private void CancelEdit()
    {
        IsEditMode = false;
        CurrentGroup = null;
        EditGroupName = string.Empty;
        EditDescription = string.Empty;
        EditIsActive = true;
        _navigation.NavigateToSecure("/System/UserGroups/ApplicationGroups");
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
            ShowErrorAsyncNotification("Group name is required.");
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
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification($"Application group '{NewGroupName}' created successfully.");
                CloseCreateModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to create application group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application group");
            ShowErrorAsyncNotification("Error creating application group. Please try again.");
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
            ShowErrorAsyncNotification("Group name is required.");
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
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification($"Application group '{EditGroupName}' updated successfully.");
                CloseEditModal();
                await LoadDataAsync();
                await groupsGrid?.Reload();
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to update application group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application group: {GroupCode}", CurrentGroup.Code);
            ShowErrorAsyncNotification("Error updating application group. Please try again.");
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
            ShowErrorAsyncNotification("Group code is required for deletion.");
            return;
        }

        try
        {
            IsSaving = true;
            StateHasChanged();

            var command = new DeleteSMSApplicationGroupCommand(DeleteGroupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("Application group deleted successfully.");
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
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to delete application group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application group: {GroupCode}", DeleteGroupCode);
            ShowErrorAsyncNotification("Error deleting application group. Please try again.");
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

    private void ShowErrorAsyncNotification(string message)
    {
        _notificationHelper.ShowErrorAsync(message);
    }

    private void ShowSuccessAsyncNotification(string message)
    {
        _notificationHelper.ShowSuccessAsync(message);
    }

    #endregion

    #region Member Management Operations

    private async Task ManageMembers(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ShowErrorAsyncNotification("Group code is required to manage members.");
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
            _logger.LogError(ex, "Error entering manage members mode for group: {GroupCode}", groupCode);
            ShowErrorAsyncNotification("Error entering manage members mode. Please try again.");
        }
    }

    private async Task LoadGroupMembersAsync(string groupCode)
    {
        try
        {
            // Get users in this group using the enhanced repository method
            var groupMembersQuery = new GetUsersByApplicationGroupCodeQuery(groupCode);
            var membersResult = await _mediator.SendAsync(groupMembersQuery, CancellationToken.None);
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

            _logger.LogInformation("Loaded {MemberCount} group members and {AvailableCount} available users for group {GroupCode}",
                GroupMembers.Count, AvailableUsers.Count, groupCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group members for group: {GroupCode}", groupCode);

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
        _navigation.NavigateToSecure("/System/UserGroups/ApplicationGroups");
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
            ShowErrorAsyncNotification("User code and group code are required.");
            return;
        }

        try
        {
            var command = new RemoveUserFromApplicationGroupCommand(userCode, CurrentGroupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("User removed from group successfully.");
                await LoadGroupMembersAsync(CurrentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to remove user from group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, CurrentGroupCode);
            ShowErrorAsyncNotification("Error removing user from group. Please try again.");
        }
    }

    private async Task AssignMultipleUsers()
    {
        if (string.IsNullOrWhiteSpace(CurrentGroupCode) || !SelectedUsers.Any(s => s.Value))
        {
            ShowErrorAsyncNotification("Group code and at least one user must be selected.");
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
                    var result = await _mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, CurrentGroupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                var message = $"Successfully assigned {successCount} user(s) to group.";
                if (failureCount > 0)
                    message += $" {failureCount} assignment(s) failed.";
                ShowSuccessAsyncNotification(message);

                await LoadGroupMembersAsync(CurrentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                ShowErrorAsyncNotification("Failed to assign users to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning multiple users to group {GroupCode}", CurrentGroupCode);
            ShowErrorAsyncNotification("Error assigning users to group. Please try again.");
        }
    }

    private async Task AssignSingleUser(string userCode)
    {
        if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(CurrentGroupCode))
        {
            ShowErrorAsyncNotification("User code and group code are required.");
            return;
        }

        try
        {
            var command = new AssignUserToApplicationGroupCommand(userCode, CurrentGroupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                ShowSuccessAsyncNotification("User assigned to group successfully.");
                await LoadGroupMembersAsync(CurrentGroupCode);
                StateHasChanged(); // Refresh the modal
            }
            else
            {
                ShowErrorAsyncNotification(result.Error?.Message ?? "Failed to assign user to group.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, CurrentGroupCode);
            ShowErrorAsyncNotification("Error assigning user to group. Please try again.");
        }
    }

    #endregion

    
}
