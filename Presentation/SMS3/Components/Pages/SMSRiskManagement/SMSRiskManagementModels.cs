using Microsoft.AspNetCore.Components.Forms;

namespace SMS3.Components.Pages.SMSRiskManagement;

/// <summary>
/// Shared data models for SMS Risk Management pages
/// </summary>
public static class SMSRiskManagementModels
{
    // File already contains these classes, but we need to avoid conflicts
    // The existing classes in HazardReporting.razor.cs will be kept
    // New classes for ConfidentialReporting will use different names
}

/// <summary>
/// Form data for confidential report creation
/// </summary>
public class ConfidentialReportForm
{
    public string? ReportType { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string Priority { get; set; } = "Medium";
    public bool IsRetaliation { get; set; }
    public bool IsPersonnelIssue { get; set; }
    public bool IsComplianceViolation { get; set; }
    public string? AdditionalProtection { get; set; }
}

/// <summary>
/// Dropdown option for confidential reporting form controls
/// </summary>
public class ConfidentialDropdownOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    
    public ConfidentialDropdownOption() { }
    public ConfidentialDropdownOption(string value, string text)
    {
        Value = value;
        Text = text;
    }
}

/// <summary>
/// File attachment information for confidential reports
/// </summary>
public class ConfidentialAttachedFile
{
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string SizeDisplay { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public long Size { get; set; }
}