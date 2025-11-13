// =============================================
// SMS DATA SERVICES - COMPLETE SET
// All 10 SMS Data Services for easy copy/paste
// =============================================

// 1. HazardDataService.cs
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

// 8. MitigationAssignmentDataService.cs
public class MitigationAssignmentDataService : BaseDataService<MitigationAssignmentDataService>
{
    private readonly ILogger<MitigationAssignmentDataService> _logger;
    private readonly string _logheader;
    private readonly MitigationAssignmentRepository _repo;

    public MitigationAssignmentDataService(ILogger<MitigationAssignmentDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, MitigationAssignmentRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<MitigationAssignment>> CreateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        return _repo.CreateMitigationAssignmentAsync(mitigationAssignment, ct);
    }

    public Task<Result<MitigationAssignment>> GetMitigationAssignmentByIdAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        return _repo.GetMitigationAssignmentByIdAsync(id, ct);
    }

    public Task<Result<List<MitigationAssignment>>> GetAllMitigationAssignmentsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllMitigationAssignmentsAsync(ct);
    }

    public Task<Result<MitigationAssignment>> UpdateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        return _repo.UpdateMitigationAssignmentAsync(mitigationAssignment, ct);
    }

    public Task<Result<bool>> DeleteMitigationAssignmentAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        return _repo.DeleteMitigationAssignmentAsync(id, ct);
    }
}
