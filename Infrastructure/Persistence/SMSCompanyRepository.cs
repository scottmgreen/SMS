//-----------------------------------------------------------------------
// <copyright file="SMSCompanyRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementation for SQL-backed SMS Company operations.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.Interfaces;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class SMSCompanyRepository : BaseRepository<SMSCompanyRepository, SMSCompany>, ISMSCompanyRepository
{
    private readonly ILogger<SMSCompanyRepository> _logger;
    private readonly string _logHeader;
    private readonly string _connectionString;

    public SMSCompanyRepository(ILogger<SMSCompanyRepository> logger, ILogSupport logSupport, IConfiguration configuration)
        : base(logger, logSupport, configuration)
    {
        _logger = base.Logger;
        _logHeader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logHeader} SMS Company Repository Initialized");
    }

    public async Task<Result<IEnumerable<SMSCompany>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSCompanies_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSCompanies_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var companies = new List<SMSCompany>();

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                companies.Add(Mappers.MapToSMSCompany(reader));
            }

            await sql.CloseAsync().ConfigureAwait(false);

            SMSCompany.SetCompanies(companies);

            return Result<IEnumerable<SMSCompany>>.Success(companies.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSCompany>>.Failure<IEnumerable<SMSCompany>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSCompany>> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructureGetItem($"{_logHeader} {StoredProcs.pr_SMSCompany_GetByCode} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSCompany_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyCode, code));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);

            SMSCompany? company = null;
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                company = Mappers.MapToSMSCompany(reader);
            }

            await sql.CloseAsync().ConfigureAwait(false);

            return company is not null
                ? Result<SMSCompany>.Success(company)
                : Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.DepartmentNotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSCompany>> CreateAsync(SMSCompany company, string createdBy, CancellationToken ct = default)
    {
        try
        {
            if (company is null)
            {
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logHeader} {StoredProcs.pr_SMSCompany_Insert}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSCompany_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyCode, company.Value ?? string.Empty));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyName, company.Company));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyDescription, company.ServiceProvided ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyContactName, company.ContactName ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyContactEmail, company.Email ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyContactPhone, company.Phone ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyInternalRepresentative, company.PortRep ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, string.IsNullOrWhiteSpace(createdBy) ? "SYSTEM" : createdBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter(ParameterNames.pmNewID, SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCompanyCode = new SqlParameter(ParameterNames.pmNewCompanyCode, SqlDbType.VarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCompanyCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            var createdCode = Convert.ToString(newCompanyCode.Value) ?? company.Value;
            return await GetByCodeAsync(createdCode, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SMSCompany>> UpdateAsync(SMSCompany company, string updatedBy, CancellationToken ct = default)
    {
        try
        {
            if (company is null || string.IsNullOrWhiteSpace(company.Value))
            {
                return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.SMSDepartmentError.InvalidDepartment);
            }

            _logger.LogInfrastructurePutItem($"{_logHeader} {StoredProcs.pr_SMSCompany_Update} Code:{company.Value}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSCompany_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyCode, company.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyName, company.Company));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyDescription, company.ServiceProvided ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyContactName, company.ContactName ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyContactEmail, company.Email ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyContactPhone, company.Phone ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyInternalRepresentative, company.PortRep ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, string.IsNullOrWhiteSpace(updatedBy) ? "SYSTEM" : updatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetByCodeAsync(company.Value, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logHeader} {ex.Message}", null);
            return Result<SMSCompany>.Failure<SMSCompany>(DomainErrors.GeneralError.UnProcessableRequest);
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

            _logger.LogInfrastructureDeleteItem($"{_logHeader} {StoredProcs.pr_SMSCompany_Delete} Code:{code}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSCompany_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSMSCompanyCode, code));
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
