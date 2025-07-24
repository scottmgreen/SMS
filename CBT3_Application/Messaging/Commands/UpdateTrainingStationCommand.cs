namespace CBT3_Application.Messaging;


public class UpdateTrainingStationCommand : BaseCommandBundle, IRequest<ITrainingStation>
{
    public UpdateTrainingStationCommand(ITrainingStation station)
    {
        TrainingStation = station;
    }
    public ITrainingStation TrainingStation { get; init; }

}
