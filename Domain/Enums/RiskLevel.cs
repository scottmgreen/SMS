//-----------------------------------------------------------------------
// <copyright file="RiskLevel.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining level classifications for SMS risk assessment.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Risk levels for approval workflow and decision authority
/// Integrates with SMSRole authority levels for proper approval routing
/// ENHANCED: Now includes aviation matrix color support while keeping domain layer clean
/// </summary>
public abstract class RiskLevel : BaseEnum<RiskLevel>
{
    protected RiskLevel(string value, string name, string description, int requiredAuthorityLevel, string[] approverRoles, string backgroundColor, string textColor, string bootstrapClass) : base(value, name)
    {
        Description = description;
        RequiredAuthorityLevel = requiredAuthorityLevel;
        ApproverRoles = approverRoles;
        BackgroundColor = backgroundColor;
        TextColor = textColor;
        BootstrapClass = bootstrapClass;
    }

    public string Description { get; }
    public int RequiredAuthorityLevel { get; }
    public string[] ApproverRoles { get; }
    
    // UI Color Properties (domain-appropriate)
    public string BackgroundColor { get; }
    public string TextColor { get; }
    public string BootstrapClass { get; }

    #region Risk Levels

    /// <summary>Critical risk requiring Accountable Executive approval</summary>
    public static readonly RiskLevel Critical = new CriticalLevel();

    /// <summary>High risk requiring Responsible Executive or higher approval</summary>
    public static readonly RiskLevel High = new HighLevel();

    /// <summary>Medium risk requiring Responsible Manager or higher approval</summary>
    public static readonly RiskLevel Medium = new MediumLevel();

    /// <summary>Low risk requiring SMS Manager or higher approval</summary>
    public static readonly RiskLevel Low = new LowLevel();

    public static readonly RiskLevel Unkonwn = new UnknownLevel();

    #endregion

    #region Implementations

    private sealed class CriticalLevel : RiskLevel
    {
        public CriticalLevel() : base("CRITICAL", "Critical Risk",
            "Critical risk requiring immediate action and Accountable Executive approval", 10,
            new[] { "ACCOUNTABLE_EXECUTIVE" },
            "#dc3545", "#ffffff", "danger") // Red background, white text, Bootstrap danger
        {
        }
    }

    private sealed class HighLevel : RiskLevel
    {
        public HighLevel() : base("HIGH", "High Risk",
            "High risk requiring Responsible Executive or Accountable Executive approval", 9,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE" },
            "#fd7e14", "#ffffff", "warning") // Orange background, white text, Bootstrap warning
        {
        }
    }

    private sealed class MediumLevel : RiskLevel
    {
        public MediumLevel() : base("MEDIUM", "Medium Risk",
            "Medium risk requiring Responsible Manager or higher approval", 8,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER" },
            "#ffc107", "#000000", "warning") // Yellow background, black text, Bootstrap warning
        {
        }
    }

    private sealed class LowLevel : RiskLevel
    {
        public LowLevel() : base("LOW", "Low Risk",
            "Low risk requiring SMS Manager or higher approval", 7,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER", "SMS_MANAGER" },
            "#28a745", "#ffffff", "success") // Green background, white text, Bootstrap success
        {
        }
    }

    private sealed class UnknownLevel : RiskLevel
    {
        public UnknownLevel() : base("UNKNOWN", "Unknown Risk",
            "Unknown risk is the initial", 0,
            new[] { "ACCOUNTABLE_EXECUTIVE", "RESPONSIBLE_EXECUTIVE", "RESPONSIBLE_MANAGER", "SMS_MANAGER" },
            "#6c757d", "#ffffff", "secondary") // Gray background, white text, Bootstrap secondary
        {
        }
    }
    #endregion

    #region Domain-appropriate UI Helper Methods

    /// <summary>
    /// Gets the CSS style string for this risk level
    /// </summary>
    public string GetCssStyle(bool includeBorder = true)
    {
        var style = $"background: {BackgroundColor}; color: {TextColor};";
        if (includeBorder)
        {
            style += " border: 1px solid #000;";
        }
        return style;
    }

    /// <summary>
    /// Gets the inline style for color indicators (like in your legend)
    /// </summary>
    public string GetIndicatorStyle(int width = 20, int height = 16)
    {
        return $"width: {width}px; height: {height}px; background: {BackgroundColor}; border: 1px solid #000;";
    }

    #endregion

    #region Aviation Matrix Color Support (Domain-appropriate)

    /// <summary>
    /// NEW: Get aviation matrix color for specific severity/likelihood combination
    /// This supports the granular color mapping needed for aviation matrices
    /// Domain-appropriate as it provides data, not UI framework specifics
    /// </summary>
    public static string GetAviationMatrixColor(int severity, int likelihood)
    {
        return (severity, likelihood) switch
        {
            (5, 1) => "#ffc107", // Yellow
            (5, 2) => "#fd7e14", // Orange  
            (5, 3) => "#dc3545", // Red
            (5, 4) => "#dc3545", // Red
            (5, 5) => "#dc3545", // Red
            (4, 1) => "#28a745", // Green
            (4, 2) => "#ffc107", // Yellow
            (4, 3) => "#fd7e14", // Orange
            (4, 4) => "#dc3545", // Red
            (4, 5) => "#dc3545", // Red
            (3, 1) => "#28a745", // Green
            (3, 2) => "#ffc107", // Yellow
            (3, 3) => "#ffc107", // Yellow
            (3, 4) => "#fd7e14", // Orange
            (3, 5) => "#dc3545", // Red
            (2, 1) => "#28a745", // Green
            (2, 2) => "#28a745", // Green
            (2, 3) => "#ffc107", // Yellow
            (2, 4) => "#ffc107", // Yellow
            (2, 5) => "#fd7e14", // Orange
            (1, 1) => "#28a745", // Green
            (1, 2) => "#28a745", // Green
            (1, 3) => "#28a745", // Green
            (1, 4) => "#28a745", // Green
            (1, 5) => "#ffc107", // Yellow
            _ => "#f8f9fa"       // Default gray
        };
    }

    /// <summary>
    /// NEW: Get aviation matrix text color for specific severity/likelihood combination
    /// Domain-appropriate helper for text color determination
    /// </summary>
    public static string GetAviationMatrixTextColor(int severity, int likelihood)
    {
        var backgroundColor = GetAviationMatrixColor(severity, likelihood);
        return IsLightColor(backgroundColor) ? "#000000" : "#ffffff";
    }

    /// <summary>
    /// NEW: Get CSS style for aviation matrix cells
    /// Domain-appropriate as it provides CSS data without UI framework dependencies
    /// </summary>
    public static string GetAviationMatrixCellStyle(int severity, int likelihood)
    {
        var backgroundColor = GetAviationMatrixColor(severity, likelihood);
        var textColor = GetAviationMatrixTextColor(severity, likelihood);
        return $"background: {backgroundColor}; color: {textColor};";
    }

    /// <summary>
    /// Helper method to determine if a color is light (needs black text) or dark (needs white text)
    /// </summary>
    private static bool IsLightColor(string hexColor)
    {
        if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#"))
            return false;

        try
        {
            var hex = hexColor[1..];
            if (hex.Length != 6)
                return false;

            var r = Convert.ToInt32(hex[0..2], 16);
            var g = Convert.ToInt32(hex[2..4], 16);
            var b = Convert.ToInt32(hex[4..6], 16);

            var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            return luminance > 0.5;
        }
        catch
        {
            return false; // Default to dark if parsing fails
        }
    }

    #endregion

    #region Core Enum Methods

    /// <summary>
    /// Gets all available risk levels
    /// </summary>
    public static IEnumerable<RiskLevel> GetAllValues()
    {
        return typeof(RiskLevel)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(RiskLevel))
            .Select(f => (RiskLevel)f.GetValue(null)!)
            .Where(rl => rl != null);
    }

    /// <summary>
    /// Gets risk levels that can be approved by the given authority level
    /// </summary>
    public static IEnumerable<RiskLevel> GetApprovableRiskLevels(int authorityLevel)
    {
        return GetAllValues().Where(rl => rl.RequiredAuthorityLevel <= authorityLevel);
    }

    /// <summary>
    /// Gets risk levels ordered by severity (Critical -> Low)
    /// </summary>
    public static IEnumerable<RiskLevel> GetOrderedBySeverity()
    {
        return new[] { Critical, High, Medium, Low };
    }

    /// <summary>
    /// Checks if this is a critical risk level
    /// </summary>
    public bool IsCritical => RequiredAuthorityLevel >= 10;

    /// <summary>
    /// Checks if this risk requires executive approval
    /// </summary>
    public bool RequiresExecutiveApproval => RequiredAuthorityLevel >= 9;

    /// <summary>
    /// Gets the escalation risk level (next higher level)
    /// </summary>
    public RiskLevel? GetEscalationLevel()
    {
        return this switch
        {
            _ when this == Low => Medium,
            _ when this == Medium => High,
            _ when this == High => Critical,
            _ when this == Critical => null,
            _ => null
        };
    }

    #endregion
}

