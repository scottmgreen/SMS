//-----------------------------------------------------------------------
// <copyright file="ReportValidationCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS report management and processing operations.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Commands;

public class CreateReportValidationCommand : BaseCommandBundle, IRequest<Result<ReportValidation>>, ICreateCommand
{
    public ReportValidation ReportValidation { get; set; }

    public CreateReportValidationCommand(ReportValidation reportValidation)
    {
        ReportValidation = reportValidation ?? throw new ArgumentNullException(nameof(reportValidation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        ReportValidation.CreatedBy = userId;
        ReportValidation.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateReportValidationCommand : BaseCommandBundle, IRequest<Result<ReportValidation>>, IUpdateCommand
{
    public ReportValidation ReportValidation { get; set; }

    public UpdateReportValidationCommand(ReportValidation reportValidation)
    {
        ReportValidation = reportValidation ?? throw new ArgumentNullException(nameof(reportValidation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ReportValidation.UpdatedBy = userId;
        ReportValidation.UpdatedDate = timestamp;
    }
}

public class DeleteReportValidationCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public ReportValidationID ReportValidationId { get; set; }
    public string DeletedBy { get; set; } = string.Empty;

    public DeleteReportValidationCommand(ReportValidationID reportValidationId)
    {
        ReportValidationId = reportValidationId ?? throw new ArgumentNullException(nameof(reportValidationId));
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}

public class ResetReportValidationCommand : BaseCommandBundle, IRequest<Result<bool>>, IUpdateCommand
{
    public ReportValidationID ReportValidationId { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;

    public ResetReportValidationCommand(ReportValidationID reportValidationId)
    {
        ReportValidationId = reportValidationId ?? throw new ArgumentNullException(nameof(reportValidationId));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For reset commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}
