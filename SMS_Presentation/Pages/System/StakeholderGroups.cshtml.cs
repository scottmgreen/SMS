using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;

namespace SMS_Presentation.Pages.System;

/// <summary>
/// Stakeholder Group Management - Full CRUD for SMS Stakeholder Groups
/// </summary>
public class StakeholderGroupsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<StakeholderGroupsModel> _logger;

    public StakeholderGroupsModel(IMediator mediator, ILogger<StakeholderGroupsModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties

    public IEnumerable<SMSStakeholderGroup> StakeholderGroups { get; private set; } = new List<SMSStakeholderGroup>();
    public IEnumerable<SMSStakeholderUser> StakeholderUsers { get; private set; } = new List<SMSStakeholderUser>();
    public IEnumerable<SMSStakeholderUser> GroupMembers { get; private set; } = new List<SMSStakeholderUser>();
    public IEnumerable<SMSStakeholderUser> AvailableUsers { get; private set; } = new List<SMSStakeholderUser>();
    public SMSStakeholderGroup? CurrentGroup { get; set; }
    public bool IsEditMode { get; set; }
    public bool IsManagingMembers { get; set; }
    public string? CurrentGroupCode { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData] 
    public string? ErrorMessage { get; set; }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync(string? action = null, string? groupCode = null)
    {
        await LoadDataAsync();
        
        if (action == "create")
        {
            IsEditMode = false;
            CurrentGroup = null;
        }
        else if (action == "members" && !string.IsNullOrWhiteSpace(groupCode))
        {
            await LoadGroupMembersAsync(groupCode);
            IsManagingMembers = true;
            CurrentGroupCode = groupCode;
            
            // Load current group info
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsSuccess)
            {
                CurrentGroup = groupResult.Value;
            }
        }
        
        return Page();
    }

    public async Task<IActionResult> OnGetMembersAsync(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            ErrorMessage = "Group code is required.";
            return RedirectToPage();
        }

        try
        {
            await LoadDataAsync();
            await LoadGroupMembersAsync(groupCode);
            IsManagingMembers = true;
            CurrentGroupCode = groupCode;
            
            // Load current group info
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsSuccess)
            {
                CurrentGroup = groupResult.Value;
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group members: {GroupCode}", groupCode);
            ErrorMessage = "Error loading group members. Please try again.";
            return RedirectToPage();
        }
    }

    public async Task<IActionResult> OnGetEditAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            ErrorMessage = "Group code is required.";
            return RedirectToPage();
        }

        try
        {
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(code);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsFailure)
            {
                ErrorMessage = "Group not found.";
                return RedirectToPage();
            }

            CurrentGroup = groupResult.Value;
            IsEditMode = true;
            await LoadDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group for edit: {GroupCode}", code);
            ErrorMessage = "Error loading group. Please try again.";
            return RedirectToPage();
        }
    }

    #endregion

    #region Create/Update Handlers

    public async Task<IActionResult> OnPostCreateAsync(
        string groupName,
        string? description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                ErrorMessage = "Group name is required.";
                await LoadDataAsync();
                return Page();
            }

            // Create group entity
            var groupCode = $"SG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var groupId = new SMSStakeholderGroupID(groupCode);
            var group = new SMSStakeholderGroup(groupId)
            {
                Code = groupCode,
                Name = groupName,
                Description = description,
                IsActive = true
            };

            var command = new CreateSMSStakeholderGroupCommand(group);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Stakeholder group '{groupName}' created successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to create stakeholder group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stakeholder group");
            ErrorMessage = "Error creating stakeholder group. Please try again.";
        }

        await LoadDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(
        string groupCode,
        string groupName,
        string? description,
        bool isActive)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(groupName))
            {
                ErrorMessage = "Group code and name are required.";
                return await OnGetEditAsync(groupCode);
            }

            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsFailure)
            {
                ErrorMessage = "Group not found.";
                return RedirectToPage();
            }

            var group = groupResult.Value;
            group.Name = groupName;
            group.Description = description;
            group.IsActive = isActive;

            var updateCommand = new UpdateSMSStakeholderGroupCommand(group);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Stakeholder group '{groupName}' updated successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update stakeholder group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stakeholder group: {GroupCode}", groupCode);
            ErrorMessage = "Error updating stakeholder group. Please try again.";
        }

        return await OnGetEditAsync(groupCode);
    }

    #endregion

    #region Delete Handler

    public async Task<IActionResult> OnPostDeleteAsync(string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                ErrorMessage = "Group code is required for deletion.";
                return RedirectToPage();
            }

            // Get existing group to pass to delete command
            var getGroupQuery = new GetSMSStakeholderGroupByCodeQuery(groupCode);
            var groupResult = await _mediator.SendAsync(getGroupQuery, CancellationToken.None);
            
            if (groupResult.IsFailure)
            {
                ErrorMessage = "Group not found.";
                return RedirectToPage();
            }

            var deleteCommand = new DeleteSMSStakeholderGroupCommand(groupResult.Value);
            var result = await _mediator.SendAsync(deleteCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "Stakeholder group deleted successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to delete stakeholder group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stakeholder group: {GroupCode}", groupCode);
            ErrorMessage = "Error deleting stakeholder group. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Manage Members

    public async Task<IActionResult> OnPostManageMembersAsync(string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                ErrorMessage = "Group code is required to manage members.";
                return RedirectToPage();
            }

            CurrentGroupCode = groupCode;
            IsManagingMembers = true;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error entering manage members mode for group: {GroupCode}", groupCode);
            ErrorMessage = "Error entering manage members mode. Please try again.";
        }

        return Page();
    }

    #endregion

    #region Group Membership Handlers

    public async Task<IActionResult> OnPostAssignUserAsync(string userCode, string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                ErrorMessage = "User code and group code are required.";
                return RedirectToPage("StakeholderGroups", new { action = "members", groupCode });
            }

            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new AssignUserToStakeholderGroupCommand(userCode, groupId, "SYSTEM");
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "User assigned to group successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to assign user to group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
            ErrorMessage = "Error assigning user to group. Please try again.";
        }

        return RedirectToPage("StakeholderGroups", new { action = "members", groupCode });
    }

    public async Task<IActionResult> OnPostRemoveUserAsync(string userCode, string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                ErrorMessage = "User code and group code are required.";
                return RedirectToPage("StakeholderGroups", new { action = "members", groupCode });
            }

            var groupId = new SMSStakeholderGroupID(groupCode);
            var command = new RemoveUserFromStakeholderGroupCommand(userCode, groupId);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "User removed from group successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to remove user from group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user {UserCode} from group {GroupCode}", userCode, groupCode);
            ErrorMessage = "Error removing user from group. Please try again.";
        }

        return RedirectToPage("StakeholderGroups", new { action = "members", groupCode });
    }

    public async Task<IActionResult> OnPostAssignMultipleUsersAsync(string groupCode, string[] selectedUsers)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode) || selectedUsers?.Length == 0)
            {
                ErrorMessage = "Group code and at least one user must be selected.";
                return RedirectToPage("StakeholderGroups", new { action = "members", groupCode });
            }

            int successCount = 0;
            int failureCount = 0;

            foreach (var userCode in selectedUsers)
            {
                try
                {
                    var groupId = new SMSStakeholderGroupID(groupCode);
                    var command = new AssignUserToStakeholderGroupCommand(userCode, groupId, "SYSTEM");
                    var result = await _mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                        successCount++;
                    else
                        failureCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assigning user {UserCode} to group {GroupCode}", userCode, groupCode);
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                SuccessMessage = $"Successfully assigned {successCount} user(s) to group.";
                if (failureCount > 0)
                    SuccessMessage += $" {failureCount} assignment(s) failed.";
            }
            else
            {
                ErrorMessage = "Failed to assign any users to group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning multiple users to group {GroupCode}", groupCode);
            ErrorMessage = "Error assigning users to group. Please try again.";
        }

        return RedirectToPage("StakeholderGroups", new { action = "members", groupCode });
    }

    #endregion

    #region Default Groups

    public async Task<IActionResult> OnPostCreateDefaultGroupsAsync()
    {
        try
        {
            var defaultGroups = new[]
            {
                new { Name = "Airlines", Description = "Commercial airline operators at PDX" },
                new { Name = "Ground Handlers", Description = "Ground handling service providers" },
                new { Name = "Contractors", Description = "Airport contractors and service providers" },
                new { Name = "Cargo Operations", Description = "Cargo handling and logistics providers" },
                new { Name = "Fixed Base Operators", Description = "FBO and general aviation services" },
                new { Name = "Security Providers", Description = "Security and screening service providers" },
                new { Name = "Maintenance Providers", Description = "Aircraft and facility maintenance providers" }
            };

            int createdCount = 0;
            foreach (var defaultGroup in defaultGroups)
            {
                try
                {
                    var groupCode = $"SG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
                    var groupId = new SMSStakeholderGroupID(groupCode);
                    
                    var group = new SMSStakeholderGroup(groupId)
                    {
                        Code = groupCode,
                        Name = defaultGroup.Name,
                        Description = defaultGroup.Description,
                        IsActive = true
                    };

                    var command = new CreateSMSStakeholderGroupCommand(group);
                    var result = await _mediator.SendAsync(command, CancellationToken.None);

                    if (result.IsSuccess)
                    {
                        createdCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create default group {GroupName}: {Error}", 
                            defaultGroup.Name, result.Error?.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error creating default group {GroupName}", defaultGroup.Name);
                }
            }

            SuccessMessage = $"Successfully created {createdCount} default stakeholder groups.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default stakeholder groups");
            ErrorMessage = "Error creating default groups. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Helper Methods

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Stakeholder Groups
            var groupsQuery = new GetAllSMSStakeholderGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            StakeholderGroups = groupsResult.IsSuccess ? groupsResult.Value ?? new List<SMSStakeholderGroup>() : new List<SMSStakeholderGroup>();

            // Load Stakeholder Users for potential group assignments
            var usersQuery = new GetAllSMSStakeholderUsersQuery();
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            StakeholderUsers = usersResult.IsSuccess ? usersResult.Value ?? new List<SMSStakeholderUser>() : new List<SMSStakeholderUser>();

            _logger.LogInformation("Loaded {GroupCount} stakeholder groups and {UserCount} stakeholder users", 
                StakeholderGroups.Count(), StakeholderUsers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            throw;
        }
    }

    private async Task LoadGroupMembersAsync(string groupCode)
    {
        try
        {
            // Use the existing stored procedure to get users by group code
            var membersQuery = new GetSMSStakeholderUsersByGroupCodeQuery(groupCode);
            var membersResult = await _mediator.SendAsync(membersQuery, CancellationToken.None);
            GroupMembers = membersResult.IsSuccess ? membersResult.Value ?? new List<SMSStakeholderUser>() : new List<SMSStakeholderUser>();

            // Load available users (users not in this group)
            if (StakeholderUsers?.Any() != true)
            {
                var usersQuery = new GetAllSMSStakeholderUsersQuery();
                var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
                StakeholderUsers = usersResult.IsSuccess ? usersResult.Value ?? new List<SMSStakeholderUser>() : new List<SMSStakeholderUser>();
            }

            var memberCodes = GroupMembers.Select(m => m.Code).ToHashSet();
            AvailableUsers = StakeholderUsers.Where(u => !memberCodes.Contains(u.Code)).ToList();

            _logger.LogInformation("Loaded {MemberCount} group members and {AvailableCount} available users for group {GroupCode}", 
                GroupMembers.Count(), AvailableUsers.Count(), groupCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading group members for {GroupCode}", groupCode);
            // For now, if the query fails, just load empty collections
            GroupMembers = new List<SMSStakeholderUser>();
            AvailableUsers = StakeholderUsers?.ToList() ?? new List<SMSStakeholderUser>();
        }
    }

    #endregion
}