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
}
