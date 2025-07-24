
namespace CBT3_Application.Messaging.Queries;

public class GetTraineeQuery : BaseQueryBundle, IRequest<Result<Trainee>>
{
    public TraineeID TraineeId { get; set; }
    public GetTraineeQuery(TraineeID traineeId)
    {
        TraineeId = traineeId;
    }


}
