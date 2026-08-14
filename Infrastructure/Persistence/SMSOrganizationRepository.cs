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
public sealed class SMSOrganizationRepository : BaseRepository<SMSOrganizationRepository, SMSDepartment>, ISMSOrganizationRepository
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

    public async Task<Result<IEnumerable<SMSDepartment>>> GetAllAsync()
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logHeader} {StoredProcs.pr_SMSOrginization_GetAll}", null);

            using var sql = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(StoredProcs.pr_SMSOrginization_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            var departments = new List<SMSDepartment>();
            var responsibilitiesByCode = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            await sql.OpenAsync().ConfigureAwait(false);
            using var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                departments.Add(Mappers.MapToSMSDepartment(reader));
            }

            if (await reader.NextResultAsync().ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var departmentCode = reader.GetValue<string>(FieldNames.fSMSDepartmentResponsibilityDepartmentCode);
                    var responsibility = reader.GetValue<string>(FieldNames.fSMSDepartmentResponsibilityValue);

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
                .Select(d => SMSDepartment.Create(
                    d.Value,
                    d.Name,
                    d.Description,
                    responsibilitiesByCode.TryGetValue(d.Value, out var responsibilities)
                        ? responsibilities.ToArray()
                        : Array.Empty<string>()))
                .ToList();

            SMSDepartment.SetDepartments(hydratedDepartments);

            return Result<IEnumerable<SMSDepartment>>.Success(hydratedDepartments.AsEnumerable());
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logHeader} {ex.Message}", null);
            return Result<IEnumerable<SMSDepartment>>.Failure<IEnumerable<SMSDepartment>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}
