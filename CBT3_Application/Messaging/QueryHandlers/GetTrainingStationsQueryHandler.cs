
using CBT3_Application.Messaging.Queries;

namespace CBT3_Application.Messaging.QueryHandlers;

public class GetTrainingStationsQueryHandler : BaseQueryBundle, IRequestHandler<GetTrainingStationsQuery, Result<List<TrainingStation>>>
{
    private DashboardService _dashboardService;
    public GetTrainingStationsQueryHandler(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    public Task<Result<List<TrainingStation>>> HandleAsync(GetTrainingStationsQuery request, CancellationToken ct = default)
    {
        Result<List<TrainingStation>> records = Task.Run(() => _dashboardService.GetTrainingStationsAsync(ct)).Result;

        return Task.FromResult(records);
    }


}
