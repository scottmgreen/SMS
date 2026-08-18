//-----------------------------------------------------------------------
// <copyright file="SMSJobTitleRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementation for SQL-backed SMS Job Title operations.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class SMSJobTitleRepository : BaseRepository<SMSJobTitleRepository, SMSJobTitle>, ISMSJobTitleRepository
{
    private readonly ILogger<SMSJobTitleRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSJobTitleRepository(ILogger<SMSJobTitleRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Job Title Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSJobTitle>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSTitle_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSTitle_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var titles = new List<SMSJobTitle>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                titles.Add(Mappers.MapToSMSJobTitle(reader));
            }

            await sql.CloseAsync().ConfigureAwait(false);

            SMSJobTitle.SetTitles(titles);

            return Result<IEnumerable<SMSJobTitle>>.Success(titles.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSJobTitle>>.Failure<IEnumerable<SMSJobTitle>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSJobTitle>> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSTitle_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSTitle_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSJobTitleCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            SMSJobTitle? title = null;
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                title = Mappers.MapToSMSJobTitle(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return title is not null
                ? Result<SMSJobTitle>.Success(title)
                : Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.DepartmentNotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSJobTitle>> CreateAsync(SMSJobTitle title, string createdBy, CancellationToken ct = default)
    {
        try
        {
            if (title is null)
            {
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSTitle_Insert}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSTitle_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSJobTitleCode, title.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSJobTitleName, title.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, string.IsNullOrWhiteSpace(createdBy) ? string.Empty : createdBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter(ParameterNames.pmNewID, SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            var newCode = new SqlParameter("@pNewCode", SqlDbType.VarChar, 50)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            var createdCode = Convert.ToString(newCode.Value) ?? title.Value;
            return await GetByCodeAsync(createdCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSJobTitle>> UpdateAsync(string code, SMSJobTitle title, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || title is null)
            {
                return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSTitle_Update} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSTitle_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSJobTitleCode, code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSJobTitleName, title.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, string.IsNullOrWhiteSpace(updatedBy) ? string.Empty : updatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSJobTitle>.Failure<SMSJobTitle>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<bool>> DeleteAsync(string code, string deletedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<bool>.Failure<bool>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSTitle_Delete} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSTitle_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSJobTitleCode, code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmDeletedBy, string.IsNullOrWhiteSpace(deletedBy) ? string.Empty : deletedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logHeader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}