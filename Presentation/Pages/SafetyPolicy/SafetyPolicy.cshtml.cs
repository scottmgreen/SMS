using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyPolicy;

/// <summary>
/// Safety Policy Management
/// Manage organizational safety policy and commitments
/// </summary>
public class SafetyPolicyModel : PageModel
{
    public SafetyPolicyDocument CurrentPolicy { get; set; } = new();
    public List<PolicyRevision> PolicyHistory { get; set; } = new();
    public PolicyMetrics Metrics { get; set; } = new();

    [BindProperty]
    public UpdatePolicyViewModel PolicyUpdate { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "Safety Policy Management";
        LoadPolicyData();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!ModelState.IsValid)
        {
            LoadPolicyData();
            return Page();
        }

        // TODO: Update safety policy
        TempData["SuccessMessage"] = "Safety policy has been updated and is pending approval.";
        return RedirectToPage();
    }

    private void LoadPolicyData()
    {
        CurrentPolicy = GetCurrentPolicy();
        PolicyHistory = GetPolicyHistory();
        Metrics = GetPolicyMetrics();
    }

    private SafetyPolicyDocument GetCurrentPolicy()
    {
        return new SafetyPolicyDocument
        {
            Version = "2.1",
            EffectiveDate = DateTime.Now.AddMonths(-6),
            LastReviewDate = DateTime.Now.AddDays(-30),
            NextReviewDate = DateTime.Now.AddMonths(6),
            ApprovedBy = "CEO - Portland International Airport",
            Status = "Active",
            Content = "Portland International Airport is committed to the highest standards of aviation safety..."
        };
    }

    private List<PolicyRevision> GetPolicyHistory()
    {
        return new List<PolicyRevision>
        {
            new() { Version = "2.1", Date = DateTime.Now.AddMonths(-6), Description = "Updated emergency response procedures", UpdatedBy = "Safety Manager", Status = "Active" },
            new() { Version = "2.0", Date = DateTime.Now.AddMonths(-12), Description = "Comprehensive policy restructure", UpdatedBy = "SMS Coordinator", Status = "Superseded" },
            new() { Version = "1.5", Date = DateTime.Now.AddMonths(-18), Description = "Added wildlife management section", UpdatedBy = "Safety Manager", Status = "Superseded" }
        };
    }

    private PolicyMetrics GetPolicyMetrics()
    {
        return new PolicyMetrics
        {
            TotalRevisions = 8,
            AverageReviewCycle = 6,
            LastDistributionDate = DateTime.Now.AddDays(-30),
            AcknowledgmentRate = 98,
            ComplianceRating = 96
        };
    }
}

public class SafetyPolicyDocument
{
    public string Version { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime LastReviewDate { get; set; }
    public DateTime NextReviewDate { get; set; }
    public string ApprovedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class PolicyRevision
{
    public string Version { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class PolicyMetrics
{
    public int TotalRevisions { get; set; }
    public int AverageReviewCycle { get; set; }
    public DateTime LastDistributionDate { get; set; }
    public int AcknowledgmentRate { get; set; }
    public int ComplianceRating { get; set; }
}

public class UpdatePolicyViewModel
{
    public string UpdateReason { get; set; } = string.Empty;
    public string UpdateDescription { get; set; } = string.Empty;
    public string UpdatedContent { get; set; } = string.Empty;
    public bool RequireReapproval { get; set; } = true;
    public string DistributionList { get; set; } = string.Empty;
}
