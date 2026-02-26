//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Commands;

public class CreateMitigationAssignmentCommand : BaseCommandBundle, IRequest<Result<MitigationAssignment>>, ICreateCommand
{
    public MitigationAssignment MitigationAssignment { get; set; }

    public CreateMitigationAssignmentCommand(MitigationAssignment mitigationAssignment)
    {
        MitigationAssignment = mitigationAssignment ?? throw new ArgumentNullException(nameof(mitigationAssignment));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        MitigationAssignment.CreatedBy = userId;
        MitigationAssignment.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateMitigationAssignmentCommand : BaseCommandBundle, IRequest<Result<MitigationAssignment>>, IUpdateCommand
{
    public MitigationAssignment MitigationAssignment { get; set; }

    public UpdateMitigationAssignmentCommand(MitigationAssignment mitigationAssignment)
    {
        MitigationAssignment = mitigationAssignment ?? throw new ArgumentNullException(nameof(mitigationAssignment));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        MitigationAssignment.UpdatedBy = userId;
        MitigationAssignment.UpdatedDate = timestamp;
    }
}

public class DeleteMitigationAssignmentCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public MitigationAssignmentID MitigationAssignmentId { get; set; }

    public DeleteMitigationAssignmentCommand(MitigationAssignmentID mitigationAssignmentId)
    {
        MitigationAssignmentId = mitigationAssignmentId ?? throw new ArgumentNullException(nameof(mitigationAssignmentId));
    }
}
