namespace CBT3_Application.Messaging.Queries;

public class GetTrainingStationsQuery : BaseQueryBundle, IRequest<Result<List<TrainingStation>>>
{

    public GetTrainingStationsQuery()
    {
    }


}
