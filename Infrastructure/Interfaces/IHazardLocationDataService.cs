namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// HazardLocation Data Service Interface
/// </summary>
public interface IHazardLocationDataService
{
    Task<Result<HazardLocation>> CreateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default);
    Task<Result<HazardLocation>> GetHazardLocationByCodeAsync(HazardLocationID code, CancellationToken ct = default);
    Task<Result<List<HazardLocation>>> GetAllHazardLocationsAsync(CancellationToken ct = default);
    Task<Result<List<HazardLocation>>> GetHazardLocationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default);
    Task<Result<HazardLocation>> UpdateHazardLocationAsync(HazardLocation hazardLocation, CancellationToken ct = default);
    Task<Result<bool>> DeleteHazardLocationAsync(HazardLocationID id, CancellationToken ct = default);
}