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
    
    protected override async Task OnInitializedAsync()
    {
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
                Name = "Implementation Plan",
                Description = "PDX SMS Implementation Plan Signed 7182024",
                FileName = "PDX SMS Implementation Plan Signed 7182024.pdf",
                Category = "policies",
                Type = "Policy",
                Size = "2.1 MB",
                LastModified = DateTime.Now.AddDays(-15),
                Icon = "picture_as_pdf",
                Color = "var(--rz-primary)"
            },
            new()
            {
                Name = "Approval Letter",
                Description = "PDX SMS IP Approval Letter 7182024",
                FileName = "PDX SMS IP Approval Letter 7182024.pdf",
                Category = "policies",
                Type = "Regulation",
                Size = "1.8 MB",
                LastModified = DateTime.Now.AddDays(-30),
                Icon = "picture_as_pdf",
                Color = "var(--rz-info)"
            },
            new()
            {
                Name = "PDX SMS Manual 2025",
                Description = "PDX SMS Manual 2025",
                FileName = "PDX SMS Manual 2025.pdf",
                Category = "policies",
                Type = "Procedure",
                Size = "3.2 MB",
                LastModified = DateTime.Now.AddDays(-7),
                Icon = "picture_as_pdf",
                Color = "var(--rz-warning)"
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

 
    #endregion
}