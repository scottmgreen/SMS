using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a hazard from an existing Hazard entity
/// </summary>
public class CreateHazardLocationCommand : BaseCommandBundle, IRequest<Result<HazardLocation>>
{
    public HazardLocation HazardLocation { get; set; }

    public CreateHazardLocationCommand(HazardLocation hazardlocation)
    {
        HazardLocation = hazardlocation ?? throw new ArgumentNullException(nameof(hazardlocation));
    }
}

/// <summary>
/// Command to update an existing hazard
/// </summary>
public class UpdateHazardLocationCommand : BaseCommandBundle, IRequest<Result<HazardLocation>>
{
    public HazardLocation HazardLocation { get; set; }

    public UpdateHazardLocationCommand(HazardLocation hazardlocation)
    {
        HazardLocation = hazardlocation ?? throw new ArgumentNullException(nameof(hazardlocation));
    }
}

/// <summary>
/// Command to delete a hazard (soft delete)
/// </summary>
public class DeleteHazardLocationCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public HazardLocationID HazardLocationId { get; set; }

    public DeleteHazardLocationCommand(HazardLocationID hazardlocationId)
    {
        HazardLocationId = hazardlocationId ?? throw new ArgumentNullException(nameof(hazardlocationId));
    }
}