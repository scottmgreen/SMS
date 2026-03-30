//-----------------------------------------------------------------------
// <copyright file="MitigationCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for write operations in the SMS CQRS architecture.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

public class CreateMitigationCommand : BaseCommandBundle, IRequest<Result<Mitigation>>, ICreateCommand
{
    public Mitigation Mitigation { get; set; }

    public CreateMitigationCommand(Mitigation mitigation)
    {
        Mitigation = mitigation ?? throw new ArgumentNullException(nameof(mitigation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Mitigation.CreatedBy = userId;
        Mitigation.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateMitigationCommand : BaseCommandBundle, IRequest<Result<Mitigation>>, IUpdateCommand
{
    public Mitigation Mitigation { get; set; }

    public UpdateMitigationCommand(Mitigation mitigation)
    {
        Mitigation = mitigation ?? throw new ArgumentNullException(nameof(mitigation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Mitigation.UpdatedBy = userId;
        Mitigation.UpdatedDate = timestamp;
    }
}

public class DeleteMitigationCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public MitigationID MitigationId { get; set; }
    public string DeletedBy { get; set; } = string.Empty;

    public DeleteMitigationCommand(MitigationID mitigationId)
    {
        MitigationId = mitigationId ?? throw new ArgumentNullException(nameof(mitigationId));
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}
