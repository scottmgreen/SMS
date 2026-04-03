using Microsoft.JSInterop;

using SMS3.Configuration.Extensions;

namespace SMS3.Components.Pages.SMSPolicy;

public partial class SafetyPolicy : ComponentBase
{
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IJSRuntime _jsRuntime { get; set; } = default!;
    [Inject] private ILogger<SafetyPolicy> _logger { get; set; } = default!;

    // Document management properties
    private List<PolicyDocument> PolicyDocuments { get; set; } = new();
    private SafetyPolicyDocument CurrentPolicy { get; set; } = new();
    private List<PolicyRevision> PolicyHistory { get; set; } = new();
    private PolicyMetrics Metrics { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        LoadPolicyData();
        LoadPolicyDocuments();
        await base.OnInitializedAsync();
    }

    #region Document Management

    private void LoadPolicyDocuments()
    {
        PolicyDocuments = new List<PolicyDocument>
        {
            new()
            {
                Name = "SMS Policy Overview",
                Description = "14 CFR Part 139 - Comprehensive safety management system overview",
                FileName = "14 CFR Part 139.pdf",
                Category = "policies",
                Type = "Policy",
                Size = "2.1 MB",
                LastModified = DateTime.Now.AddDays(-15),
                Icon = "picture_as_pdf",
                Color = "var(--rz-primary)"
            },
            new()
            {
                Name = "Regulatory Compliance",
                Description = "FAA Final Rule - Regulatory compliance requirements and procedures",
                FileName = "FAA Final Rule.pdf",
                Category = "policies",
                Type = "Regulation",
                Size = "1.8 MB",
                LastModified = DateTime.Now.AddDays(-30),
                Icon = "picture_as_pdf",
                Color = "var(--rz-info)"
            },
            new()
            {
                Name = "Emergency Procedures",
                Description = "Emergency response procedures and crisis management protocols",
                FileName = "part-139-cert-alert-23-02-SMS-rule.pdf",
                Category = "policies",
                Type = "Procedure",
                Size = "3.2 MB",
                LastModified = DateTime.Now.AddDays(-7),
                Icon = "picture_as_pdf",
                Color = "var(--rz-warning)"
            },
            new()
            {
                Name = "Safety Training Manual",
                Description = "Safety training procedures and educational materials for staff",
                FileName = "safety-training-manual.pdf",
                Category = "manuals",
                Type = "Manual",
                Size = "4.5 MB",
                LastModified = DateTime.Now.AddDays(-45),
                Icon = "picture_as_pdf",
                Color = "var(--rz-success)"
            }
        };
    }

    #endregion

    #region Document Actions

    private async Task ViewDocument(string filename, string category)
    {
        try
        {
            // Construct the direct path to the PDF file in wwwroot
            var documentUrl = $"/documents/{category}/{filename}";

            // Log document access for analytics
            _logger.LogInformation("Opening PDF document in new tab: {Filename} from category {Category}", filename, category);

            // Open PDF in new tab using JavaScript
            await _jsRuntime.InvokeVoidAsync("window.open", documentUrl, "_blank");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening PDF document {Filename} in new tab", filename);
            // Optionally show a user-friendly error message
            await _jsRuntime.InvokeVoidAsync("alert", $"Error opening document: {filename}");
        }
    }

    private async Task DownloadDocument(string documentPath)
    {
        try
        {
            var downloadUrl = $"/documents/{documentPath}";

            // Log download activity
            _logger.LogInformation("Document download initiated: {DocumentPath}", documentPath);

            // Trigger download using JavaScript
            await _jsRuntime.InvokeVoidAsync("window.open", downloadUrl, "_blank");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document: {DocumentPath}", documentPath);
        }
    }

    #endregion

    #region _navigation

    private void NavigateToPage(string url)
    {
        // ?? SECURE NAVIGATION - Navigate with encrypted URL
        _navigation.NavigateToSecure(url);
    }

    #endregion

    #region Data Management

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

    #endregion

    #region Models

    public class PolicyDocument
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public string Icon { get; set; } = "picture_as_pdf";
        public string Color { get; set; } = "var(--rz-primary)";
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

    #endregion
}