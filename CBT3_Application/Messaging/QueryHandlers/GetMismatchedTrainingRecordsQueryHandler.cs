
using CBT3_Application.Messaging.Queries;

namespace CBT3_Application.Messaging.QueryHandlers;
public class GetMismatchedTrainingRecordsQueryHandler : BaseQueryBundle, IRequestHandler<GetTrainingRecordsQuery, Result<List<TrainingRecord>>>
{
    private DashboardService _dashboardService;
    public GetMismatchedTrainingRecordsQueryHandler(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    public Task<Result<List<TrainingRecord>>> HandleAsync(GetTrainingRecordsQuery request, CancellationToken ct = default)
    {
        Result<List<TrainingRecord>> records = Task.Run(() => _dashboardService.GetMismatchedTrainingRecordsAsync(ct)).Result;

        return Task.FromResult(records);
    }


}