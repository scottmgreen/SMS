//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementation for SQL-backed SMS Organization operations.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for SQL-backed SMS Organization operations.
/// </summary>
public sealed class SMSOrganizationRepository : BaseRepository<SMSOrganizationRepository, SMSOrganization>, ISMSOrganizationRepository
{
    private readonly ILogger<SMSOrganizationRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSOrganizationRepository(ILogger<SMSOrganizationRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Organization Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSOrganization>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrganization_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganization_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var departments = new List<SMSOrganization>();
            var responsibilitiesByCode = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                departments.Add(Mappers.MapToSMSOrganization(reader));
            }

            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var departmentCode = reader.GetValue<string>(FieldNames.fSMSOrganizationResponsibilityOrganziationCode);
                    var responsibility = reader.GetValue<string>(FieldNames.fSMSOrganizationResponsibilityValue);

                    if (string.IsNullOrWhiteSpace(departmentCode) || string.IsNullOrWhiteSpace(responsibility))
                    {
                        continue;
                    }

                    if (!responsibilitiesByCode.TryGetValue(departmentCode, out var responsibilities))
                    {
                        responsibilities = new List<string>();
                        responsibilitiesByCode[departmentCode] = responsibilities;
                    }

                    if (!responsibilities.Contains(responsibility, StringComparer.OrdinalIgnoreCase))
                    {
                        responsibilities.Add(responsibility);
                    }
                }
            }

            await sql.CloseAsync().ConfigureAwait(false);

            var hydratedDepartments = departments
                .Select(d => SMSOrganization.Create(
                    d.Value,
                    d.Name,
                    d.Description,
                    responsibilitiesByCode.TryGetValue(d.Value, out var responsibilities)
                        ? responsibilities.ToArray()
                        : Array.Empty<string>()))
                .ToList();

            SMSOrganization.SetDepartments(hydratedDepartments);

            return Result<IEnumerable<SMSOrganization>>.Success(hydratedDepartments.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSOrganization>>.Failure<IEnumerable<SMSOrganization>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganization>> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSOrganization_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganization_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            SMSOrganization? organization = null;
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                organization = Mappers.MapToSMSOrganization(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return organization is not null
                ? Result<SMSOrganization>.Success(organization)
                : Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.DepartmentNotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganization>> CreateAsync(SMSOrganization organization, string createdBy, CancellationToken ct = default)
    {
        try
        {
            if (organization is null)
            {
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSOrganization_Insert}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganization_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationCode, organization.Value ?? string.Empty));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationValue, organization.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationName, organization.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationDescription, organization.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, string.IsNullOrWhiteSpace(createdBy) ? "SYSTEM" : createdBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter(ParameterNames.pmNewID, SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newOrganizationCode = new SqlParameter(ParameterNames.pmNewOrganizationCode, SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newOrganizationCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            var createdCode = Convert.ToString(newOrganizationCode.Value) ?? organization.Value;
            return await GetByCodeAsync(createdCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSOrganization>> UpdateAsync(string code, SMSOrganization organization, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || organization is null)
            {
                return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSOrganization_Update} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganization_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationCode, code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationValue, organization.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationName, organization.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationDescription, organization.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, string.IsNullOrWhiteSpace(updatedBy) ? "SYSTEM" : updatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetByCodeAsync(code, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSOrganization>.Failure<SMSOrganization>(DomainErrors.GeneralError.UnProcessableRequest);
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

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSOrganization_Delete} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrganization_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSOrganizationCode, code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmDeletedBy, string.IsNullOrWhiteSpace(deletedBy) ? "SYSTEM" : deletedBy));

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
