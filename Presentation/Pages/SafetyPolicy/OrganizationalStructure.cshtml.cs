using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS_Domain.Entities.Organization;
using PDXSMS_Domain.Common;
using PDXSMS.UseCases.Queries.Organization;
using PDXSMS.Interfaces;

namespace PDXSMS_Presentation.Pages.SafetyPolicy;

/// <summary>
/// Safety Policy Organizational Structure
/// Display the SMS organizational hierarchy and responsibilities
/// </summary>
public class OrganizationalStructureModel : PageModel
{
    private readonly IRequestHandler<GetSMSOrganizationalRolesQuery, Result<List<SMSOrganizationalRoleDto>>> _getRolesHandler;
    private readonly IRequestHandler<GetSMSTeamsQuery, Result<List<SMSTeamDto>>> _getTeamsHandler;
    private readonly IRequestHandler<GetOrganizationDashboardQuery, Result<OrganizationDashboardDto>> _getDashboardHandler;

    public OrganizationalStructureModel(
        IRequestHandler<GetSMSOrganizationalRolesQuery, Result<List<SMSOrganizationalRoleDto>>> getRolesHandler,
        IRequestHandler<GetSMSTeamsQuery, Result<List<SMSTeamDto>>> getTeamsHandler,
        IRequestHandler<GetOrganizationDashboardQuery, Result<OrganizationDashboardDto>> getDashboardHandler)
    {
        _getRolesHandler = getRolesHandler;
        _getTeamsHandler = getTeamsHandler;
        _getDashboardHandler = getDashboardHandler;
    }

    public List<SMSOrganizationalRoleDto> OrganizationalRoles { get; set; } = new();
    public List<SMSTeamDto> Committees { get; set; } = new();
    public OrganizationDashboardDto Metrics { get; set; } = new();

    public async Task OnGetAsync()
    {
        ViewData["Title"] = "SMS Organizational Structure";
        await LoadOrganizationalData();
    }

    private async Task LoadOrganizationalData()
    {
        // Load organizational roles
        var rolesQuery = new GetSMSOrganizationalRolesQuery { IncludeInactive = false };
        var rolesResult = await _getRolesHandler.HandleAsync(rolesQuery);
        
        if (rolesResult.IsSuccess)
        {
            OrganizationalRoles = rolesResult.Value ?? new List<SMSOrganizationalRoleDto>();
        }

        // Load teams/committees
        var teamsQuery = new GetSMSTeamsQuery { ActiveOnly = true };
        var teamsResult = await _getTeamsHandler.HandleAsync(teamsQuery);
        
        if (teamsResult.IsSuccess)
        {
            Committees = teamsResult.Value ?? new List<SMSTeamDto>();
        }

        // Load metrics
        var metricsQuery = new GetOrganizationDashboardQuery();
        var metricsResult = await _getDashboardHandler.HandleAsync(metricsQuery);
        
        if (metricsResult.IsSuccess)
        {
            Metrics = metricsResult.Value ?? new OrganizationDashboardDto();
        }
    }
}