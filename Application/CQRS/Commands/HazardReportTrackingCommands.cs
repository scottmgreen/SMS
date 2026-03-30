//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS hazard management and lifecycle operations.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

// =============================================
// HAZARD REPORT TRACKING COMMANDS
// =============================================

public class CreateHazardReportTrackingCommand : BaseCommandBundle, IRequest<Result<HazardReportTracking>>, ICreateCommand
{
    public HazardReportTracking HazardReportTracking { get; set; }

    public CreateHazardReportTrackingCommand(HazardReportTracking hazardReportTracking)
    {
        HazardReportTracking = hazardReportTracking ?? throw new ArgumentNullException(nameof(hazardReportTracking));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        HazardReportTracking.CreatedBy = userId;
        HazardReportTracking.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateHazardReportTrackingCommand : BaseCommandBundle, IRequest<Result<HazardReportTracking>>, IUpdateCommand
{
    public HazardReportTracking HazardReportTracking { get; set; }

    public UpdateHazardReportTrackingCommand(HazardReportTracking hazardReportTracking)
    {
        HazardReportTracking = hazardReportTracking ?? throw new ArgumentNullException(nameof(hazardReportTracking));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        HazardReportTracking.UpdatedBy = userId;
        HazardReportTracking.UpdatedDate = timestamp;
    }
}

public class DeleteHazardReportTrackingCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public HazardReportTrackingID HazardReportTrackingId { get; set; }

    public DeleteHazardReportTrackingCommand(HazardReportTrackingID hazardReportTrackingId)
    {
        HazardReportTrackingId = hazardReportTrackingId ?? throw new ArgumentNullException(nameof(hazardReportTrackingId));
    }
}

/// <summary>
/// Command to create a new hazard report with automatic tracking code generation
/// </summary>
public class CreateHazardReportWithTrackingCommand : BaseCommandBundle, IRequest<Result<HazardReportTrackingResult>>, ICreateCommand
{
    public string HazardCode { get; set; }
    public string ReportCode { get; set; }

    public CreateHazardReportWithTrackingCommand(string hazardCode, string reportCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // Implementation handled in command handler
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Result model for hazard report creation with tracking information
/// </summary>
public class HazardReportTrackingResult
{
    public HazardReportTracking HazardReportTracking { get; set; } = null!;
    public string TrackingCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
