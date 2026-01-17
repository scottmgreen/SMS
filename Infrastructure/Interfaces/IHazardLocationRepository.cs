namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// HazardLocation Repository Interface
/// </summary>
public interface IHazardLocationRepository
{
    Task<Result<HazardLocation>> GetByCodeAsync(HazardLocationID code);
    Task<Result<HazardLocation>> AddAsync(HazardLocation hazardLocation);
    Task<Result<bool>> UpdateAsync(HazardLocation hazardLocation);
    Task<Result<bool>> DeleteAsync(HazardLocationID code);
    Task<Result<IEnumerable<HazardLocation>>> GetAllAsync();
    Task<Result<IEnumerable<HazardLocation>>> GetByHazardCodeAsync(string hazardCode);
}