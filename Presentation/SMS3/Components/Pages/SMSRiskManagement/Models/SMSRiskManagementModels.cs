namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// Shared data models for SMS Risk Management pages
/// </summary>

#region Core Shared Models

/// <summary>
/// Form data for hazard report creation - used by both HazardReporting and ConfidentialReporting
/// </summary>
public class HazardReportForm
{
    public string? HazardCategory { get; set; }
    public string? HazardType { get; set; }
    public string? Location { get; set; }

    public DateTime IncidentDateTime { get; set; } = DateTime.UtcNow;
    public string? SubmittedBy { get; set; }
    public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
    public string? SubmittingDepartment { get; set; }
    public string? SubmittingDepartmentJobFunction { get; set; }
    public string? ReportContactName { get; set; }
    public string? ReportContactCell { get; set; }
    public string? ReportContactEmail { get; set; }

    public string? ReportContactCompany { get; set; }
    public string? Description { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

/// <summary>
/// File attachment information 
/// </summary>
public class AttachedFile
{
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public long FileSizeBytes { get; set; }
    public string SizeDisplay { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Dropdown option for form controls
/// </summary>


#endregion

