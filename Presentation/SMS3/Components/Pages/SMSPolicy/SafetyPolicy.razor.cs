using Microsoft.JSInterop;

namespace SMS3.Components.Pages.SMSPolicy;

public partial class SafetyPolicy : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private ILogger<SafetyPolicy> Logger { get; set; } = default!;

    // Policy document management properties
    private SafetyPolicyDocument CurrentPolicy { get; set; } = new();
    private List<PolicyRevision> PolicyHistory { get; set; } = new();
    private PolicyMetrics Metrics { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        LoadPolicyData();
        await base.OnInitializedAsync();
    }

    #region Document Actions

    private async Task ViewDocument(string filename, string category)
    {
        try
        {
            var documentUrl = $"/Documents/Viewer?file={Uri.EscapeDataString(filename)}&category={Uri.EscapeDataString(category)}";

            // Log document access for analytics
            Logger.LogInformation("Document viewer opened: {Filename} in category {Category}", filename, category);

            // Navigate to document viewer
            Navigation.NavigateTo(documentUrl);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error opening document viewer for {Filename}", filename);
        }
    }

    private async Task DownloadDocument(string documentPath)
    {
        try
        {
            var downloadUrl = $"/documents/{documentPath}";

            // Log download activity
            Logger.LogInformation("Document download initiated: {DocumentPath}", documentPath);

            // Trigger download using JavaScript
            await JSRuntime.InvokeVoidAsync("open", downloadUrl, "_blank");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error downloading document: {DocumentPath}", documentPath);
        }
    }

    #endregion

    #region Navigation

    private void NavigateToPage(string url)
    {
        Navigation.NavigateTo(url);
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