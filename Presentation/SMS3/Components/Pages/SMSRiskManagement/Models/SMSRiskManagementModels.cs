using Microsoft.AspNetCore.Components.Forms;

namespace SMS3.Components.Pages.SMSRiskManagement.Models;

/// <summary>
/// Shared data models for SMS Risk Management pages
/// </summary>
public static class SMSRiskManagementModels
{
    // This file contains shared model classes used by HazardReporting and ConfidentialReporting pages
}

#region Core Shared Models

/// <summary>
/// Form data for hazard report creation - used by both HazardReporting and ConfidentialReporting
/// </summary>
public class HazardReportForm
{
    public string? HazardCategory { get; set; }
    public string? HazardType { get; set; }
    public string? ReportType { get; set; }
    public string? UrgencyLevel { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime ReportedOn { get; set; } = DateTime.Now;
    public string? ReportingDepartment { get; set; }
    public string? NextAction { get; set; }
    public bool IsConfidential { get; set; }
}

/// <summary>
/// Geographic location data for map integration
/// </summary>
public class GeoLocationData
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime SelectedDateTime { get; set; } = DateTime.UtcNow;
    public bool IsValid => Latitude != 0 && Longitude != 0;
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
public class DropdownOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    
    public DropdownOption() { }
    public DropdownOption(string value, string text)
    {
        Value = value;
        Text = text;
    }
}

#endregion

#region Confidential Reporting Specific Models

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

#endregion