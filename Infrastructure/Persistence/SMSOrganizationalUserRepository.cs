//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsorganizationaluser entities with CRUD operations and business queries.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using Infrastructure.Interfaces;

using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SMS Organizational User operations
/// </summary>
public sealed class SMSOrganizationalUserRepository : BaseRepository<SMSOrganizationalUserRepository, SMSOrganizationalUser>, ISMSOrganizationalUserRepository
{
    private readonly ILogger<SMSOrganizationalUserRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSOrganizationalUserRepository(ILogger<SMSOrganizationalUserRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Organizational User Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<SMSOrganizationalUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSOrganizationalUser(reader);
                users.Add(user);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSOrganizationalUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    public async Task<Result<SMSOrganizationalUser>> GetByCodeAsync(SMSOrganizationalUserID code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            SMSOrganizationalUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSOrganizationalUser(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (user is not null)
            {
                return Result<SMSOrganizationalUser>.Success(user);
            }
            else
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }


    public async Task<Result<SMSOrganizationalUser>> GetByUserNameAsync(string userName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.UserNameError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_GetByUserName} UserName:{userName}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_GetByUserName, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserUserName, userName));

            SMSOrganizationalUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSOrganizationalUser(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (user is not null)
            {
                return Result<SMSOrganizationalUser>.Success(user);
            }
            else
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetActiveUsersAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_GetActiveUsers}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_GetActiveUsers, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<SMSOrganizationalUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSOrganizationalUser(reader);
                users.Add(user);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSOrganizationalUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    public async Task<Result<SMSOrganizationalUser>> AddAsync(SMSOrganizationalUser user)
    {
        try
        {
            if (user is null)
            {
                return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_Insert} Code:{user.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserPassword, user.Password.HashedValue));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserDepartment, user.Department.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserPosition, user.Position));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserOrganizationLevel, user.OrganizationLevel.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserIsActive, user.IsActive));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserLastLoginDate, user.LastLoginDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, user.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewUserCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;

            return await GetByCodeAsync(newCodeValue).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganizationalUser>.Failure<SMSOrganizationalUser>(DomainErrors.SMSOrganizationalUserError.CreateFailed);
        }
    }

    public async Task<Result<bool>> UpdateAsync(SMSOrganizationalUser user)
    {
        try
        {
            if (user is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_Update} ID:{user.UserId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserDepartment, user.Department.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserPosition, user.Position));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserOrganizationLevel, user.OrganizationLevel.Value));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserSMSRole, user.SMSUserRole.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserAuthorityLevel, user.AuthorityLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserRiskApprovalAuthority, user.RiskApprovalAuthority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserIsActive, user.IsActive));
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> UpdatePasswordAsync(SMSOrganizationalUserID userId, string hashedPassword)
    {
        // Convert BaseUserID to string for compatibility
        return await UpdatePasswordAsync(userId.Value, hashedPassword);
    }

    public async Task<Result<bool>> UpdatePasswordAsync(string userId, string hashedPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_UpdatePassword} ID:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_UpdatePassword, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserPassword, hashedPassword));
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.PasswordUpdateFailed);
        }
    }

    public async Task<Result<bool>> RecordLoginAsync(SMSOrganizationalUserID userId, DateTime loginDate)
    {
        // Convert BaseUserID to string for compatibility  
        return await RecordLoginAsync(userId.Value, loginDate);
    }

    public async Task<Result<bool>> RecordLoginAsync(string userId, DateTime loginDate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_RecordLogin} ID:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_RecordLogin, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserLoginDate, loginDate));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAsync(SMSOrganizationalUserID userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_Delete} ID:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, userId.Value));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.DeleteFailed);
        }
    }

    //public async Task<Result<bool>> DeleteAsync(BaseUserID userId)
    //{
    //    // Convert BaseUserID to string for compatibility
    //    return await DeleteAsync(userId.Value);
    //}

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

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByDepartmentAsync(string department)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_GetByDepartment} Department:{department}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_GetByDepartment, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserDepartment, department));

            var users = new List<SMSOrganizationalUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSOrganizationalUser(reader);
                users.Add(user);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSOrganizationalUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetByPositionAsync(string position)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUser_GetByPosition} Position:{position}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_GetByPosition, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserPosition, position));

            var users = new List<SMSOrganizationalUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSOrganizationalUser(reader);
                users.Add(user);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSOrganizationalUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetBySMSOrganizationalUserLevelAsync(string organizationLevel)
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var filteredUsers = allUsersResult.Value.Where(u => u.OrganizationLevel.Value.Equals(organizationLevel, StringComparison.OrdinalIgnoreCase));
            return Result<IEnumerable<SMSOrganizationalUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersAtOrAboveLevelAsync(string minimumLevel)
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(allUsersResult.Error);
            }

            var levels = new[] { "Staff", "Senior", "Supervisor", "Manager", "Director", "Executive" };
            var requiredLevelIndex = Array.IndexOf(levels, minimumLevel);

            var filteredUsers = allUsersResult.Value.Where(u =>
            {
                var userLevelIndex = Array.IndexOf(levels, u.OrganizationLevel);
                return userLevelIndex >= requiredLevelIndex;
            });

            return Result<IEnumerable<SMSOrganizationalUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalUserError.NotFound);
        }
    }

    //public async Task<Result<bool>> UpdateSMSOrganizationalUserInfoAsync(BaseUserID userId, string department, string position, string organizationLevel)
    //{
    //    try
    //    {
    //        var userResult = await GetByCodeAsync(userId);
    //        if (userResult.IsFailure)
    //        {
    //            return Result<bool>.Failure<bool>(userResult.Error);
    //        }

    //        var user = userResult.Value;
    //        user.UpdateOrganizationalInfo(department, position, organizationLevel);

    //        return await UpdateAsync(user);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
    //        return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
    //    }
    //}

    public async Task<Result<Dictionary<string, int>>> GetDepartmentStatisticsAsync()
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(allUsersResult.Error);
            }

            var stats = allUsersResult.Value
                .GroupBy(u => u.Department.Value)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result<Dictionary<string, int>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }


    // 🔐 Two-Factor Authentication Repository Methods

    /// <summary>
    /// Setup 2FA for an organizational user (first-time setup)
    /// </summary>
    public async Task<Result<bool>> Setup2FAAsync(string userCode, string secretKey, string? backupCodes = null, string updatedBy = "SYSTEM-2FA")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(secretKey))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Setup2FA for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Setup2FA, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pSecretKey", secretKey));
            cmd.Parameters.Add(DataAccess.Parameter("@pBackupCodes", backupCodes));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedBy", updatedBy));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Update failed 2FA attempts and optionally set lockout
    /// </summary>
    public async Task<Result<bool>> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil = null, string updatedBy = "SYSTEM-2FA")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Update2FAFailedAttempts for user: {userCode}, Attempts: {failedAttempts}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Update2FAFailedAttempts, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pFailedAttempts", failedAttempts));
            cmd.Parameters.Add(DataAccess.Parameter("@pLockoutUntil", lockoutUntil));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedBy", updatedBy));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Reset failed 2FA attempts (called on successful 2FA verification)
    /// </summary>
    public async Task<Result<bool>> Reset2FAFailedAttemptsAsync(string userCode, string updatedBy = "SYSTEM-2FA")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Reset2FAFailedAttempts for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Reset2FAFailedAttempts, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedBy", updatedBy));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Disable 2FA for an organizational user
    /// </summary>
    public async Task<Result<bool>> Disable2FAAsync(string userCode, string updatedBy = "SYSTEM-2FA")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Disable2FA for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUser_Disable2FA, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedBy", updatedBy));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalUserError.UpdateFailed);
        }
    }
}
