using SMS_Domain.Interfaces;

namespace Infrastructure.Interfaces;
public interface ISMSOrganizationalUserRepository
{
    Task<Result<SMSOrganizationalUser>> AddAsync(SMSOrganizationalUser user);
    Task<Result<bool>> DeleteAsync(SMSOrganizationalUserID userId);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetActiveUsersAsync();
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllAsync();
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByDepartmentAsync(string department);
    Task<Result<SMSOrganizationalUser>> GetByIdAsync(SMSOrganizationalUserID id);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByPositionAsync(string position);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetBySMSOrganizationalUserLevelAsync(string organizationLevel);
    Task<Result<SMSOrganizationalUser>> GetByUserNameAsync(string userName);
    Task<Result<Dictionary<string, int>>> GetDepartmentStatisticsAsync();
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetDepartmentSupervisorsAsync(string department);
    Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersAtOrAboveLevelAsync(string minimumLevel);
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<bool>> RecordLoginAsync(SMSOrganizationalUserID userId, DateTime loginDate);
    Task<Result<bool>> RecordLoginAsync(string userId, DateTime loginDate);
    Task<Result<bool>> UpdateAsync(SMSOrganizationalUser user);
    Task<Result<bool>> UpdatePasswordAsync(SMSOrganizationalUserID userId, string hashedPassword);
    Task<Result<bool>> UpdatePasswordAsync(string userId, string hashedPassword);
    Task<Result<bool>> UserNameExistsAsync(string userName);
}