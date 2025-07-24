namespace CBT3_Application.Messaging.Queries;

public class GetMismatchedTrainingRecordsQuery : BaseQueryBundle, IRequest<Result<List<TrainingRecord>>>
{

    public GetMismatchedTrainingRecordsQuery()
    {
    }


}
