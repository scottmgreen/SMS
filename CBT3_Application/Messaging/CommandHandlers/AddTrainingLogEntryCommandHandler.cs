namespace CBT3_Application.Messaging.CommandHandlers;

public class AddTrainingLogEntryCommandHandler : IRequestHandler<AddTrainingLogEntryCommand, TrainingLogEntry>
{
    public AddTrainingLogEntryCommandHandler(TrainingDataService dataService)
    {
        _dataService = dataService;
    }
    private readonly TrainingDataService _dataService;
    public Task<TrainingLogEntry> HandleAsync(AddTrainingLogEntryCommand request, CancellationToken ct = default)
    {
        _ = _dataService.AddTrainingLogEntryAsync(request.TrainingLogEntry,ct);
        return Task.FromResult(request.TrainingLogEntry);
    }
}