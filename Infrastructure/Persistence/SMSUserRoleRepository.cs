//-----------------------------------------------------------------------
// <copyright file="SMSUserRoleRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsuserrole entities with CRUD operations and business queries.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SMS User Role operations
/// </summary>
public sealed class SMSUserRoleRepository : BaseRepository<SMSUserRoleRepository, SMSUserRole>, ISMSUserRoleRepository
{
    private readonly ILogger<SMSUserRoleRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSUserRoleRepository(ILogger<SMSUserRoleRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS User Role Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode.Trim());
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetAllActiveAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetAll} (Active Only)", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleIsActive, true));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<SMSUserRole>> GetByIdAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetById} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id));

            SMSUserRole? userRole = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                userRole = Mappers.MapToSMSUserRole(reader);
            }

            // Dataset 2: SMSUserRolePermissions data (multiple rows)
            if (userRole != null && await reader.NextResultAsync().ConfigureAwait(false))
            {
                var permissions = new List<SMSUserRolePermission>();
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var permission = Mappers.MapToSMSUserRolePermission(reader);
                    permissions.Add(permission);
                }
                userRole.Permissions = permissions;
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (userRole is not null)
            {
                return Result<SMSUserRole>.Success(userRole);
            }
            else
            {
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetByApplicationUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetByApplicationUserId} UserId:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetByApplicationUserId, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserId, userId));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetByStakeholderUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetByStakeholderUserId} UserId:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetByStakeholderUserId, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserId, userId));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetByUserId} UserId:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetByUserId, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserId, userId));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetActiveRolesByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetActiveByUserId} UserId:{userId}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetActiveByUserId, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserId, userId));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetByDepartmentAsync(string department)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(department))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetByDepartment} Department:{department}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetByDepartment, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleDepartment, department));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetByRoleValueAsync(string roleValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(roleValue))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetByRole} RoleValue:{roleValue}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetByRole, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleSMSRoleCode, roleValue));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetExpiringRolesAsync(DateTime cutoffDate)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetExpiringRoles} CutoffDate:{cutoffDate}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetExpiringRoles, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleCutoffDate, cutoffDate));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSUserRole>>> GetByUserTypeAsync(string userType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userType))
            {
                return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} pr_SMSUserRole_GetByUserType UserType:{userType}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("pr_SMSUserRole_GetByUserType", sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserType, userType));

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            // Dataset 1: SMSUserRole data
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
            }

            // Dataset 2: SMSUserRolePermissions data for each user role
            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var userRoleCode = reader.GetString(FieldNames.fSMSUserRolePermissionSMSUserRoleCode);
                    var userRole = userRoles.FirstOrDefault(ur => ur.Code == userRoleCode);
                    if (userRole != null)
                    {
                        if (userRole.Permissions == null)
                            userRole.Permissions = new List<SMSUserRolePermission>();

                        var permission = Mappers.MapToSMSUserRolePermission(reader);
                        userRole.Permissions.Add(permission);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSUserRole>>.Success(userRoles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSUserRole>>.Failure<IEnumerable<SMSUserRole>>(DomainErrors.SMSUserRoleError.NotFound);
        }
    }

    public async Task<Result<SMSUserRole>> AddAsync(SMSUserRole userRole)
    {
        try
        {
            if (userRole is null)
            {
                return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_Insert} Code:{userRole.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleCode, userRole.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRoleName, userRole.Name.Trim()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, userRole.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, userRole.CreatedDate));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewSMSUserRoleCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);



            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);


            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;


            if (userRole.Permissions != null)
            {
                foreach (var permission in userRole.Permissions)
                {
                    using var permCmd = new SqlCommand(StoredProcs.pr_SMSUserRolePermission_Insert, sql)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionCode, permission.Code));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionSMSUserRoleCode, newCodeValue));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionModule, permission.SMSModule));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionCreate, permission.Create));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionRead, permission.Read));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionUpdate, permission.Update));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionDelete, permission.Delete));

                    var newPermissionID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                    var newPermissionCode = new SqlParameter("@pNewPermissionCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
                    permCmd.Parameters.Add(newPermissionID);
                    permCmd.Parameters.Add(newPermissionCode);

                    await permCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }


            await sql.CloseAsync().ConfigureAwait(false);



            return await GetByIdAsync(newCodeValue).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSUserRole>.Failure<SMSUserRole>(DomainErrors.SMSUserRoleError.CreateFailed);
        }
    }

    public async Task<Result<bool>> UpdateAsync(SMSUserRole userRole)
    {
        try
        {
            if (userRole is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_Update} Code:{userRole.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            await sql.OpenAsync().ConfigureAwait(false);

            // Update the main user role
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleCode, userRole.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRoleName, userRole.Name));


            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            // Update each permission
            if (userRole.Permissions != null)
            {
                foreach (var permission in userRole.Permissions)
                {
                    using var permCmd = new SqlCommand(StoredProcs.pr_SMSUserRolePermission_Update, sql)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionCode, permission.Code));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionSMSUserRoleCode, permission.SMSUserRoleCode));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionModule, permission.SMSModule));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionCreate, permission.Create));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionRead, permission.Read));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionUpdate, permission.Update));
                    permCmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRolePermissionDelete, permission.Delete));

                    await permCmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_Delete} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSUserRoleError.DeleteFailed);
        }
    }

    public async Task<Result<UserRoleStatistics>> GetUserRoleStatisticsAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_GetStatsReport}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_GetStatsReport, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            var stats = new UserRoleStatistics();

            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                stats.TotalAssignments = reader.GetValue<int>("TotalAssignments");
                stats.ActiveAssignments = reader.GetValue<int>("ActiveAssignments");
                stats.ExpiredAssignments = reader.GetValue<int>("ExpiredAssignments");
                stats.ExpiringAssignments = reader.GetValue<int>("ExpiringAssignments");
                stats.LastAssignmentDate = reader.IsDBNull("LastAssignmentDate") ? null : reader.GetDateTime("LastAssignmentDate");
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<UserRoleStatistics>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<UserRoleStatistics>.Failure<UserRoleStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
