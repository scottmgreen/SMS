using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Presentation.Pages.System;

/// <summary>
/// User Management Dashboard - Navigation hub for SMS User Management
/// </summary>
public class UserManagementModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<UserManagementModel> _logger;

    public UserManagementModel(IMediator mediator, ILogger<UserManagementModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties

    public int ApplicationUserCount { get; private set; }
    public int StakeholderUserCount { get; private set; }
    public int UserRoleCount { get; private set; }
    public int ApplicationGroupCount { get; private set; }
    public int StakeholderGroupCount { get; private set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData] 
    public string? ErrorMessage { get; set; }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await LoadSummaryDataAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user management dashboard");
            ErrorMessage = "Error loading dashboard data. Please try again.";
            return Page();
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Loads summary data for dashboard display
    /// </summary>
    private async Task LoadSummaryDataAsync()
    {
        try
        {
            // Get counts for dashboard stats
            var appUsersQuery = new GetAllSMSApplicationUsersQuery();
            var appUsersResult = await _mediator.SendAsync(appUsersQuery, CancellationToken.None);
            ApplicationUserCount = appUsersResult.IsSuccess ? appUsersResult.Value?.Count() ?? 0 : 0;

            var stakeholderUsersQuery = new GetAllSMSStakeholderUsersQuery();
            var stakeholderUsersResult = await _mediator.SendAsync(stakeholderUsersQuery, CancellationToken.None);
            StakeholderUserCount = stakeholderUsersResult.IsSuccess ? stakeholderUsersResult.Value?.Count() ?? 0 : 0;

            var userRolesQuery = new GetAllSMSUserRolesQuery();
            var userRolesResult = await _mediator.SendAsync(userRolesQuery, CancellationToken.None);
            UserRoleCount = userRolesResult.IsSuccess ? userRolesResult.Value?.Count() ?? 0 : 0;

            try
            {
                var stakeholderGroupsQuery = new GetAllSMSStakeholderGroupsQuery();
                var stakeholderGroupsResult = await _mediator.SendAsync(stakeholderGroupsQuery, CancellationToken.None);
                StakeholderGroupCount = stakeholderGroupsResult.IsSuccess ? stakeholderGroupsResult.Value?.Count() ?? 0 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load stakeholder groups count");
                StakeholderGroupCount = 0;
            }

            try
            {
                var applicationGroupsQuery = new GetAllSMSApplicationGroupsQuery();
                var applicationGroupsResult = await _mediator.SendAsync(applicationGroupsQuery, CancellationToken.None);
                ApplicationGroupCount = applicationGroupsResult.IsSuccess ? applicationGroupsResult.Value?.Count() ?? 0 : 0;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load application groups count");
                ApplicationGroupCount = 0;
            }

            _logger.LogInformation("Dashboard loaded - App Users: {AppUserCount}, Stakeholder Users: {StakeholderUserCount}, Roles: {UserRoleCount}, App Groups: {ApplicationGroupCount}, Stakeholder Groups: {StakeholderGroupCount}", 
                ApplicationUserCount, StakeholderUserCount, UserRoleCount, ApplicationGroupCount, StakeholderGroupCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading summary data");
            throw;
        }
    }

    #endregion
}