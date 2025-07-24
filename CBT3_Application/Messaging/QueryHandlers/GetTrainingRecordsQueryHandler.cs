
using CBT3_Application.Messaging.Queries;

namespace CBT3_Application.Messaging.QueryHandlers;




public class GetTrainingRecordsQueryHandler : BaseQueryBundle, IRequestHandler<GetTrainingRecordsQuery, Result<List<TrainingRecord>>>
{
    private DashboardService _dashboardService;
    public GetTrainingRecordsQueryHandler(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    public Task<Result<List<TrainingRecord>>> HandleAsync(GetTrainingRecordsQuery request, CancellationToken ct = default)
    {
        Result<List<TrainingRecord>> records = Task.Run(() => _dashboardService.GetTrainingRecordsAsync(ct)).Result;

        return Task.FromResult(records);
    }


}
