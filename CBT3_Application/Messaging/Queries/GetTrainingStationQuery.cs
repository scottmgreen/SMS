namespace CBT3_Application.Messaging.Queries;



public class GetTrainingStationQuery : BaseQueryBundle, IRequest<Result<TrainingStation>>
{
    public string HostName { get; set; }
    public GetTrainingStationQuery(string hostname)
    {
        HostName = hostname;
    }


}