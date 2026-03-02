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
using System.Reflection;

namespace SMS_Domain.Enums;

/// <summary>
/// Interview Status Smart Enumeration - COMPLETELY REWRITTEN
/// Represents the approved final interview workflow statuses
/// Based on approved final status list from StatusList.txt
/// </summary>
public abstract class InterviewStatus : BaseEnum<InterviewStatus>
{
    protected InterviewStatus(string value, string name, string description, bool allowsModification, int workflowOrder) : base(value, name)
    {
        Description = description;
        AllowsModification = allowsModification;
        WorkflowOrder = workflowOrder;
    }

    public string Description { get; }
    public bool AllowsModification { get; }
    public int WorkflowOrder { get; }

    #region ✅ APPROVED FINAL INTERVIEW STATUS VALUES FROM StatusList.txt

    /// <summary>Interviewee has been identified for the interview</summary>
    public static readonly InterviewStatus IntervieweeIdentified = new IntervieweeIdentifiedStatus();

    /// <summary>Interview has been scheduled with specific date and time</summary>
    public static readonly InterviewStatus InterviewScheduled = new InterviewScheduledStatus();

    /// <summary>Interview is currently in progress</summary>
    public static readonly InterviewStatus InterviewInProgress = new InterviewInProgressStatus();

    /// <summary>Interview has been completed successfully</summary>
    public static readonly InterviewStatus InterviewComplete = new InterviewCompleteStatus();

    /// <summary>Interview could not be conducted due to various reasons</summary>
    public static readonly InterviewStatus UnableToConduct = new UnableToConductStatus();

    /// <summary>Interview was canceled before completion</summary>
    public static readonly InterviewStatus InterviewCanceled = new InterviewCanceledStatus();

    #endregion

    #region Implementations

    private sealed class IntervieweeIdentifiedStatus : InterviewStatus
    {
        public IntervieweeIdentifiedStatus() : base("INTERVIEWEE_IDENTIFIED", "Interviewee Identified",
            "Interviewee has been identified and contacted for the interview", true, 1)
        {
        }
    }

    private sealed class InterviewScheduledStatus : InterviewStatus
    {
        public InterviewScheduledStatus() : base("INTERVIEW_SCHEDULED", "Interview Scheduled",
            "Interview has been scheduled with specific date and time", true, 2)
        {
        }
    }

    private sealed class InterviewInProgressStatus : InterviewStatus
    {
        public InterviewInProgressStatus() : base("INTERVIEW_INPROGRESS", "Interview In Progress",
            "Interview is currently being conducted", true, 3)
        {
        }
    }

    private sealed class InterviewCompleteStatus : InterviewStatus
    {
        public InterviewCompleteStatus() : base("INTERVIEW_COMPLETE", "Interview Complete",
            "Interview has been completed successfully with all required information captured", false, 4)
        {
        }
    }

    private sealed class UnableToConductStatus : InterviewStatus
    {
        public UnableToConductStatus() : base("UNABLE_TO_CONDUCT", "Unable to Conduct",
            "Interview could not be conducted due to unavailability, refusal, or other circumstances", false, 5)
        {
        }
    }

    private sealed class InterviewCanceledStatus : InterviewStatus
    {
        public InterviewCanceledStatus() : base("INTERVIEW_CANCELED", "Interview Canceled",
            "Interview was canceled before completion by investigator or interviewee", true, 6)
        {
        }
    }

    #endregion

    /// <summary>
    /// Gets all available interview status values
    /// </summary>
    public static IEnumerable<InterviewStatus> GetAllValues()
    {
        return typeof(InterviewStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => f.FieldType == typeof(InterviewStatus))
            .Select(f => (InterviewStatus)f.GetValue(null))
            .Where(ins => ins != null)
            .OrderBy(ins => ins.WorkflowOrder);
    }
}
