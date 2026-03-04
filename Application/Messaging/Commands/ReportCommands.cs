//-----------------------------------------------------------------------
// <copyright file="ReportCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS report management and processing operations.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Special Handling Commands 
/// </summary>

public class UpdateReportStatusCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public string ReportCode { get; set; }
    public ReportStatus ReportStatus { get; set; }

    public string UpdatedBy { get; set; }   
    public UpdateReportStatusCommand(string reportcode, ReportStatus reportstatus ,string updatedby)
    {
        ReportCode = reportcode ?? throw new ArgumentNullException(nameof(reportcode));
        ReportStatus = reportstatus ?? throw new ArgumentNullException(nameof(reportstatus));
        UpdatedBy = updatedby ?? throw new ArgumentNullException(nameof(updatedby));    
    }

    
}



public class CreateReportCommand : BaseCommandBundle, IRequest<Result<Report>>, ICreateCommand
{
    public Report Report { get; set; }

    public CreateReportCommand(Report report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Report.CreatedBy = userId;
        Report.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateReportCommand : BaseCommandBundle, IRequest<Result<Report>>, IUpdateCommand
{
    public Report Report { get; set; }

    public UpdateReportCommand(Report report)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Report.UpdatedBy = userId;
        Report.UpdatedDate = timestamp;
    }
}

public class DeleteReportCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public ReportID ReportId { get; set; }

    public DeleteReportCommand(ReportID reportId)
    {
        ReportId = reportId ?? throw new ArgumentNullException(nameof(reportId));
    }
}
