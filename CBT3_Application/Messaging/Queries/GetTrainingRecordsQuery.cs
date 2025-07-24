namespace CBT3_Application.Messaging.Queries;

public class GetTrainingRecordsQuery : BaseQueryBundle, IRequest<Result<List<TrainingRecord>>>
{
    
    public GetTrainingRecordsQuery()
    {
    }


}
