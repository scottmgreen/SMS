using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Persistence;
public interface ISMSStakeholderUserRepository
{
    Task<Result<SMSStakeholderUser>> AddAsync(SMSStakeholderUser user);
    Task<Result<bool>> DeleteAsync(SMSStakeholderUserID userId);
    Task<Result<bool>> DeleteAsync(string userId);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetActiveUsersAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAirlineStakeholdersAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllAsync();
    //Task<Result<IEnumerable<SMSStakeholderUser>>> GetByAccessLevelAsync(string accessLevel);
    Task<Result<SMSStakeholderUser>> GetByIdAsync(BaseUserID id);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByOrganizationAsync(string organization);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetByStakeholderTypeAsync(string stakeholderType);
    Task<Result<SMSStakeholderUser>> GetByUserNameAsync(string userName);
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetContractorStakeholdersAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetGroundHandlerStakeholdersAsync();
    Task<Result<Dictionary<string, int>>> GetOrganizationStatisticsAsync();
    Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode);
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
    Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersWithMinimumAccessAsync(string minimumAccessLevel);
    Task<Result<bool>> RecordLoginAsync(SMSStakeholderUserID userId, DateTime loginDate);
    Task<Result<bool>> UpdateAsync(SMSStakeholderUser user);
    //Task<Result<bool>> UpdatePasswordAsync(SMSStakeholderUserID userId, string hashedPassword);*/
    Task<Result<bool>> UpdatePasswordAsync(SMSStakeholderUserID userId, string hashedPassword);
    Task<Result<bool>> UserNameExistsAsync(string userName);
}