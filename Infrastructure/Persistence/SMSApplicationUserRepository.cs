//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsapplicationuser entities with CRUD operations and business queries.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Infrastructure.Interfaces;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;


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

            // Dataset 1: SMSApplicationUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSApplicationUser(reader);
                users.Add(user);
            }

            
            // Dataset 2: SMSUserRole data for users that have roles
            var rolesDictionary = new Dictionary<string, SMSUserRole>();
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var role = Mappers.MapToSMSUserRole(reader);
                    if (!rolesDictionary.ContainsKey(role.Code))
                    {
                        rolesDictionary[role.Code] = role;
                    }
                }
            }
            
            // Dataset 3: SMSUserRolePermissions data
            var permissionsDictionary = new Dictionary<string, List<SMSUserRolePermission>>();
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    var roleCode = permission.SMSUserRoleCode?.Trim();

                    if (!string.IsNullOrEmpty(roleCode))
                    {
                        if (!permissionsDictionary.ContainsKey(roleCode))
                        {
                            permissionsDictionary[roleCode] = new List<SMSUserRolePermission>();
                        }
                        permissionsDictionary[roleCode].Add(permission);
                    }
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);
            // Now assign roles and permissions to users
            foreach (var user in users)
            {
                if (user.UserRole != null && !string.IsNullOrEmpty(user.UserRole.Code?.Trim()))
                {
                    var userRoleCode = user.UserRole.Code.Trim();

                    // Get the complete role information
                    if (rolesDictionary.TryGetValue(userRoleCode, out var completeRole))
                    {
                        user.UserRole = completeRole;

                        // Assign permissions to the role
                        if (permissionsDictionary.TryGetValue(userRoleCode, out var permissions))
                        {
                            user.UserRole.Permissions = permissions;
                        }
                        else
                        {
                            user.UserRole.Permissions = new List<SMSUserRolePermission>();
                        }
                    }
                }
                
            }
            return Result<IEnumerable<SMSApplicationUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    //public async Task<Result<SMSApplicationUser>> GetByIdAsync(SMSApplicationUserID id)
    //{
    //    try
    //    {
    //        if (id?.Value is null || string.IsNullOrWhiteSpace(id.Value))
    //        {
    //            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
    //        }

    //        _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetByCode} ID:{id.Value}", null);

    //        using var sql = new SqlConnection(_connectionString);
    //        using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetByCode, sql)
    //        {
    //            CommandType = CommandType.StoredProcedure
    //        };

    //        cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

    //        SMSApplicationUser? user = null;

    //        await sql.OpenAsync().ConfigureAwait(false);
    //        using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

    //        // Dataset 1: SMSApplicationUser data
    //        if (await reader.ReadAsync().ConfigureAwait(false))
    //        {
    //            user = Mappers.MapToSMSApplicationUser(reader);
    //        }

    //        // Dataset 2: SMSUserRole data
    //        if (user != null && await reader.NextResultAsync().ConfigureAwait(false) && await reader.ReadAsync().ConfigureAwait(false))
    //        {
    //            user.UserRole = Mappers.MapToSMSUserRole(reader);
    //        }

    //        // Dataset 3: SMSUserRolePermissions data (multiple rows)
    //        if (user?.UserRole != null && await reader.NextResultAsync().ConfigureAwait(false))
    //        {
    //            var permissions = new List<SMSUserRolePermission>();
    //            while (await reader.ReadAsync().ConfigureAwait(false))
    //            {
    //                var permission = Mappers.MapToSMSUserRolePermission(reader);
    //                permissions.Add(permission);
    //            }
    //            user.UserRole.Permissions = permissions;
    //        }

    //        await sql.CloseAsync().ConfigureAwait(false);

    //        if (user is not null)
    //        {
    //            return Result<SMSApplicationUser>.Success(user);
    //        }
    //        else
    //        {
    //            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NotFound);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
    //        return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.GeneralError.UnProcessableRequest);
    //    }
    //}

    public async Task<Result<SMSApplicationUser>> GetByCodeAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code));

            SMSApplicationUser? user = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSApplicationUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSApplicationUser(reader);
                
            }


            // Dataset 2: SMSUserRole data for users that have roles
            var rolesDictionary = new Dictionary<string, SMSUserRole>();
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var role = Mappers.MapToSMSUserRole(reader);
                    if (!rolesDictionary.ContainsKey(role.Code))
                    {
                        rolesDictionary[role.Code] = role;
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data
            var permissionsDictionary = new Dictionary<string, List<SMSUserRolePermission>>();
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    var roleCode = permission.SMSUserRoleCode?.Trim();

                    if (!string.IsNullOrEmpty(roleCode))
                    {
                        if (!permissionsDictionary.ContainsKey(roleCode))
                        {
                            permissionsDictionary[roleCode] = new List<SMSUserRolePermission>();
                        }
                        permissionsDictionary[roleCode].Add(permission);
                    }
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);
            // Now assign roles and permissions to users
            //foreach (var user in users)
            //{
                if (user.UserRole != null && !string.IsNullOrEmpty(user.UserRole.Code?.Trim()))
                {
                    var userRoleCode = user.UserRole.Code.Trim();

                    // Get the complete role information
                    if (rolesDictionary.TryGetValue(userRoleCode, out var completeRole))
                    {
                        user.UserRole = completeRole;

                        // Assign permissions to the role
                        if (permissionsDictionary.TryGetValue(userRoleCode, out var permissions))
                        {
                            user.UserRole.Permissions = permissions;
                        }
                        else
                        {
                            user.UserRole.Permissions = new List<SMSUserRolePermission>();
                        }
                    }
                }

            //}

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

            // Dataset 1: SMSApplicationUser data
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                user = Mappers.MapToSMSApplicationUser(reader);
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

            // Dataset 1: SMSApplicationUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSApplicationUser(reader);
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserPassword, user.Password.HashedValue));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserType, user.SMSUserType.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserRole, user.UserRole?.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserIsActive, user.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastLoginDate, user.LastLoginDate));
            // 🔐 NEW: Add 2FA parameters to AddAsync  
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorSecretKey, user.TwoFactorSecretKey));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorEnabled, user.TwoFactorEnabled));
            
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserBackupCodes, user.BackupCodes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorSetupDate , user.TwoFactorSetupDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserFailedTwoFactorAttempts, user.FailedTwoFactorAttempts));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserTwoFactorLockedUntil, user.TwoFactorLockedUntil));
                        
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
            SMSApplicationUserID userId = new(newCodeValue);
            return await GetByIdAsync(userId).ConfigureAwait(false);
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, user.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserFirstName, user.FirstName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastName, user.LastName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserUserName, user.UserName.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserType, user.SMSUserType.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserRole, user.UserRole?.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserIsActive, user.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserLastLoginDate, user.LastLoginDate));
            // 🔐 NEW: Add 2FA parameters to UpdateAsync
            // 🔐 NEW: Add 2FA parameters to AddAsync  
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> UpdatePasswordAsync(BaseUserID code, string hashedPassword)
    {
        try
        {
            if (code?.Value is null || string.IsNullOrWhiteSpace(code.Value))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_UpdatePassword} ID:{code.Value}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_UpdatePassword, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserPassword, hashedPassword));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, code.Value));
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

    public async Task<Result<bool>> RecordLoginAsync(BaseUserID code, DateTime loginDate)
    {
        try
        {
            if (code?.Value is null || string.IsNullOrWhiteSpace(code.Value))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_RecordLogin} ID:{code.Value}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_RecordLogin, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));
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

    // 🔐 Two-Factor Authentication Repository Methods

    /// <summary>
    /// Setup 2FA for a user (first-time setup)
    /// </summary>
    public async Task<Result<bool>> Setup2FAAsync(string userCode, string secretKey, string? backupCodes = null, string updatedBy = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(secretKey))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Setup2FA for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("pr_SMSApplicationUser_Setup2FA", sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
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
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Update2FAFailedAttempts for user: {userCode}, Attempts: {failedAttempts}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("pr_SMSApplicationUser_Update2FAFailedAttempts", sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
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
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Reset2FAFailedAttempts for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("pr_SMSApplicationUser_Reset2FAFailedAttempts", sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    /// <summary>
    /// Disable 2FA for a user
    /// </summary>
    public async Task<Result<bool>> Disable2FAAsync(string userCode, string updatedBy = "")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} Disable2FA for user: {userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("pr_SMSApplicationUser_Disable2FA", sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAsync(BaseUserID code)
    {
        try
        {
            if (code?.Value is null || string.IsNullOrWhiteSpace(code.Value))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_Delete} ID:{code.Value}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

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

    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetBySMSApplicationUserRoleAsync(string applicationRole)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSApplicationUser_GetByRole} Role:{applicationRole}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSApplicationUser_GetByRole, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserRole, applicationRole));

            var users = new List<SMSApplicationUser>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSApplicationUser data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var user = Mappers.MapToSMSApplicationUser(reader);
                users.Add(user);
            }

            // Dataset 2: SMSUserRole data for users that have roles
            var rolesDictionary = new Dictionary<string, SMSUserRole>();
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var role = Mappers.MapToSMSUserRole(reader);
                    if (!rolesDictionary.ContainsKey(role.Code))
                    {
                        rolesDictionary[role.Code] = role;
                    }
                }
            }

            // Dataset 3: SMSUserRolePermissions data
            var permissionsDictionary = new Dictionary<string, List<SMSUserRolePermission>>();
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    var roleCode = permission.SMSUserRoleCode?.Trim();

                    if (!string.IsNullOrEmpty(roleCode))
                    {
                        if (!permissionsDictionary.ContainsKey(roleCode))
                        {
                            permissionsDictionary[roleCode] = new List<SMSUserRolePermission>();
                        }
                        permissionsDictionary[roleCode].Add(permission);
                    }
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);
            // Now assign roles and permissions to users
            foreach (var user in users)
            {
                if (user.UserRole != null && !string.IsNullOrEmpty(user.UserRole.Code?.Trim()))
                {
                    var userRoleCode = user.UserRole.Code.Trim();

                    // Get the complete role information
                    if (rolesDictionary.TryGetValue(userRoleCode, out var completeRole))
                    {
                        user.UserRole = completeRole;

                        // Assign permissions to the role
                        if (permissionsDictionary.TryGetValue(userRoleCode, out var permissions))
                        {
                            user.UserRole.Permissions = permissions;
                        }
                        else
                        {
                            user.UserRole.Permissions = new List<SMSUserRolePermission>();
                        }
                    }
                }
            }
            return Result<IEnumerable<SMSApplicationUser>>.Success(users.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationUserError.NotFound);
        }
    }

    // Legacy method implementations for backward compatibility
    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetByApplicationRoleAsync(string applicationRole)
    {
        return await GetBySMSApplicationUserRoleAsync(applicationRole);
    }

    /// <summary>
    /// Base interface method implementation - required by IBaseUserRepository
    /// Delegates to the string version for consistency
    /// </summary>
    public async Task<Result<SMSApplicationUser>> GetByIdAsync(BaseUserID id)
    {
        if (id?.Value is null || string.IsNullOrWhiteSpace(id.Value))
        {
            return Result<SMSApplicationUser>.Failure<SMSApplicationUser>(DomainErrors.SMSApplicationUserError.NullOrEmpty);
        }

        return await GetByIdAsync(id.Value);
    }
}
