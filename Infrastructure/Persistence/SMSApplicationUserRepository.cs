using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SMS Application User operations
/// </summary>
public sealed class SMSApplicationUserRepository : BaseRepository<SMSApplicationUserRepository, SMSApplicationUser>, ISMSApplicationUserRepository
{
    private readonly ILogger<SMSApplicationUserRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSApplicationUserRepository(ILogger<SMSApplicationUserRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Application User Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<SMSApplicationUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSApplicationUser(reader);
                users.Add(user);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    public async Task<Result<SMSApplicationUser>> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetById} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id));

            SMSApplicationUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSApplicationUser(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (user is not null)
            {
                return Result<SMSApplicationUser>.Success(user);
            }
            else
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSApplicationUser>> GetByUserNameAsync(string userName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.UserNameError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetByUserName} UserName:{userName}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetByUserName, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserUserName, userName));

            SMSApplicationUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSApplicationUser(reader);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (user is not null)
            {
                return Result<SMSApplicationUser>.Success(user);
            }
            else
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetActiveUsersAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetActiveUsers}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetActiveUsers, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<SMSApplicationUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSApplicationUser(reader);
                users.Add(user);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    public async Task<Result<SMSApplicationUser>> AddAsync(SMSApplicationUser user)
    {
        try
        {
            if (user is null)
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_Insert} Code:{user.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserPassword, user.Password.HashedValue));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserApplicationRole, user.ApplicationRole));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserPermissionLevel, user.PermissionLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserIsActive, user.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastLoginDate, user.LastLoginDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, user.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewSMSApplicationUserCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;

            return await GetByIdAsync(newCodeValue).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.CreateFailed);
        }
    }

    public async Task<Result<bool>> UpdateAsync(SMSApplicationUser user)
    {
        try
        {
            if (user is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_Update} ID:{user.UserId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, user.UserId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserApplicationRole, user.ApplicationRole));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserPermissionLevel, user.PermissionLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserIsActive, user.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastLoginDate, user.LastLoginDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, user.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> UpdatePasswordAsync(string userId, string hashedPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_UpdatePassword} ID:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_UpdatePassword, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserPassword, hashedPassword));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.PasswordUpdateFailed);
        }
    }

    public async Task<Result<bool>> RecordLoginAsync(string userId, DateTime loginDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_RecordLogin} ID:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_RecordLogin, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLoginDate, loginDate));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_Delete} ID:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userId));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.DeleteFailed);
        }
    }

    public async Task<Result<bool>> UserNameExistsAsync(string userName)
    {
        try
        {
            var result = await GetByUserNameAsync(userName);
            return Result<bool>.Success(result.IsSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<UserStatistics>> GetUserStatisticsAsync()
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<UserStatistics>.Failure<UserStatistics>(allUsersResult.Error);
            }

            var users = allUsersResult.Value;
            var stats = new UserStatistics
            {
                TotalUsers = users.Count(),
                ActiveUsers = users.Count(u => u.IsActive),
                InactiveUsers = users.Count(u => !u.IsActive),
                UsersRequiringPasswordChange = users.Count(u => u.RequiresPasswordChange),
                StaleUsers = users.Count(u => u.IsStale()),
                LastLoginDate = users.Where(u => u.LastLoginDate.HasValue).Max(u => u.LastLoginDate)
            };

            return Result<UserStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<UserStatistics>.Failure<UserStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetByApplicationRoleAsync(string applicationRole)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetByRole} Role:{applicationRole}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetByRole, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserApplicationRole, applicationRole));

            var users = new List<SMSApplicationUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSApplicationUser(reader);
                users.Add(user);
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetByPermissionLevelAsync(string permissionLevel)
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value.Where(u => u.PermissionLevel.Equals(permissionLevel, StringComparison.OrdinalIgnoreCase));
            return Result<IEnumerable<SMSApplicationUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersWithMinimumPermissionAsync(string minimumPermissionLevel)
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(allUsersResult.Error);
            }

            var levels = new[] { "Read", "Write", "Admin", "SuperAdmin" };
            var requiredLevelIndex = Array.IndexOf(levels, minimumPermissionLevel);

            var filteredUsers = allUsersResult.Value.Where(u => 
            {
                var userLevelIndex = Array.IndexOf(levels, u.PermissionLevel);
                return userLevelIndex >= requiredLevelIndex;
            });

            return Result<IEnumerable<SMSApplicationUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    public async Task<Result<bool>> UpdateApplicationInfoAsync(string userId, string applicationRole, string permissionLevel)
    {
        try
        {
            var userResult = await GetByIdAsync(userId);
            if (userResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(userResult.Error);
            }

            var user = userResult.Value;
            user.UpdateApplicationInfo(applicationRole, permissionLevel);

            return await UpdateAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }
}