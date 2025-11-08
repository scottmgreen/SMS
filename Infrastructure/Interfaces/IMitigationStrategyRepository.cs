using SMS_Domain.Entities;
using SMS_Shared.Common;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Mitigation Strategy Repository Interface - NEW
/// Supports MitigationStrategy as Domain Entity (not Value Object)
/// </summary>
public interface IMitigationStrategyRepository
{
    Task<Result<MitigationStrategy>> GetByIdAsync(MitigationStrategyId id);
    Task<Result<MitigationStrategy>> AddAsync(MitigationStrategy mitigationStrategy);
    Task<Result<bool>> UpdateAsync(MitigationStrategy mitigationStrategy);
    Task<Result<bool>> DeleteAsync(MitigationStrategyId id);
    Task<Result<IEnumerable<MitigationStrategy>>> GetAllAsync();
    Task<Result<IEnumerable<MitigationStrategy>>> GetByHazardIdAsync(string hazardId);
    Task<Result<IEnumerable<MitigationStrategy>>> GetByStatusAsync(MitigationStatus status);
    Task<Result<IEnumerable<MitigationStrategy>>> GetOverdueAsync();
    Task<Result<IEnumerable<MitigationStrategy>>> GetByResponsiblePersonAsync(string responsiblePerson);
}