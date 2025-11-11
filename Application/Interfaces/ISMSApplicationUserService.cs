namespace Application.Interfaces;

public interface ISMSApplicationUserService
{
    Task<Result<SMSApplicationUser>> AuthenticateSMSApplicationUserAsync(string userName, string plainTextPassword, CancellationToken ct = default);
    Task<Result<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> CreateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default);
    Task<Result<bool>> DeleteSMSApplicationUserAsync(string userId, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetActiveSMSApplicationUsersAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetAllSMSApplicationUsersAsync(CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> GetSMSApplicationUserByIdAsync(string id, CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> GetSMSApplicationUserByUserNameAsync(string userName, CancellationToken ct = default);
    Task<Result<IEnumerable<SMSApplicationUser>>> GetSMSApplicationUsersByRoleAsync(string applicationRole, CancellationToken ct = default);
    Task<Result<UserStatistics>> GetSMSApplicationUserStatisticsAsync(CancellationToken ct = default);
    Task<Result<SMSApplicationUser>> UpdateSMSApplicationUserAsync(SMSApplicationUser user, CancellationToken ct = default);
}