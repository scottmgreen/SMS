
using CBT3_Application.Messaging.Queries;

using CBT3_Domain.Entities;

namespace CBT3_Application.Messaging.QueryHandlers;

public class GetTraineeQueryHandler : BaseQueryBundle, IRequestHandler<GetTraineeQuery, Result<Trainee>>
{
    private TrainingService _trainingService;
    public GetTraineeQueryHandler(TrainingService trainingService)
    {
        _trainingService = trainingService;
    }
    public Task<Result<Trainee>> HandleAsync(GetTraineeQuery request, CancellationToken ct = default)
    {
        TraineeID traineeid = request.TraineeId;

        Result<Trainee> trainee = Task.Run(() => _trainingService.GetTraineeByIdAsync(traineeid, ct)).Result;

        return Task.FromResult(trainee);
    }
}
