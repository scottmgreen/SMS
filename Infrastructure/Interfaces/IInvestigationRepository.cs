namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Enhanced Investigation Repository Interface
/// Defines comprehensive contract for investigation data operations
/// </summary>
public interface IInvestigationRepository
{
    // Core CRUD Operations
    Task<Result<Investigation>> CreateInvestigationAsync(Investigation investigation, CancellationToken ct = default);
    Task<Result<Investigation>> GetInvestigationByCodeAsync(string code, CancellationToken ct = default);
    Task<Result<List<Investigation>>> GetAllInvestigationsAsync(CancellationToken ct = default);
    Task<Result<Investigation>> UpdateInvestigationAsync(Investigation investigation, CancellationToken ct = default);
    Task<Result<bool>> DeleteInvestigationAsync(InvestigationID id, CancellationToken ct = default);

    // Query Operations
    Task<Result<IEnumerable<Investigation>>> GetByHazardCodeAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<IEnumerable<Investigation>>> GetByInvestigatorAsync(string investigatorId, CancellationToken ct = default);
    Task<Result<IEnumerable<Investigation>>> GetByStatusAsync(InvestigationStatus status, CancellationToken ct = default);

    // Status Management Operations
    Task<Result<bool>> UpdateStatusAsync(string investigationCode, InvestigationStatus status, CancellationToken ct = default);
    Task<Result<bool>> RecordDecisionAsync(string investigationCode, string decisionType, string rationale,
        string decisionMaker, string? nextSteps = null, string? referralDetails = null, CancellationToken ct = default);
    Task<Result<bool>> CompleteInvestigationAsync(string investigationCode, CancellationToken ct = default);
}