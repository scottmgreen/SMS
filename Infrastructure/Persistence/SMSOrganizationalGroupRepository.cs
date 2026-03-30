//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsorganizationalgroup entities with stored procedure integration.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SMS Organizational Group operations
/// </summary>
public sealed class SMSOrganizationalGroupRepository : BaseRepository<SMSOrganizationalGroupRepository, SMSOrganizationalGroup>, ISMSOrganizationalGroupRepository
{
    private readonly ILogger<SMSOrganizationalGroupRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSOrganizationalGroupRepository(ILogger<SMSOrganizationalGroupRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Organizational Group Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalGroup_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalGroup_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var groups = new List<SMSOrganizationalGroup>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var group = Mappers.MapToSMSOrganizationalGroup(reader);
                groups.Add(group);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSOrganizationalGroup>>.Success(groups.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    public async Task<Result<SMSOrganizationalGroup>> GetByCodeAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalGroup_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalGroup_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCode, code));

            SMSOrganizationalGroup? group = null;

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            if (await reader.ReadAsync().ConfigureAwait(false))
            {
                group = Mappers.MapToSMSOrganizationalGroup(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            if (group is not null)
            {
                return Result<SMSOrganizationalGroup>.Success(group);
            }
            else
            {
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganizationalGroup>> AddAsync(SMSOrganizationalGroup group)
    {
        try
        {
            if (group is null)
            {
                return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalGroup_Insert} Code:{group.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalGroup_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCode, group.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupName, group.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupDescription, group.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupGroupType, group.GroupType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupAuthorityLevel, group.AuthorityLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupIsActive, group.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, group.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));


            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewGroupCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);



            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetByCodeAsync(group.Code).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganizationalGroup>.Failure<SMSOrganizationalGroup>(DomainErrors.SMSOrganizationalGroupError.CreateFailed);
        }
    }

    public async Task<Result<bool>> UpdateAsync(SMSOrganizationalGroup group)
    {
        try
        {
            if (group is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalGroup_Update} Code:{group.Code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalGroup_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCode, group.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupName, group.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupDescription, group.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupGroupType, group.GroupType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupAuthorityLevel, group.AuthorityLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupIsActive, group.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, group.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalGroup_Delete} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalGroup_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCode, code));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSOrganizationalGroupError.DeleteFailed);
        }
    }

    #region Group Membership Operations

    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUserGroup_Assign} UserCode:{userCode}, GroupCode:{groupCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUserGroup_Assign, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCodeForAssignment, groupCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAssignedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAssignedDate, DateTime.UtcNow));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUserGroup_Remove} UserCode:{userCode}, GroupCode:{groupCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUserGroup_Remove, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCodeForAssignment, groupCode));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<IEnumerable<SMSOrganizationalUser>>> GetUsersByGroupCodeAsync(string groupCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUserGroup_GetUsersByGroup} GroupCode:{groupCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUserGroup_GetUsersByGroup, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupCodeForAssignment, groupCode));

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
            return Result<IEnumerable<SMSOrganizationalUser>>.Failure<IEnumerable<SMSOrganizationalUser>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSOrganizationalGroup>>> GetGroupsByUserCodeAsync(string userCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganizationalGroups_GetByUserCode} GroupCode:{userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalGroups_GetByUserCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalUserCode, userCode));

            var groups = new List<SMSOrganizationalGroup>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                var group = Mappers.MapToSMSOrganizationalGroup(reader);
                groups.Add(group);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSOrganizationalGroup>>.Success(groups.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganizationalGroup>>.Failure<IEnumerable<SMSOrganizationalGroup>>(DomainErrors.SMSOrganizationalGroupError.NotFound);
        }
    }

    public async Task<Result<bool>> ClearUserGroupsAsync(string userCode, string clearedBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSOrganizationalUserGroup_ClearUserGroups} UserCode:{userCode}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganizationalUserGroup_ClearUserGroups, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationalGroupClearedBy, clearedBy ?? "SYSTEM"));

            await sql.OpenAsync().ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    #endregion
}
