using Microsoft.Data.SqlClient;
using SMS_Domain.Entities;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using SMS_Shared.Common;
using System.Data;
using SMS_Domain.Errors;

namespace SMS_Infrastructure.Persistence;

public sealed class SMSApplicationGroupRepository : BaseRepository<SMSApplicationGroupRepository, SMSApplicationGroup>
{
    private readonly ILogger<SMSApplicationGroupRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SMSApplicationGroupRepository(ILogger<SMSApplicationGroupRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} SMS Application Group Repository Initialized");
    }

    /// <summary>
    /// Creates a new SMS application group
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> CreateAsync(SMSApplicationGroup applicationGroup, CancellationToken ct = default)
    {
        try
        {
            if (applicationGroup == null)
            {
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_Insert} Code:{applicationGroup.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupCode, applicationGroup.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupName, applicationGroup.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupDescription, applicationGroup.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, applicationGroup.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, applicationGroup.CreatedDate));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewGroupCode", SqlDbType.VarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            string newCodeValue = Convert.ToString(newCode.Value) ?? applicationGroup.Code;

            return await GetByCodeAsync(newCodeValue, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CreateFailed);
        }
    }

    /// <summary>
    /// Updates an existing SMS application group
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> UpdateAsync(SMSApplicationGroup applicationGroup, CancellationToken ct = default)
    {
        try
        {
            if (applicationGroup == null)
            {
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_Update} Code:{applicationGroup.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, applicationGroup.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupName, applicationGroup.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupDescription, applicationGroup.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupIsActive, applicationGroup.IsActive));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, applicationGroup.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, applicationGroup.UpdatedDate ?? DateTime.UtcNow));
            cmd.Parameters.Add(DataAccess.Parameter("@pRowsAffected",0, null));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<SMSApplicationGroup>.Success(applicationGroup);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.UpdateFailed);
        }
    }

    /// <summary>
    /// Deletes an SMS application group
    /// </summary>
    public async Task<Result<bool>> DeleteAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_Delete} Code:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter("@pRowsAffected", 0,null));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets all SMS application groups
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            List<SMSApplicationGroup> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var group = Mappers.MapToSMSApplicationGroup(reader);
                    response.Add(group);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationGroup>>.Success((IEnumerable<SMSApplicationGroup>)response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets SMS application groups by user code
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_GetByUserID} UserCode:{userCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_GetByUserID, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            List<SMSApplicationGroup> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var group = Mappers.MapToSMSApplicationGroup(reader);
                    response.Add(group);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationGroup>>.Success((IEnumerable<SMSApplicationGroup>)response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets an SMS application group by code
    /// </summary>
    public async Task<Result<SMSApplicationGroup>> GetByCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_GetByCode} Code:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            SMSApplicationGroup? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToSMSApplicationGroup(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<SMSApplicationGroup>.Success(response);
            }
            else
            {
                return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.SMSApplicationGroupError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<SMSApplicationGroup>.Failure<SMSApplicationGroup>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Assigns a user to an application group
    /// </summary>
    public async Task<Result<bool>> AssignUserToGroupAsync(string userCode, string groupCode, string assignedBy = "SYSTEM", CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SMSApplicationUserGroup_Assign} UserCode:{userCode}, GroupCode:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationUserGroup_Assign, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupCode, groupCode));
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
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.AssignmentFailed);
        }
    }

    /// <summary>
    /// Removes a user from an application group
    /// </summary>
    public async Task<Result<bool>> RemoveUserFromGroupAsync(string userCode, string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode) || string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSApplicationUserGroup_Remove} UserCode:{userCode}, GroupCode:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationUserGroup_Remove, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupCode, groupCode));
            cmd.Parameters.Add(DataAccess.Parameter("@pRemovedBy", "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter("@pRowsAffected", 0,null));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.RemovalFailed);
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
                return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.UserCodeRequired);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SMSApplicationUserGroup_ClearUserGroups} UserCode:{userCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationUserGroup_ClearUserGroups, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupUserCode, userCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupClearedBy, "SYSTEM"));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SMSApplicationGroupError.ClearGroupsFailed);
        }
    }

    /// <summary>
    /// Gets all SMS application groups with their member information
    /// </summary>
    public async Task<Result<(IEnumerable<SMSApplicationGroup> Groups, Dictionary<string, List<SMSApplicationUser>> GroupMembers)>> GetAllWithMembersAsync(bool activeOnly = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_GetAll} with members", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter("@pActiveOnly", activeOnly));
            cmd.Parameters.Add(DataAccess.Parameter("@pRequestedBy", "SYSTEM"));

            List<SMSApplicationGroup> groups = new();
            Dictionary<string, List<SMSApplicationUser>> groupMembers = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);

            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                // First dataset: Groups with member counts
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var group = Mappers.MapToSMSApplicationGroup(reader);
                    groups.Add(group);
                }

                // Move to next dataset: Group members (if it exists)
                if (await reader.NextResultAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var user = Mappers.MapToSMSApplicationUser(reader);
                        var groupCode = reader.GetValue<string>("GroupCode") ?? string.Empty;

                        if (!groupMembers.ContainsKey(groupCode))
                        {
                            groupMembers[groupCode] = new List<SMSApplicationUser>();
                        }

                        groupMembers[groupCode].Add(user);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return Result<(IEnumerable<SMSApplicationGroup>, Dictionary<string, List<SMSApplicationUser>>)>.Success(((IEnumerable<SMSApplicationGroup>)groups, groupMembers));
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<(IEnumerable<SMSApplicationGroup>, Dictionary<string, List<SMSApplicationUser>>)>.Failure<(IEnumerable<SMSApplicationGroup>, Dictionary<string, List<SMSApplicationUser>>)>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    /// <summary>
    /// Gets users by application group code
    /// </summary>
    public async Task<Result<IEnumerable<SMSApplicationUser>>> GetUsersByGroupCodeAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSApplicationUserGroup_GetUsersByGroup} GroupCode:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationUserGroup_GetUsersByGroup, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupCode, groupCode));

            List<SMSApplicationUser> users = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var user = Mappers.MapToSMSApplicationUser(reader);
                    users.Add(user);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationUser>>.Success((IEnumerable<SMSApplicationUser>)users);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationUser>>.Failure<IEnumerable<SMSApplicationUser>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<SMSApplicationGroup>>> GetGroupsByUserCodeAsync(string userCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SMSApplicationGroups_GetByUserCode} GroupCode:{userCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroups_GetByUserCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSApplicationGroupUserCode, userCode));

            List<SMSApplicationGroup> groups = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var group = Mappers.MapToSMSApplicationGroup(reader);
                    groups.Add(group);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SMSApplicationGroup>>.Success((IEnumerable<SMSApplicationGroup>)groups);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<IEnumerable<SMSApplicationGroup>>.Failure<IEnumerable<SMSApplicationGroup>>(DomainErrors.SMSApplicationGroupError.NotFound);
        }
    }




    /// <summary>
    /// Gets an SMS application group by code with its members
    /// </summary>
    public async Task<Result<(SMSApplicationGroup Group, List<SMSApplicationUser> Members)>> GetByCodeWithMembersAsync(string groupCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupCode))
            {
                return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Failure<(SMSApplicationGroup, List<SMSApplicationUser>)>(DomainErrors.SMSApplicationGroupError.CodeRequired);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SMSApplicationGroup_GetByCode} with members Code:{groupCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SMSApplicationGroup_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, groupCode));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUserId, "SYSTEM"));

            SMSApplicationGroup? group = null;
            List<SMSApplicationUser> members = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                // First dataset: Group details
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    group = Mappers.MapToSMSApplicationGroup(reader);
                }

                // Move to second dataset: Group members using existing mapper - NO CHANGES NEEDED!
                if (await reader.NextResultAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var user = Mappers.MapToSMSApplicationUser(reader);
                        members.Add(user);
                    }
                }
            }
            
            await sql.CloseAsync().ConfigureAwait(false);

            if (group is not null)
            {
                return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Success((group, members));
            }
            else
            {
                return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Failure<(SMSApplicationGroup, List<SMSApplicationUser>)>(DomainErrors.SMSApplicationGroupError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<(SMSApplicationGroup, List<SMSApplicationUser>)>.Failure<(SMSApplicationGroup, List<SMSApplicationUser>)>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}