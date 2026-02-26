//-----------------------------------------------------------------------
// <copyright file="InterviewStatus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enumeration defining valid status values for SMS interview workflows.
//                  Domain enumeration defining valid states and classifications
//                  for business entities and processes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;

namespace SMS_Domain.Enums;

/// <summary>
/// Interview Status Smart Enumeration - COMPLETELY REPLACED
/// Represents the approved final interview workflow statuses
/// Based on approved final status list from StatusList.txt
/// </summary>
public sealed class InterviewStatus : BaseEnum<InterviewStatus>
{
    #region ✅ APPROVED FINAL INTERVIEW STATUS VALUES FROM StatusList.txt

    /// <summary>Interviewee has been identified for the interview</summary>
    public static readonly InterviewStatus IntervieweeIdentified = new("INTERVIEWEE_IDENTIFIED", "Interviewee Identified");

    /// <summary>Interview has been scheduled with specific date and time</summary>
    public static readonly InterviewStatus InterviewScheduled = new("INTERVIEW_SCHEDULED", "Interview Scheduled");

    /// <summary>Interview has been completed successfully</summary>
    public static readonly InterviewStatus InterviewComplete = new("INTERVIEW_COMPLETE", "Interview Complete");

    /// <summary>Interview could not be conducted due to various reasons</summary>
    public static readonly InterviewStatus UnableToConduct = new("UNABLE_TO_CONDUCT", "Unable to Conduct");

    /// <summary>Interview could not be conducted due to various reasons</summary>
    public static readonly InterviewStatus InterviewInProgress = new("INTERVIEW_INPROGRESS", "Interview In Progress");

    public static readonly InterviewStatus InterviewCanceled = new("INTERVIEW_CANCELED", "Interview Canceled");
    #endregion

    private InterviewStatus(string value, string name) : base(value, name)
    {
    }

    /// <summary>
    /// Get all interview statuses that are considered active
    /// </summary>
    public static IEnumerable<InterviewStatus> GetActiveStatuses()
    {
        return new[] { IntervieweeIdentified, InterviewScheduled };
    }

    /// <summary>
    /// Get all interview statuses that are considered final
    /// </summary>
    public static IEnumerable<InterviewStatus> GetFinalStatuses()
    {
        return new[] { InterviewComplete, UnableToConduct };
    }

    /// <summary>
    /// Get all interview statuses that allow modifications
    /// </summary>
    public static IEnumerable<InterviewStatus> GetModifiableStatuses()
    {
        return new[] { IntervieweeIdentified, InterviewScheduled };
    }

    /// <summary>
    /// Check if this status allows modifications
    /// </summary>
    public bool AllowsModifications()
    {
        return this == IntervieweeIdentified || this == InterviewScheduled;
    }

    

    /// <summary>
    /// Check if this status indicates interview is scheduled
    /// </summary>
    public bool IsScheduled()
    {
        return this == InterviewScheduled;
    }

    /// <summary>
    /// Check if this status indicates the interview is complete
    /// </summary>
    public bool IsComplete()
    {
        return this == InterviewComplete;
    }

    /// <summary>
    /// Check if this status indicates interview could not be conducted
    /// </summary>
    public bool IsUnableToConduct()
    {
        return this == UnableToConduct;
    }

    

    /// <summary>
    /// Check if this status allows the interview to be conducted
    /// </summary>
    public bool CanConduct()
    {
        return this == InterviewScheduled;
    }

    /// <summary>
    /// Check if this status allows the interview to be marked as unable to conduct
    /// </summary>
    public bool CanMarkUnableToConduct()
    {
        return this == IntervieweeIdentified || this == InterviewScheduled;
    }

    /// <summary>
    /// Get the next possible statuses from this status
    /// </summary>
    public IEnumerable<InterviewStatus> GetPossibleNextStatuses()
    {
        return Value switch
        {
            "INTERVIEWEE_IDENTIFIED" => new[] { InterviewScheduled, UnableToConduct },
            "INTERVIEW_SCHEDULED" => new[] { InterviewComplete, UnableToConduct },
            "INTERVIEW_COMPLETE" => new InterviewStatus[] { }, // Final state
            "UNABLE_TO_CONDUCT" => new InterviewStatus[] { }, // Final state
            _ => new InterviewStatus[] { }
        };
    }

    

    

    

    /// <summary>
    /// Determines if this status is an active processing state
    /// </summary>
    public bool IsActiveStatus => this == IntervieweeIdentified || this == InterviewScheduled;

    /// <summary>
    /// Gets the UI color for this status
    /// </summary>
    public string GetDisplayColor()
    {
        return this switch
        {
            var s when s == IntervieweeIdentified => "#17a2b8", // Info blue
            var s when s == InterviewScheduled => "#ffc107", // Warning yellow
            var s when s == InterviewComplete => "#28a745", // Success green
            var s when s == UnableToConduct => "#dc3545", // Danger red
            _ => "#6c757d"
        };
    }

    /// <summary>
    /// Gets the status description for workflow display
    /// </summary>
    public string GetWorkflowDescription()
    {
        return this switch
        {
            var s when s == IntervieweeIdentified => "Interviewee identified, ready to schedule interview",
            var s when s == InterviewScheduled => "Interview scheduled, ready to conduct",
            var s when s == InterviewComplete => "Interview completed successfully",
            var s when s == UnableToConduct => "Interview could not be conducted",
            _ => Name
        };
    }

    /// <summary>
    /// Get display description for this status
    /// </summary>
    public string GetDisplayDescription()
    {
        return Value switch
        {
            "INTERVIEWEE_IDENTIFIED" => "Potential interviewee has been identified for the investigation",
            "INTERVIEW_SCHEDULED" => "Interview has been scheduled with confirmed date and time",
            "INTERVIEW_COMPLETE" => "Interview has been completed with documented findings",
            "UNABLE_TO_CONDUCT" => "Interview could not be conducted due to availability or other constraints",
            _ => Name
        };
    }

    /// <summary>
    /// Get progress percentage for this status
    /// </summary>
    public int GetProgressPercentage()
    {
        return Value switch
        {
            "INTERVIEWEE_IDENTIFIED" => 25,
            "INTERVIEW_SCHEDULED" => 50,
            "INTERVIEW_COMPLETE" => 100,
            "UNABLE_TO_CONDUCT" => 0,
            _ => 0
        };
    }

    /// <summary>
    /// Check if this status represents an interview that can be rescheduled
    /// </summary>
    public bool CanBeRescheduled()
    {
        return this == InterviewScheduled;
    }

    /// <summary>
    /// Check if this status represents an active interview workflow state
    /// </summary>
    public bool IsActiveWorkflow()
    {
        return this == IntervieweeIdentified || this == InterviewScheduled;
    }
}
