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
/// Application Group Management - Full CRUD for SMS Application Groups
/// </summary>
public class ApplicationGroupsModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<ApplicationGroupsModel> _logger;

    public ApplicationGroupsModel(IMediator mediator, ILogger<ApplicationGroupsModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties

    public IEnumerable<SMSApplicationGroup> ApplicationGroups { get; private set; } = new List<SMSApplicationGroup>();
    public IEnumerable<SMSApplicationUser> ApplicationUsers { get; private set; } = new List<SMSApplicationUser>();
    public SMSApplicationGroup? CurrentGroup { get; set; }
    public bool IsEditMode { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData] 
    public string? ErrorMessage { get; set; }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync(string? action = null)
    {
        await LoadDataAsync();
        
        if (action == "create")
        {
            IsEditMode = false;
            CurrentGroup = null;
        }
        
        return Page();
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
            var getGroupQuery = new GetSMSApplicationGroupByCodeQuery(code);
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
            var groupCode = $"AG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var groupId = new SMSApplicationGroupID(groupCode);
            var group = new SMSApplicationGroup(groupId)
            {
                Code = groupCode,
                Name = groupName,
                Description = description,
                IsActive = true
            };

            var command = new CreateSMSApplicationGroupCommand(group);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Application group '{groupName}' created successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to create application group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating application group");
            ErrorMessage = "Error creating application group. Please try again.";
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

            var getGroupQuery = new GetSMSApplicationGroupByCodeQuery(groupCode);
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

            var updateCommand = new UpdateSMSApplicationGroupCommand(group);
            var result = await _mediator.SendAsync(updateCommand, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = $"Application group '{groupName}' updated successfully.";
                return RedirectToPage();
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to update application group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating application group: {GroupCode}", groupCode);
            ErrorMessage = "Error updating application group. Please try again.";
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

            var command = new DeleteSMSApplicationGroupCommand(groupCode);
            var result = await _mediator.SendAsync(command, CancellationToken.None);

            if (result.IsSuccess)
            {
                SuccessMessage = "Application group deleted successfully.";
            }
            else
            {
                ErrorMessage = result.Error?.Message ?? "Failed to delete application group.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting application group: {GroupCode}", groupCode);
            ErrorMessage = "Error deleting application group. Please try again.";
        }

        return RedirectToPage();
    }

    #endregion

    #region Helper Methods

    private async Task LoadDataAsync()
    {
        try
        {
            // Load Application Groups
            var groupsQuery = new GetAllSMSApplicationGroupsQuery();
            var groupsResult = await _mediator.SendAsync(groupsQuery, CancellationToken.None);
            ApplicationGroups = groupsResult.IsSuccess ? groupsResult.Value ?? new List<SMSApplicationGroup>() : new List<SMSApplicationGroup>();

            // Load Application Users for potential group assignments
            var usersQuery = new GetAllSMSApplicationUsersQuery();
            var usersResult = await _mediator.SendAsync(usersQuery, CancellationToken.None);
            ApplicationUsers = usersResult.IsSuccess ? usersResult.Value ?? new List<SMSApplicationUser>() : new List<SMSApplicationUser>();

            _logger.LogInformation("Loaded {GroupCount} application groups and {UserCount} application users", 
                ApplicationGroups.Count(), ApplicationUsers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data");
            throw;
        }
    }

    #endregion
}