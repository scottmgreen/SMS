//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS smsstakeholdergroup entities with group management functionality.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SMS Stakeholder Group operations
/// </summary>
public sealed class SMSStakeholderGroupRepository : BaseRepository<SMSStakeholderGroupRepository, SMSStakeholderGroup>
{
    private readonly ILogger<SMSStakeholderGroupRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SMSStakeholderGroupRepository(ILogger<SMSStakeholderGroupRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} SMS Stakeholder Group Repository Initialized");
    }

    /// <summary>
    /// Creates a new SMS stakeholder group
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> CreateAsync(SMSStakeholderGroup stakeholderGroup, CancellationToken ct = default)
    {
        try
        {
            if (stakeholderGroup == null)
            {
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_Insert} Code:{stakeholderGroup.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, stakeholderGroup.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderGroupName, stakeholderGroup.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderGroupDescription, stakeholderGroup.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, stakeholderGroup.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, stakeholderGroup.CreatedDate));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewGroupCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            string newCodeValue = Convert.ToString(newCode.Value) ?? stakeholderGroup.Code;

            return await GetByCodeAsync(newCodeValue, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Updates an existing SMS stakeholder group
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> UpdateAsync(SMSStakeholderGroup stakeholderGroup, CancellationToken ct = default)
    {
        try
        {
            if (stakeholderGroup == null)
            {
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_Update} Code:{stakeholderGroup.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, stakeholderGroup.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderGroupName, stakeholderGroup.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderGroupDescription, stakeholderGroup.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderGroupIsActive, stakeholderGroup.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, stakeholderGroup.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, stakeholderGroup.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<SMSStakeholderGroup>.Success(stakeholderGroup);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS stakeholder group
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_Delete} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Trim()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets all SMS stakeholder groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SMSStakeholderGroup> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var group = Mappers.MapToSMSStakeholderGroup(reader);
                    response.Add(group);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSStakeholderGroup>>.Success((IEnumerable<SMSStakeholderGroup>)response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets SMS stakeholder groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderGroup>>> GetGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_GetByUserID} UserCode:{userCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_GetByUserID, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            List<SMSStakeholderGroup> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var group = Mappers.MapToSMSStakeholderGroup(reader);
                    response.Add(group);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSStakeholderGroup>>.Success((IEnumerable<SMSStakeholderGroup>)response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderGroup>>.Failure<IEnumerable<SMSStakeholderGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets an SMS stakeholder group by code
    /// </summary>
    public async Task<Result<SMSStakeholderGroup>> GetByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_GetByCode} Code:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            SMSStakeholderGroup? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToSMSStakeholderGroup(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<SMSStakeholderGroup>.Success(response);
            }
            else
            {
                return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.SMSStakeholderGroupError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<SMSStakeholderGroup>.Failure<SMSStakeholderGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Assigns a user to a stakeholder group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_Assign} UserCode:{userCode}, GroupCode:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_Assign, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserCodeForAssignment, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAssignedBy, assignedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmAssignedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from a stakeholder group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_Remove} UserCode:{userCode}, GroupCode:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_Remove, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSStakeholderUserCodeForAssignment, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.RemovalFailed);
        }
    }

    /// <summary>
    /// Clears all group memberships for a user
    /// </summary>
    public async Task<Result<bool>> ClearUserGroupsAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_ClearUserGroups} UserCode:{userCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_ClearUserGroups, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSStakeholderGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Gets users by SMS Stakeholder Group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSStakeholderUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.SMSStakeholderGroupError.CodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSStakeholderUserGroup_GetUsersByGroup} GroupCode:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSStakeholderUserGroup_GetUsersByGroup, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            List<SMSStakeholderUser> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var user = Mappers.MapToSMSStakeholderUser(reader);
                    response.Add(user);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSStakeholderUser>>.Success((IEnumerable<SMSStakeholderUser>)response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSStakeholderUser>>.Failure<IEnumerable<SMSStakeholderUser>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
