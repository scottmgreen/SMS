//-----------------------------------------------------------------------
// <copyright file="HazardCommands.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command definitions for SMS hazard management and lifecycle operations.
//                  Defines command objects for write operations in the CQRS pattern.
//                  Commands represent business intentions and trigger state changes.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a hazard from an existing Hazard entity
/// </summary>
public class CreateHazardCommand : BaseCommandBundle, IRequest<Result<Hazard>>, ICreateCommand
{
    public Hazard Hazard { get; set; }

    public CreateHazardCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        Hazard.CreatedBy = userId;
        Hazard.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
        // But if needed for your domain logic, you can implement it
    }
}

/// <summary>
/// Command to update an existing hazard
/// </summary>
public class UpdateHazardCommand : BaseCommandBundle, IRequest<Result<Hazard>>, IUpdateCommand
{
    public Hazard Hazard { get; set; }

    public UpdateHazardCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
        // But if needed for your domain logic, you can implement it
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Hazard.UpdatedBy = userId;
        Hazard.UpdatedDate = timestamp;
    }
}
public class ResetHazardScoresCommand : BaseCommandBundle, IRequest<Result<Hazard>>, IUpdateCommand
{
    public Hazard Hazard { get; set; }

    public ResetHazardScoresCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
        // But if needed for your domain logic, you can implement it
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        Hazard.UpdatedBy = userId;
        Hazard.UpdatedDate = timestamp;
    }
}
/// <summary>
/// Command to delete a hazard (soft delete)
/// </summary>
public class DeleteHazardCommand : BaseCommandBundle, IRequest<Result<bool>>, IDeleteCommand
{
    public HazardID HazardId { get; set; }
    public string DeletedBy { get; set; } = string.Empty;

    public DeleteHazardCommand(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }

    public void SetDeletedBy(string userId, DateTime timestamp)
    {
        DeletedBy = userId;
    }
}
