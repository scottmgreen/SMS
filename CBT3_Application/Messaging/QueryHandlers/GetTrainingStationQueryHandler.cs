
using CBT3_Application.Messaging.Queries;

namespace CBT3_Application.Messaging.QueryHandlers;



public class GetTrainingStationQueryHandler : BaseQueryBundle, IRequestHandler<GetTrainingStationQuery, Result<TrainingStation>>
{
    private DashboardService _dashboardService;
    public GetTrainingStationQueryHandler(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    public Task<Result<TrainingStation>> HandleAsync(GetTrainingStationQuery request, CancellationToken ct = default)
    {
        string hostname = request.HostName;
        Result<TrainingStation> records = Task.Run(() => _dashboardService.GetTrainingStationAsync(hostname, ct)).Result;

        return Task.FromResult(records);
    }


}