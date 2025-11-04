namespace SMS_Application.Messaging.Commands;

public class CreateHazardCommand : BaseCommandBundle, IRequest<Result<Hazard>>
{
    public Hazard Hazard { get; set; }

    public CreateHazardCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }
}

public class UpdateHazardCommand : BaseCommandBundle, IRequest<Result<Hazard>>
{
    public Hazard Hazard { get; set; }

    public UpdateHazardCommand(Hazard hazard)
    {
        Hazard = hazard ?? throw new ArgumentNullException(nameof(hazard));
    }
}

public class DeleteHazardCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public HazardID HazardId { get; set; }

    public DeleteHazardCommand(HazardID hazardId)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
    }
}