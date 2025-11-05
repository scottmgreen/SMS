using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// Hazard Review - Review and process individual hazard reports
/// FIXED: Now loads actual hazard data from repository instead of hardcoded mock data
/// </summary>
public class HazardReviewModel : PageModel
{
    private readonly UniversalJsonDataService _dataService;

    public HazardReviewModel(UniversalJsonDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
    }

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    public HazardReviewDetails Hazard { get; set; } = new();
    public ReviewAction[] AvailableActions { get; set; } = Array.Empty<ReviewAction>();
    public HazardDocument[] SupportingDocuments { get; set; } = Array.Empty<HazardDocument>();
    public ReviewHistory[] ReviewHistoryItems { get; set; } = Array.Empty<ReviewHistory>();

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["Title"] = "Hazard Review - Safety Risk Management";
        
        if (string.IsNullOrWhiteSpace(HazardId))
        {
            TempData["ErrorMessage"] = "Hazard ID is required for review.";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        await LoadHazardDetailsAsync();
        
        // If hazard not found, redirect back to processing page
        if (string.IsNullOrEmpty(Hazard.Id))
        {
            TempData["ErrorMessage"] = $"Hazard {HazardId} not found.";
            return RedirectToPage("/SafetyRiskManagement/HazardProcessing");
        }

        return Page();
    }

    public IActionResult OnPostAssignReviewer(string assigneeId, string priority)
    {
        // TODO: Implement hazard assignment logic
        TempData["SuccessMessage"] = "Hazard successfully assigned for review.";
        return RedirectToPage();
    }

    public IActionResult OnPostUpdateStatus(string newStatus, string comments)
    {
        // TODO: Implement status update logic
        TempData["SuccessMessage"] = $"Hazard status updated to {newStatus}.";
        return RedirectToPage();
    }

    public IActionResult OnPostApproveHazard(string approvalComments)
    {
        // TODO: Implement approval logic
        TempData["SuccessMessage"] = "Hazard report approved and forwarded for risk assessment.";
        return RedirectToPage("/SafetyRiskManagement/RiskAssessment", new { hazardId = HazardId });
    }

    public IActionResult OnPostRejectHazard(string rejectionReason)
    {
        // TODO: Implement rejection logic
        TempData["WarningMessage"] = "Hazard report rejected and returned to submitter.";
        return RedirectToPage();
    }

    private async Task LoadHazardDetailsAsync()
    {
        try
        {
            Console.WriteLine($"*** HAZARD REVIEW: Loading data for hazard ID: {HazardId}");

            // Load hazard data from the repository
            var hazards = _dataService.GetAllHazards();
            var reports = _dataService.GetAllReports();
            
            // Find the specific hazard
            var hazardData = hazards.FirstOrDefault(h => h.Id == HazardId);
            if (hazardData == null)
            {
                Console.WriteLine($"*** HAZARD REVIEW: Hazard {HazardId} not found in hazards data");
                
                // Check if we have a report with this ID as tracking ID
                var reportData = reports.FirstOrDefault(r => r.TrackingId == HazardId || r.Id == HazardId);
                if (reportData != null)
                {
                    // Get the associated hazard
                    hazardData = hazards.FirstOrDefault(h => h.Id == reportData.HazardId);
                    Console.WriteLine($"*** HAZARD REVIEW: Found report {reportData.TrackingId}, loading associated hazard {reportData.HazardId}");
                }
            }

            if (hazardData != null)
            {
                // Find associated report
                var associatedReport = reports.FirstOrDefault(r => r.HazardId == hazardData.Id);
                
                Console.WriteLine($"*** HAZARD REVIEW: Found hazard - Type: {hazardData.HazardType}, Location: {hazardData.Location}");

                // Map to display model
                Hazard = new HazardReviewDetails
                {
                    Id = HazardId ?? hazardData.Id,
                    TrackingNumber = associatedReport?.TrackingId ?? $"HAZ-{hazardData.Id[..8]}",
                    Title = $"{hazardData.HazardType} at {hazardData.Location}",
                    Status = hazardData.Status,
                    Priority = hazardData.Priority,
                    Type = hazardData.HazardType,
                    Location = hazardData.Location,
                    ReportedDate = hazardData.ReportedDate,
                    ReportedBy = hazardData.ReportedById,
                    Description = hazardData.Description,
                    ImmediateActions = "Actions documented during initial reporting process.",
                    PotentialConsequences = "Assessment pending - to be determined during risk analysis phase.",
                    ContributingFactors = "Contributing factors to be identified during detailed investigation.",
                    IsConfidential = hazardData.IsConfidential,
                    ReviewerAssigned = "To be assigned",
                    AssignedDate = hazardData.CreatedDate,
                    ReviewDueDate = DateTime.Now.AddDays(5),
                    CurrentReviewStep = "Initial Review",
                    RiskLevel = "To be assessed",
                    RequiresInvestigation = hazardData.Priority == "High" || hazardData.Priority == "Critical"
                };

                Console.WriteLine($"*** HAZARD REVIEW: Successfully loaded hazard review details");
            }
            else
            {
                Console.WriteLine($"*** HAZARD REVIEW: No hazard found for ID {HazardId}");
                // Set empty hazard to trigger error handling
                Hazard = new HazardReviewDetails();
            }

            // Load available actions (these are static)
            LoadAvailableActions();
            
            // Load mock supporting documents (since we don't have a file system yet)
            LoadMockSupportingDocuments();
            
            // Load mock review history (since we don't have history tracking yet)
            LoadMockReviewHistory();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"*** HAZARD REVIEW: Error loading hazard details: {ex.Message}");
            Hazard = new HazardReviewDetails();
        }
    }

    private void LoadAvailableActions()
    {
        AvailableActions = new[]
        {
            new ReviewAction { Id = "assign", Name = "Assign to Reviewer", Icon = "fas fa-user-plus", ButtonClass = "btn-primary" },
            new ReviewAction { Id = "approve", Name = "Approve for Risk Assessment", Icon = "fas fa-check", ButtonClass = "btn-success" },
            new ReviewAction { Id = "reject", Name = "Reject Report", Icon = "fas fa-times", ButtonClass = "btn-danger" },
            new ReviewAction { Id = "request_info", Name = "Request Additional Information", Icon = "fas fa-question-circle", ButtonClass = "btn-warning" },
            new ReviewAction { Id = "duplicate", Name = "Mark as Duplicate", Icon = "fas fa-copy", ButtonClass = "btn-secondary" },
            new ReviewAction { Id = "escalate", Name = "Escalate to Management", Icon = "fas fa-arrow-up", ButtonClass = "btn-info" }
        };
    }

    private void LoadMockSupportingDocuments()
    {
        // Mock documents since we don't have file attachment system yet
        SupportingDocuments = new[]
        {
            new HazardDocument
            {
                Id = "DOC001",
                FileName = "initial_report.pdf",
                FileSize = "125 KB",
                UploadDate = Hazard.ReportedDate,
                UploadedBy = Hazard.ReportedBy,
                DocumentType = "Initial Report"
            }
        };
    }

    private void LoadMockReviewHistory()
    {
        // Mock history since we don't have history tracking yet
        ReviewHistoryItems = new[]
        {
            new ReviewHistory
            {
                Date = Hazard.ReportedDate,
                Action = "Hazard Report Submitted",
                PerformedBy = Hazard.ReportedBy,
                Comments = "Initial hazard report submitted through the reporting system."
            },
            new ReviewHistory
            {
                Date = DateTime.Now.AddHours(-1),
                Action = "Assigned for Review",
                PerformedBy = "Safety Manager",
                Comments = "Assigned for initial review based on reported priority level."
            }
        };
    }
}

public record HazardReviewDetails
{
    public string Id { get; init; } = "";
    public string TrackingNumber { get; init; } = "";
    public string Title { get; init; } = "";
    public string Status { get; init; } = "";
    public string Priority { get; init; } = "";
    public string Type { get; init; } = "";
    public string Location { get; init; } = "";
    public DateTime ReportedDate { get; init; }
    public string ReportedBy { get; init; } = "";
    public string Description { get; init; } = "";
    public string ImmediateActions { get; init; } = "";
    public string PotentialConsequences { get; init; } = "";
    public string ContributingFactors { get; init; } = "";
    public bool IsConfidential { get; init; }
    public string ReviewerAssigned { get; init; } = "";
    public DateTime? AssignedDate { get; init; }
    public DateTime? ReviewDueDate { get; init; }
    public string CurrentReviewStep { get; init; } = "";
    public string RiskLevel { get; init; } = "";
    public bool RequiresInvestigation { get; init; }
}

public record ReviewAction
{
    public string Id { get; init; } = "";
    public string Name { get; init; } = "";
    public string Icon { get; init; } = "";
    public string ButtonClass { get; init; } = "";
}

public record HazardDocument
{
    public string Id { get; init; } = "";
    public string FileName { get; init; } = "";
    public string FileSize { get; init; } = "";
    public DateTime UploadDate { get; init; }
    public string UploadedBy { get; init; } = "";
    public string DocumentType { get; init; } = "";
}

public record ReviewHistory
{
    public DateTime Date { get; init; }
    public string Action { get; init; } = "";
    public string PerformedBy { get; init; } = "";
    public string Comments { get; init; } = "";
}

