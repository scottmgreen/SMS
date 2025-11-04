// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
namespace SMS_Infrastructure.Services;

// 4. InterviewDataService.cs
public class InterviewDataService : BaseDataService<InterviewDataService>
{
    private readonly ILogger<InterviewDataService> _logger;
    private readonly string _logheader;
    private readonly InterviewRepository _repo;

    public InterviewDataService(ILogger<InterviewDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, InterviewRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<Interview>> CreateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        return _repo.CreateInterviewAsync(interview, ct);
    }

    public Task<Result<Interview>> GetInterviewByIdAsync(InterviewID id, CancellationToken ct = default)
    {
        return _repo.GetInterviewByIdAsync(id, ct);
    }

    public Task<Result<List<Interview>>> GetAllInterviewsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllInterviewsAsync(ct);
    }

    public Task<Result<Interview>> UpdateInterviewAsync(Interview interview, CancellationToken ct = default)
    {
        return _repo.UpdateInterviewAsync(interview, ct);
    }

    public Task<Result<bool>> DeleteInterviewAsync(InterviewID id, CancellationToken ct = default)
    {
        return _repo.DeleteInterviewAsync(id, ct);
    }
}
