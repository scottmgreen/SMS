namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// Hazard Repository Interface
/// </summary>
public interface IHazardRepository
{
    Task<Result<Hazard>> GetByIdAsync(HazardID id);
    Task<Result<Hazard>> AddAsync(Hazard hazard);
    Task<Result<bool>> UpdateAsync(Hazard hazard);
    Task<Result<bool>> DeleteAsync(HazardID id);
    Task<Result<IEnumerable<Hazard>>> GetAllAsync();
    Task<Result<IEnumerable<Hazard>>> GetAllActiveAsync();
    Task<Result<IEnumerable<Hazard>>> GetByStatusAsync(string status);
    Task<Result<IEnumerable<Hazard>>> GetByReportCodeAsync(string reportCode);
}
