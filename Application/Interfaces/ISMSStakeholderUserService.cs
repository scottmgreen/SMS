namespace Application.Interfaces;

public interface ISMSStakeholderUserService
{
    Task<Result<SMSStakeholderUser>> AuthenticateSMSStakeholderUserAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<SMSStakeholderUser>> CreateSMSStakeholderUserAsync(SMSStakeholderUser user, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAirlineStakeholdersAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllSMSStakeholderUsersAsync(CancellationToken ct = default);
    Task<Result<SMSStakeholderUser>> GetSMSStakeholderUserByIdAsync(string id, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetSMSStakeholderUsersByTypeAsync(string stakeholderType, CancellationToken ct = default);
    Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersRequiringAOAAccessAsync(CancellationToken ct = default);
    Task<Result<SMSStakeholderUser>> UpdateSMSStakeholderUserAsync(SMSStakeholderUser user, CancellationToken ct = default);
}