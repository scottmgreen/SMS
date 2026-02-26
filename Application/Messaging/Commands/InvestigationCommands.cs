//-----------------------------------------------------------------------
// <copyright file="InvestigationCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS investigation workflow and process management.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Messaging.Commands;

public class CreateInvestigationCommand : BaseCommandBundle, IRequest<Result<Investigation>>, ICreateCommand
{
    public Investigation Investigation { get; set; }

    public CreateInvestigationCommand(Investigation investigation)
    {
        Investigation = investigation ?? throw new ArgumentNullException(nameof(investigation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Investigation.CreatedBy = userId;
        Investigation.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateInvestigationCommand : BaseCommandBundle, IRequest<Result<Investigation>>, IUpdateCommand
{
    public Investigation Investigation { get; set; }

    public UpdateInvestigationCommand(Investigation investigation)
    {
        Investigation = investigation ?? throw new ArgumentNullException(nameof(investigation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Investigation.UpdatedBy = userId;
        Investigation.UpdatedDate = timestamp;
    }
}

public class DeleteInvestigationCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public InvestigationID InvestigationId { get; set; }

    public DeleteInvestigationCommand(InvestigationID investigationId)
    {
        InvestigationId = investigationId ?? throw new ArgumentNullException(nameof(investigationId));
    }
}
