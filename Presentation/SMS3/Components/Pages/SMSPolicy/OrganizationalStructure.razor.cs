using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS3.Components.Pages.SMSPolicy;

public partial class OrganizationalStructure : ComponentBase
{
    [Inject] private IMediator Mediator { get; set; } = default!;
    [Inject] private ILogger<OrganizationalStructure> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    // Data Properties
    private List<SMSOrganizationalUser> OrganizationalUsers { get; set; } = new();
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();
    }

    #region Data Loading

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;

            // Load organizational users using existing CQRS
            var organizationalUsersQuery = new GetAllSMSOrganizationalUsersQuery();
            var usersResult = await Mediator.SendAsync(organizationalUsersQuery, CancellationToken.None);
            
            OrganizationalUsers = usersResult.IsSuccess ? 
                usersResult.Value?.ToList() ?? new List<SMSOrganizationalUser>() : 
                new List<SMSOrganizationalUser>();

            Logger.LogInformation("Loaded {UserCount} organizational users for structure display", OrganizationalUsers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading organizational structure data");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    #endregion

    #region SMS Roles Definition

    private List<SMSRole> GetSMSRoles()
    {
        return new List<SMSRole>
        {
            new SMSRole
            {
                Code = "AE",
                Title = "Accountable Executive (AE)",
                Description = "Chief Aviation Officer - Ultimate responsibility for SMS performance and 14 CFR Part 139 Subpart E compliance",
                Level = 1,
                RiskApprovalLevels = new[] { "Critical", "High", "Medium", "Low" },
                Position = "Chief Aviation Officer"
            },
            new SMSRole
            {
                Code = "RE", 
                Title = "Responsible Executive (RE)",
                Description = "Director, Airport Operations - Provides oversight of safety initiatives and strategic improvements",
                Level = 2,
                RiskApprovalLevels = new[] { "High", "Medium", "Low" },
                Position = "Director, Airport Operations"
            },
            new SMSRole
            {
                Code = "RM",
                Title = "Responsible Manager (RM)", 
                Description = "Sr. Manager, PDX Airside Operations - Oversees daily SMS operations at operational level",
                Level = 3,
                RiskApprovalLevels = new[] { "Medium", "Low" },
                Position = "Sr. Manager, PDX Airside Operations"
            },
            new SMSRole
            {
                Code = "SM",
                Title = "SMS Manager",
                Description = "Manages day-to-day SMS processes, hazard reporting system, and safety investigations",
                Level = 4,
                RiskApprovalLevels = new[] { "Low" },
                Position = "SMS Manager"
            },
            new SMSRole
            {
                Code = "SC",
                Title = "SMS Coordinator", 
                Description = "Supports SMS Manager in data collection, analysis, investigations, and training",
                Level = 5,
                RiskApprovalLevels = new string[] { },
                Position = "SMS Coordinator"
            },
            new SMSRole
            {
                Code = "ST",
                Title = "SMS Team",
                Description = "Cross-functional team representing diverse subject matter expertise and airport work groups",
                Level = 6,
                RiskApprovalLevels = new string[] { },
                Position = "SMS Team Member"
            }
        };
    }

    #endregion

    #region Statistics and Status

    private int GetTotalRoles() => GetSMSRoles().Count;

    private int GetFilledRoles()
    {
        return GetSMSRoles().Count(role => GetAssignedUser(role.Code) != null);
    }

    private int GetVacantRoles()
    {
        return GetSMSRoles().Count(role => GetAssignedUser(role.Code) == null);
    }

    private int GetCompliancePercentage()
    {
        var total = GetTotalRoles();
        var filled = GetFilledRoles();
        return total > 0 ? (int)Math.Round((double)filled / total * 100) : 0;
    }

    #endregion

    #region User Assignment Logic

    private SMSOrganizationalUser? GetAssignedUser(string roleCode)
    {
        // Map SMS role codes to organization levels or positions
        return roleCode switch
        {
            "AE" => OrganizationalUsers.FirstOrDefault(u => u.Position?.Contains("Chief Aviation Officer") == true),
            "RE" => OrganizationalUsers.FirstOrDefault(u => u.Position?.Contains("Director") == true && u.Department == "Operations"),
            "RM" => OrganizationalUsers.FirstOrDefault(u => u.Position?.Contains("Sr. Manager") == true && u.Department == "Operations"),
            "SM" => OrganizationalUsers.FirstOrDefault(u => u.Position?.Contains("SMS Manager") == true),
            "SC" => OrganizationalUsers.FirstOrDefault(u => u.Position?.Contains("SMS Coordinator") == true),
            "ST" => OrganizationalUsers.FirstOrDefault(u => u.Position?.Contains("SMS Team") == true),
            _ => null
        };
    }

    #endregion

    #region UI Rendering Helpers

    private RenderFragment RenderRoleCard(SMSRole role)
    {
        return builder =>
        {
            var assignedUser = GetAssignedUser(role.Code);
            var isVacant = assignedUser == null;
            var borderColor = isVacant ? "var(--rz-warning)" : "var(--rz-success)";
            
            builder.AddMarkupContent(0, $@"
                <div class=""rz-card rz-variant-outlined"" style=""border-left: 4px solid {borderColor}; margin-bottom: 1rem;"">
                    <div class=""rz-card-content"" style=""padding: 1rem;"">
                        <div style=""display: flex; justify-content: space-between; align-items: center;"">
                            <div style=""flex: 1;"">
                                <h6 style=""margin: 0; font-weight: 600; margin-bottom: 0.25rem;"">{role.Title}</h6>
                                <p style=""margin: 0; color: var(--rz-text-secondary-color); font-size: 0.875rem;"">{role.Description}</p>
                            </div>
                            <div style=""display: flex; align-items: center; gap: 0.5rem;"">
                                {(assignedUser != null ? 
                                    $@"<img src=""https://www.gravatar.com/avatar/{GenerateEmailHash(assignedUser)}?d=identicon&s=40"" style=""width: 40px; height: 40px; border-radius: 50%;"" alt=""User Avatar"" />
                                       <div>
                                           <div style=""font-weight: 600; margin: 0;"">{assignedUser.DisplayName}</div>
                                           <span class=""rz-badge rz-variant-filled rz-success"" style=""font-size: 0.75em;"">Assigned</span>
                                       </div>" :
                                    @"<i class=""rzi rzi-person-off"" style=""color: var(--rz-warning); font-size: 2rem;""></i>
                                      <span class=""rz-badge rz-variant-filled rz-warning"" style=""font-size: 0.75em;"">Vacant</span>")}
                            </div>
                        </div>
                    </div>
                </div>");
        };
    }

    private string GenerateEmailHash(SMSOrganizationalUser user)
    {
        using var md5 = global::System.Security.Cryptography.MD5.Create();
        var emailBytes = global::System.Text.Encoding.UTF8.GetBytes($"{user.FirstName?.Value?.ToLower()}.{user.LastName?.Value?.ToLower()}@organization.com");
        var hashBytes = md5.ComputeHash(emailBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    private BadgeStyle GetRiskLevelBadgeStyle(string riskLevel)
    {
        return riskLevel switch
        {
            "Critical" => BadgeStyle.Danger,
            "High" => BadgeStyle.Warning,
            "Medium" => BadgeStyle.Info,
            "Low" => BadgeStyle.Success,
            _ => BadgeStyle.Light
        };
    }

    #endregion

    #region Models

    public class SMSRole
    {
        public string Code { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public int Level { get; set; }
        public string[] RiskApprovalLevels { get; set; } = Array.Empty<string>();
        public string Position { get; set; } = "";
    }

    #endregion
}