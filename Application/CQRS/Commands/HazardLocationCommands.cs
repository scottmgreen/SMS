//-----------------------------------------------------------------------
// <copyright file="HazardLocationCommands.cs" company="SMS Safety Management System">
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
/// Command to create a hazard location from an existing HazardLocation entity
/// </summary>
public class CreateHazardLocationCommand : BaseCommandBundle, IRequest<Result<HazardLocation>>, ICreateCommand
{
    public HazardLocation HazardLocation { get; set; }

    public CreateHazardLocationCommand(HazardLocation hazardlocation)
    {
        HazardLocation = hazardlocation ?? throw new ArgumentNullException(nameof(hazardlocation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        HazardLocation.CreatedBy = userId;
        HazardLocation.CreatedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

/// <summary>
/// Command to update an existing hazard location
/// </summary>
public class UpdateHazardLocationCommand : BaseCommandBundle, IRequest<Result<HazardLocation>>, IUpdateCommand
{
    public HazardLocation HazardLocation { get; set; }

    public UpdateHazardLocationCommand(HazardLocation hazardlocation)
    {
        HazardLocation = hazardlocation ?? throw new ArgumentNullException(nameof(hazardlocation));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        HazardLocation.UpdatedBy = userId;
        HazardLocation.UpdatedDate = timestamp;
    }
}

/// <summary>
/// Command to delete a hazard location (soft delete)
/// </summary>
public class DeleteHazardLocationCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public HazardLocationID HazardLocationId { get; set; }

    public DeleteHazardLocationCommand(HazardLocationID hazardlocationId)
    {
        HazardLocationId = hazardlocationId ?? throw new ArgumentNullException(nameof(hazardlocationId));
    }
}
