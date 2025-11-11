using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

/// <summary>
/// Command to create a hazard from an existing Hazard entity
/// </summary>
public class CreateHazardCommand : BaseCommandBundle, IRequest<Result<Hazard>>
{
    public Hazard Hazard { get; set; }

    public CreateHazardCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }
}

/// <summary>
/// Command to update an existing hazard
/// </summary>
public class UpdateHazardCommand : BaseCommandBundle, IRequest<Result<Hazard>>
{
    public Hazard Hazard { get; set; }

    public UpdateHazardCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }
}

/// <summary>
/// Command to delete a hazard (soft delete)
/// </summary>
public class DeleteHazardCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public HazardID HazardId { get; set; }

    public DeleteHazardCommand(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }
}