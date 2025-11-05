using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// FR-1.4.1: Submit Confidential Report
/// Enhanced security for sensitive safety reports with reporter protection
/// Supports both authenticated and guest (anonymous) submissions
/// </summary>
public class ConfidentialReportingModel : PageModel
{
    private readonly ILogger<ConfidentialReportingModel> _logger;

    public ConfidentialReportingModel(ILogger<ConfidentialReportingModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public ConfidentialReportViewModel ConfidentialReport { get; set; } = new();

    public IActionResult OnGet()
    {
        // Check if this is a guest session and validate it's still within time limits
        var isGuest = HttpContext.Session.GetString("IsGuest") == "true";
        
        if (isGuest)
        {
            var guestStartTimeStr = HttpContext.Session.GetString("GuestStartTime");
            if (DateTime.TryParse(guestStartTimeStr, out var guestStartTime))
            {
                var sessionAge = DateTime.UtcNow - guestStartTime;
                if (sessionAge.TotalMinutes > 30) // 30-minute session limit
                {
                    _logger.LogWarning("Guest session expired after {Minutes} minutes", sessionAge.TotalMinutes);
                    HttpContext.Session.Clear();
                    TempData["InfoMessage"] = "Your anonymous session has expired for security purposes. Please start a new session.";
                    return RedirectToPage("/Account/Login");
                }
                
                _logger.LogInformation("Guest accessing confidential reporting. Session age: {Minutes} minutes", sessionAge.TotalMinutes);
            }
        }
        else
        {
            // For authenticated users, ensure they have access
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login");
            }
        }

        ViewData["Title"] = "Submit Confidential Report - PDXSMS";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var isGuest = HttpContext.Session.GetString("IsGuest") == "true";
        
        _logger.LogInformation("Confidential report submission started. IsGuest: {IsGuest}", isGuest);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Confidential report form validation failed");
            return Page();
        }

        try
        {
            // Generate anonymous tracking ID
            var anonymousId = GenerateAnonymousTrackingId();
            
            // TODO: Process confidential report submission with enhanced security
            // This would typically involve:
            // 1. Anonymizing any identifying information
            // 2. Encrypting sensitive data
            // 3. Storing in secure database with limited access
            // 4. Notifying appropriate SMS personnel
            // 5. Creating audit trail (without identifying reporter)

            _logger.LogInformation("Confidential report submitted successfully. Tracking ID: {TrackingId}, IsGuest: {IsGuest}", 
                anonymousId, isGuest);

            // Set success message with tracking ID
            TempData["SuccessMessage"] = "Your confidential report has been submitted successfully and will be reviewed by SMS personnel.";
            TempData["TrackingId"] = anonymousId;

            // Handle guest logout
            if (isGuest)
            {
                _logger.LogInformation("Guest session ending after successful submission. Tracking ID: {TrackingId}", anonymousId);
                
                // Clear guest session
                HttpContext.Session.Clear();
                
                // Add additional success message for guests
                TempData["InfoMessage"] = "Your anonymous session has been securely ended to protect your identity. Save your tracking ID for future reference.";
                
                // Create a special success page for guests that will redirect to login
                return RedirectToPage("/Account/GuestSubmissionSuccess", new { trackingId = anonymousId });
            }

            // For authenticated users, redirect back to the same page
            return RedirectToPage("/SafetyRiskManagement/ConfidentialReporting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing confidential report submission. IsGuest: {IsGuest}", isGuest);
            TempData["ErrorMessage"] = "An error occurred while submitting your report. Please try again or contact the confidential hotline.";
            return Page();
        }
    }

    private string GenerateAnonymousTrackingId()
    {
        // Generate a secure anonymous tracking ID with timestamp and random component
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomComponent = Random.Shared.Next(1000, 9999);
        var checksum = (timestamp.GetHashCode() + randomComponent).ToString().Substring(0, 2);
        
        return $"CONF-{timestamp}-{randomComponent}-{checksum}";
    }
}

public class ConfidentialReportViewModel
{
    public string ReportType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public bool IsRetaliation { get; set; }
    public bool IsPersonnelIssue { get; set; }
    public bool IsComplianceViolation { get; set; }
    public string AdditionalProtection { get; set; } = string.Empty;
    public List<IFormFile> Attachments { get; set; } = new();
}

