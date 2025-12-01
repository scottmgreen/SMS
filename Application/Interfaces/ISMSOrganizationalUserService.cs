namespace Application.Interfaces;

public interface ISMSOrganizationalUserService
{
    Task<Result<bool>> AuthenticateSMSOrganizationalUserAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<SMSOrganizationalUser>> CreateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllSMSOrganizationalUsersAsync(CancellationToken ct = default);
    Task<Result<SMSOrganizationalUser>> GetSMSOrganizationalUserByIdAsync(string id, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetSMSOrganizationalUsersByDepartmentAsync(string department, CancellationToken ct = default);
    Task<Result<SMSOrganizationalUser>> UpdateSMSOrganizationalUserAsync(SMSOrganizationalUser user, CancellationToken ct = default);
}