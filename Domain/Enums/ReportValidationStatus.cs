//-----------------------------------------------------------------------
// <copyright file="ReportValidationStatus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid status values for SMS reportvalidation workflows.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Enums;

public abstract class ReportValidationStatus : BaseEnum<ReportValidationStatus>
{
    protected ReportValidationStatus(string value, string name) : base(value, name)
    {

    }



    #region Validation Decision Types

    /// <summary>Report constitutes an SMS risk and requires formal risk assessment</summary>
    public static readonly ReportValidationStatus ValidationNeeded = new NeedsValidationStatus();

    /// <summary>Report does not constitute an SMS risk and should be referred or closed</summary>
    public static readonly ReportValidationStatus ValidationComplete = new ValidationCompletedStatus();

    /// <summary>Report does not constitute an SMS risk and should be referred or closed</summary>
    public static readonly ReportValidationStatus Revised = new ValidationRevisedStatus();

    #endregion

    #region Implementations

    private sealed class NeedsValidationStatus : ReportValidationStatus
    {
        public NeedsValidationStatus() : base("NEEDS_VALIDATION", "NEEDS_VALIDATION")
        {
        }
    }

    private sealed class ValidationCompletedStatus : ReportValidationStatus
    {
        public ValidationCompletedStatus() : base("VALIDATION_COMPLETED", "VALIDATION_COMPLETED")
        {
        }
    }
    private sealed class ValidationRevisedStatus : ReportValidationStatus
    {
        public ValidationRevisedStatus() : base("VALIDATION_REVISED", "VALIDATION_REVISED")
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available validation decision values
    /// </summary>
    public static IEnumerable<ReportValidationStatus> GetAllValues()
    {
        return typeof(ReportValidationStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(ReportValidationStatus))
            .Select(f => (ReportValidationStatus)f.GetValue(null)!)
            .Where(vd => vd != null);
    }






    /// <summary>
    /// Parse a string value to ValidationDecision
    /// </summary>
    public static ReportValidationStatus FromValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Validation decision value cannot be null or empty", nameof(value));

        return GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Invalid validation decision value: {value}", nameof(value));
    }

    /// <summary>
    /// Try to parse a string value to ValidationDecision
    /// </summary>
    public static bool TryFromValue(string? value, out ReportValidationStatus? validationDecision)
    {
        validationDecision = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        validationDecision = GetAllValues().FirstOrDefault(vd => vd.Value.Equals(value, StringComparison.OrdinalIgnoreCase));
        return validationDecision != null;
    }
}

