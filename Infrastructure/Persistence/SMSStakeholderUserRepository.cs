//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsstakeholderuser entities with CRUD operations and business queries.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SMS Stakeholder User operations
/// </summary>
public sealed class SMSStakeholderUserRepository : BaseRepository<SMSStakeholderUserRepository, SMSStakeholderUser>, ISMSStakeholderUserRepository
{
    private readonly ILogger<SMSStakeholderUserRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSStakeholderUserRepository(ILogger<SMSStakeholderUserRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Stakeholder User Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<SMSStakeholderUser>();
            var userRoles = new Dictionary<string, SMSUserRole>();
            var userRolePermissions = new Dictionary<string, List<SMSUserRolePermission>>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSStakeholderUser(reader);
                users.Add(user);
            }

            // Dataset 2: SMSUserRole data - collect unique roles
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var role = Mappers.MapToSMSUserRole(reader);
                    if (!userRoles.ContainsKey(role.Code))
                    {
                        userRoles[role.Code] = role;
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data - collect permissions by role code with deduplication
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    var roleCode = permission.SMSUserRoleCode;

                    if (!userRolePermissions.ContainsKey(roleCode))
                    {
                        userRolePermissions[roleCode] = new List<SMSUserRolePermission>();
                    }

                    // Check for duplicates before adding
                    var existingPermission = userRolePermissions[roleCode]
                        .FirstOrDefault(p => p.SMSModule == permission.SMSModule);

                    if (existingPermission == null)
                    {
                        userRolePermissions[roleCode].Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            // Now assign roles and permissions to ALL users
            foreach (var user in users)
            {
                // Only process users that have a role assigned
                if (!string.IsNullOrWhiteSpace(user.UserRole?.Code))
                {
                    var userRoleCode = user.UserRole.Code.Trim();

                    // Get the role for this user
                    if (userRoles.TryGetValue(userRoleCode, out var userRole))
                    {
                        // Clone the role to avoid reference issues
                        user.UserRole = new SMSUserRole(new SMSUserRoleID(userRole.Code))
                        {
                            Code = userRole.Code,
                            Name = userRole.Name,
                            CreatedBy = userRole.CreatedBy,
                            CreatedDate = userRole.CreatedDate,
                            UpdatedBy = userRole.UpdatedBy,
                            UpdatedDate = userRole.UpdatedDate,
                            Permissions = new List<SMSUserRolePermission>()
                        };

                        // Get permissions for this role
                        if (userRolePermissions.TryGetValue(userRoleCode, out var permissions))
                        {
                            user.UserRole.Permissions = permissions.ToList();
                        }
                    }
                }
            }

            return Result<IEnumerable<SMSStakeholderUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    public async Task<Result<SMSStakeholderUser>> GetByCodeAsync(BaseUserID code)
    {
        try
        {
            if (code?.Value is null || string.IsNullOrWhiteSpace(code.Value))
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetByCode} Code:{code.Value}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            SMSStakeholderUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSStakeholderUser(reader);
            }

            // Dataset 2: SMSUserRole data
            if (user != null && await reader.NextResultAsync().ConfigureAwait(false) && await reader.ReadAsync().ConfigureAwait(false))
            {
                user.UserRole = Mappers.MapToSMSUserRole(reader);
            }

            // Dataset 3: SMSUserRolePermissions data (multiple rows)
            if (user?.UserRole != null && await reader.NextResultAsync().ConfigureAwait(false))
            {
                var permissions = new List<SMSUserRolePermission>();
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    permissions.Add(permission);
                }
                user.UserRole.Permissions = permissions;
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (user is not null)
            {
                return Result<SMSStakeholderUser>.Success(user);
            }
            else
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSStakeholderUser>> GetByUserNameAsync(string userName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.UserNameError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetByUserName} UserName:{userName}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetByUserName, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserUserName, userName));

            SMSStakeholderUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSStakeholderUser(reader);
            }

            // Dataset 2: SMSUserRole data
            if (user != null && await reader.NextResultAsync().ConfigureAwait(false) && await reader.ReadAsync().ConfigureAwait(false))
            {
                user.UserRole = Mappers.MapToSMSUserRole(reader);
            }

            // Dataset 3: SMSUserRolePermissions data (multiple rows)
            if (user?.UserRole != null && await reader.NextResultAsync().ConfigureAwait(false))
            {
                var permissions = new List<SMSUserRolePermission>();
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    permissions.Add(permission);
                }
                user.UserRole.Permissions = permissions;
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (user is not null)
            {
                return Result<SMSStakeholderUser>.Success(user);
            }
            else
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetActiveUsersAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetActiveUsers}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetActiveUsers, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var users = new List<SMSStakeholderUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSStakeholderUser(reader);
                users.Add(user);
            }

            // Dataset 2: SMSUserRole data for each user
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSRoleCode).Trim();
                    var user = users.FirstOrDefault(u => u.UserRole.Code.Trim() == userRoleCode);
                    if (user != null)
                    {
                        user.UserRole = Mappers.MapToSMSUserRole(reader);
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRoleCode);
                    var user = users.FirstOrDefault(u => u.UserRole.Code == userRoleCode);
                    if (user?.UserRole != null)
                    {
                        if (user.UserRole.Permissions == null)
                            user.UserRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        user.UserRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSStakeholderUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    public async Task<Result<SMSStakeholderUser>> AddAsync(SMSStakeholderUser user)
    {
        try
        {
            if (user is null)
            {
                return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_Insert} Code:{user.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserCompany, user.Company));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserOrganization, user.Organization));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTitle, user.Title));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserJobFunction, user.JobFunction));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserPassword, user.Password.HashedValue));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserStakeholderType, user.StakeholderType));
            
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserRole, user.UserRole.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserIsActive, user.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserLastLoginDate, user.LastLoginDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserIsPOPEmployee, user.IsPOPEmployee));
            
            // 🔐 NEW: Add 2FA parameters to AddAsync - uses same parameter names as Application and Organizational users
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorSecretKey, user.TwoFactorSecretKey));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorEnabled, user.TwoFactorEnabled));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserBackupCodes, user.BackupCodes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorSetupDate, user.TwoFactorSetupDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserFailedTwoFactorAttempts, user.FailedTwoFactorAttempts));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorLockedUntil, user.TwoFactorLockedUntil));
            
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, user.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewSMSStakeholderUserCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
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
            return Result<SMSStakeholderUser>.Failure<SMSStakeholderUser>(DomainErrors.SMSStakeholderUserError.CreateFailed);
        }
    }

    public async Task<Result<bool>> UpdateAsync(SMSStakeholderUser user)
    {
        try
        {
            if (user is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_Update} ID:{user.UserId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserCompany, user.Company));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserOrganization, user.Organization));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTitle, user.Title));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserJobFunction, user.JobFunction));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserUserName, user.UserName.Value));

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserStakeholderType, user.StakeholderType));
            
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserRole, user.UserRole.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserIsActive, user.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserLastLoginDate, user.LastLoginDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserIsPOPEmployee, user.IsPOPEmployee));
            
            // 🔐 NEW: Add 2FA parameters to UpdateAsync - uses same parameter names as Application and Organizational users
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorSecretKey, user.TwoFactorSecretKey));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorEnabled, user.TwoFactorEnabled));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserBackupCodes, user.BackupCodes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorSetupDate, user.TwoFactorSetupDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserFailedTwoFactorAttempts, user.FailedTwoFactorAttempts));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorLockedUntil, user.TwoFactorLockedUntil));
            
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> UpdatePasswordAsync(SMSStakeholderUserID userId, string hashedPassword)
    {
        // Convert BaseUserID to string for compatibility
        return await UpdatePasswordAsync(userId.Value, hashedPassword);
    }

    public async Task<Result<bool>> RecordLoginAsync(SMSStakeholderUserID userId, DateTime loginDate)
    {
        // Convert BaseUserID to string for compatibility  
        return await RecordLoginAsync(userId.Value, loginDate);
    }

    public async Task<Result<bool>> DeleteAsync(SMSStakeholderUserID userId)
    {
        // Convert BaseUserID to string for compatibility
        return await DeleteAsync(userId.Value);
    }

    public async Task<Result<bool>> UpdatePasswordAsync(string code, string hashedPassword)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_UpdatePassword} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_UpdatePassword, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserPassword, hashedPassword));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.PasswordUpdateFailed);
        }
    }

    //public async Task<Result<bool>> RecordLoginAsync(SMSStakeholderUserID userId, DateTime loginDate)
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(userId))
    //        {
    //            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
    //        }

    //        _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_RecordLogin} ID:{userId}", null);

    //        using var sql = new SqlConnection(_connectionString);
    //        using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_RecordLogin, sql)
    //        {
    //            CommandType = CommandType.StoredProcedure
    //        };

    //        cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userId));
    //        cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserLoginDate, loginDate));

    //        await sql.OpenAsync().ConfigureAwait(false);
    //        await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
    //        await sql.CloseAsync().ConfigureAwait(false);

    //        return Result<bool>.Success(true);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
    //        return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
    //    }
    //}

    public async Task<Result<bool>> DeleteAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_Delete} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.DeleteFailed);
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

    //Legit
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetByStakeholderTypeAsync(string stakeholderType)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetByStakeholderType} Type:{stakeholderType}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetByStakeholderType, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserStakeholderType, stakeholderType));

            var users = new List<SMSStakeholderUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSStakeholderUser(reader);
                users.Add(user);
            }

            // Dataset 2: SMSUserRole data for each user
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSRoleCode).Trim();
                    var user = users.FirstOrDefault(u => u.UserRole.Code.Trim() == userRoleCode);
                    if (user != null)
                    {
                        user.UserRole = Mappers.MapToSMSUserRole(reader);
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRoleCode);
                    var user = users.FirstOrDefault(u => u.UserRole.Code == userRoleCode);
                    if (user?.UserRole != null)
                    {
                        if (user.UserRole.Permissions == null)
                            user.UserRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        user.UserRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSStakeholderUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetByOrganizationAsync(string organization)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetByOrganization} Organization:{organization}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetByOrganization, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserOrganization, organization));

            var users = new List<SMSStakeholderUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSStakeholderUser(reader);
                users.Add(user);
            }

            // Dataset 2: SMSUserRole data for each user
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSRoleCode).Trim();
                    var user = users.FirstOrDefault(u => u.UserRole.Code.Trim() == userRoleCode);
                    if (user != null)
                    {
                        user.UserRole = Mappers.MapToSMSUserRole(reader);
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRoleCode);
                    var user = users.FirstOrDefault(u => u.UserRole.Code == userRoleCode);
                    if (user?.UserRole != null)
                    {
                        if (user.UserRole.Permissions == null)
                            user.UserRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        user.UserRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSStakeholderUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    //public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetByAccessLevelAsync(string accessLevel)
    //{
    //    try
    //    {
    //        var allUsersResult = await GetAllAsync();
    //        if (allUsersResult.IsFailure)
    //        {
    //            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(allUsersResult.Error);
    //        }

    //        var filteredUsers = allUsersResult.Value.Where(u => u.UserRole.Equals(accessLevel, StringComparison.OrdinalIgnoreCase));
    //        return Result<IEnumerable<SMSStakeholderUser>>.Success(filteredUsers);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
    //        return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
    //    }
    //}

    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersWithMinimumAccessAsync(string minimumAccessLevel)
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(allUsersResult.Error);
            }

            var levels = new[] { "Limited", "Standard", "Extended", "Full" };
            var requiredLevelIndex = Array.IndexOf(levels, minimumAccessLevel);

            var filteredUsers = allUsersResult.Value.Where(u =>
            {
                var userLevelIndex = Array.IndexOf(levels, u.UserRole);
                return userLevelIndex >= requiredLevelIndex;
            });

            return Result<IEnumerable<SMSStakeholderUser>>.Success(filteredUsers);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    //Legit
    public async Task<Result<Dictionary<string, int>>> GetStakeholderTypeStatisticsAsync()
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(allUsersResult.Error);
            }

            var stats = allUsersResult.Value
                .GroupBy(u => u.StakeholderType)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result<Dictionary<string, int>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }


    //Legit
    public async Task<Result<Dictionary<string, int>>> GetOrganizationStatisticsAsync()
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (allUsersResult.IsFailure)
            {
                return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(allUsersResult.Error);
            }

            var stats = allUsersResult.Value
                .GroupBy(u => u.Organization)
                .ToDictionary(g => g.Key, g => g.Count());

            return Result<Dictionary<string, int>>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<Dictionary<string, int>>.Failure<Dictionary<string, int>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }


    //Legit
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSStakeholderUser_GetByGroupCode} GroupCode:{groupCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_GetByGroupCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));

            var users = new List<SMSStakeholderUser>();
            var userRoles = new Dictionary<string, SMSUserRole>();
            var userRolePermissions = new Dictionary<string, List<SMSUserRolePermission>>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSStakeholderUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSStakeholderUser(reader);
                users.Add(user);
            }

            // Dataset 2: SMSUserRole data - collect unique roles
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var role = Mappers.MapToSMSUserRole(reader);
                    if (!userRoles.ContainsKey(role.Code.Trim()))
                    {
                        userRoles[role.Code] = role;
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data - collect permissions by role code with deduplication
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    var roleCode = permission.SMSUserRoleCode.Trim();

                    if (!userRolePermissions.ContainsKey(roleCode))
                    {
                        userRolePermissions[roleCode] = new List<SMSUserRolePermission>();
                    }

                    // Check for duplicates before adding
                    var existingPermission = userRolePermissions[roleCode]
                        .FirstOrDefault(p => p.SMSModule == permission.SMSModule);

                    if (existingPermission == null)
                    {
                        userRolePermissions[roleCode].Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            // Now assign roles and permissions to users
            foreach (var user in users)
            {
                // Get the role for this user
                if (userRoles.TryGetValue(user.UserRole.Code, out var userRole))
                {
                    // Clone the role to avoid reference issues
                    user.UserRole = new SMSUserRole(new SMSUserRoleID(user.UserRole.Code))
                    {
                        Code = userRole.Code,
                        Name = userRole.Name,
                        //Description = userRole.Description,
                        //IsActive = userRole.IsActive,
                        CreatedBy = userRole.CreatedBy,
                        CreatedDate = userRole.CreatedDate,
                        UpdatedBy = userRole.UpdatedBy,
                        UpdatedDate = userRole.UpdatedDate,
                        Permissions = new List<SMSUserRolePermission>()
                    };

                    // Get permissions for this role
                    if (userRolePermissions.TryGetValue(user.UserRole.Code, out var permissions))
                    {
                        user.UserRole.Permissions = permissions.ToList();
                    }
                }
            }

            return Result<IEnumerable<SMSStakeholderUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderUserError.NotFound);
        }
    }

    // 🔐 Two-Factor Authentication Repository Methods

    /// <summary>
    /// Setup 2FA for a stakeholder user (first-time setup)
    /// </summary>
    public async Task<Result<bool>> Setup2FAAsync(string userCode, string secretKey, string? backupCodes = null, string updatedBy = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(secretKey))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Setup2FA for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Setup2FA, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter("@pUserCode", userCode));
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Update failed 2FA attempts and optionally set lockout
    /// </summary>
    public async Task<Result<bool>> Update2FAFailedAttemptsAsync(string userCode, int failedAttempts, DateTime? lockoutUntil = null, string updatedBy = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Update2FAFailedAttempts for user: {userCode}, Attempts: {failedAttempts}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Update2FAFailedAttempts, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter("@pUserCode", userCode));
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Reset failed 2FA attempts (called on successful 2FA verification)
    /// </summary>
    public async Task<Result<bool>> Reset2FAFailedAttemptsAsync(string userCode, string updatedBy = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Reset2FAFailedAttempts for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Reset2FAFailedAttempts, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter("@pUserCode", userCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedBy", updatedBy));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Disable 2FA for a stakeholder user
    /// </summary>
    public async Task<Result<bool>> Disable2FAAsync(string userCode, string updatedBy = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Disable2FA for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUser_Disable2FA, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter("@pUserCode", userCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pUpdatedBy", updatedBy));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderUserError.UpdateFailed);
        }
    }
}

/// <summary>
/// Repository implementation for SQL-backed SMS Stakeholder User Title operations.
/// </summary>
public sealed class SMSStakeholderUserTitleRepository : BaseRepository<SMSStakeholderUserTitleRepository, SMSStakeholderUserTitle>, ISMSStakeholderUserTitleRepository
{
    private readonly ILogger<SMSStakeholderUserTitleRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSStakeholderUserTitleRepository(ILogger<SMSStakeholderUserTitleRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Stakeholder User Title Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSStakeholderUserTitle>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSStakeholderUserTitle_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSStakeholderUserTitle_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var stakeholderTitles = new List<SMSStakeholderUserTitle>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                stakeholderTitles.Add(Mappers.MapToSMSStakeholderUserTitle(reader));
            }

            await sql.CloseAsync().ConfigureAwait(false);

            SMSStakeholderUserTitle.SetTitles(stakeholderTitles);

            return Result<IEnumerable<SMSStakeholderUserTitle>>.Success(stakeholderTitles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUserTitle>>.Failure<IEnumerable<SMSStakeholderUserTitle>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
