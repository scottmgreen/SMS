using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Models;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                if (userRole.IsCurrentlyActive()) // Additional domain-level filtering
                {
                    userRoles.Add(userRole);
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
            
            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                userRole = Mappers.MapToSMSUserRole(reader);
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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleSMSRoleCode, roleValue)); // ? CORRECTED: Use correct stored proc and parameter

            var userRoles = new List<SMSUserRole>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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
            
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var userRole = Mappers.MapToSMSUserRole(reader);
                userRoles.Add(userRole);
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

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_Insert} UserId:{userRole.UserID}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleCode, userRole.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserId, userRole.UserID));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleUserType, userRole.UserType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleSMSRoleCode, userRole.RoleValue)); // ? CORRECTED: Use SMSRoleCode FK
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleDepartment, userRole.Department));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleEffectiveDate, userRole.EffectiveDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleExpirationDate, userRole.ExpirationDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleIsActive, userRole.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleAssignedBy, userRole.AssignedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleAssignedDate, DateTime.UtcNow)); // ? CORRECTED: Added AssignedDate
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, userRole.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, userRole.CreatedDate));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
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

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_Update} ID:{userRole.Id.Value}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, userRole.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleExpirationDate, userRole.ExpirationDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleIsActive, userRole.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleDeactivatedBy, userRole.DeactivatedBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleDeactivatedDate, userRole.DeactivatedDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, userRole.UpdatedBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, userRole.UpdatedDate ?? (object)DBNull.Value));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
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

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSUserRole_Deactivate} ID:{id}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSUserRole_Deactivate, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleDeactivatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSUserRoleDeactivatedDate, DateTime.UtcNow));

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

    public async Task<Result<bool>> UserHasRoleAsync(string userId, string roleValue)
    {
        try
        {
            var activeRolesResult = await GetActiveRolesByUserIdAsync(userId);
            if (activeRolesResult.IsFailure)
            {
                return Result<bool>.Failure<bool>(activeRolesResult.Error);
            }

            var hasRole = activeRolesResult.Value.Any(ur => ur.RoleValue.Equals(roleValue, StringComparison.OrdinalIgnoreCase));
            return Result<bool>.Success(hasRole);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
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