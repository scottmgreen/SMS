using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

public sealed class SafetyPerformanceIndicatorRepository : BaseRepository<SafetyPerformanceIndicatorRepository, SafetyPerformanceIndicator>
{
    private readonly ILogger<SafetyPerformanceIndicatorRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public SafetyPerformanceIndicatorRepository(
        ILogger<SafetyPerformanceIndicatorRepository> logger, 
        ILogSupport logsupport, 
        IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, 
            $"{_logheader} SafetyPerformanceIndicator Repository Initialized");
    }

    #region SPI CRUD Operations

    public async Task<Result<SafetyPerformanceIndicator>> CreateSafetyPerformanceIndicatorAsync(
        SafetyPerformanceIndicator spi, CancellationToken ct = default)
    {
        try
        {
            if (spi is null)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_Insert} Code:{spi.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICode, spi.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIName, spi.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIDescription, spi.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIIndicatorType, spi.IndicatorType.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIStatus, spi.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIMeasurementUnit, spi.MeasurementUnit));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIMeasurementFrequency, spi.MeasurementFrequency.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICalculationMethod, spi.CalculationMethod));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIDataSource, spi.DataSource));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPITargetValue, spi.TargetValue));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIAcceptableRange, spi.AcceptableRange));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIWarningThreshold, spi.WarningThreshold));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICriticalThreshold, spi.CriticalThreshold));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIResponsibleDepartment, spi.ResponsibleDepartment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIDataOwner, spi.DataOwner));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIReviewAuthority, spi.ReviewAuthority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPINextReviewDate, spi.NextReviewDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPILastReviewDate, spi.LastReviewDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPILastReviewNotes, spi.LastReviewNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIAlertsEnabled, spi.AlertsEnabled));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIAlertRecipients, spi.AlertRecipients));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, spi.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewSPICode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            SafetyPerformanceIndicatorID spiId = new(newCodeValue);

            return await GetSafetyPerformanceIndicatorByIdAsync(spiId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.CreateFailed);
        }
    }

    public async Task<Result<SafetyPerformanceIndicator>> UpdateSafetyPerformanceIndicatorAsync(
        SafetyPerformanceIndicator spi, CancellationToken ct = default)
    {
        try
        {
            if (spi is null)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_Update} ID:{spi.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, spi.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICode, spi.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIName, spi.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIDescription, spi.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIIndicatorType, spi.IndicatorType.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIStatus, spi.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIMeasurementUnit, spi.MeasurementUnit));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIMeasurementFrequency, spi.MeasurementFrequency.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICalculationMethod, spi.CalculationMethod));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIDataSource, spi.DataSource));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPITargetValue, spi.TargetValue));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIAcceptableRange, spi.AcceptableRange));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIWarningThreshold, spi.WarningThreshold));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICriticalThreshold, spi.CriticalThreshold));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIResponsibleDepartment, spi.ResponsibleDepartment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIDataOwner, spi.DataOwner));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIReviewAuthority, spi.ReviewAuthority));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPINextReviewDate, spi.NextReviewDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPILastReviewDate, spi.LastReviewDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPILastReviewNotes, spi.LastReviewNotes));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIAlertsEnabled, spi.AlertsEnabled));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIAlertRecipients, spi.AlertRecipients));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, spi.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetSafetyPerformanceIndicatorByIdAsync(new SafetyPerformanceIndicatorID(spi.Id.Value), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteSafetyPerformanceIndicatorAsync(
        SafetyPerformanceIndicatorID spiId, CancellationToken ct = default)
    {
        try
        {
            if (spiId is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_Delete} ID:{spiId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, spiId.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.SPIError.DeleteFailed);
        }
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> GetAllSafetyPerformanceIndicatorsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<SafetyPerformanceIndicator> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var spi = Mappers.MapToSafetyPerformanceIndicator(reader);
                    response.Add(spi);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<IEnumerable<SafetyPerformanceIndicator>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }

    public async Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByIdAsync(
        SafetyPerformanceIndicatorID spiId, CancellationToken ct = default)
    {
        try
        {
            if (spiId is null)
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_GetById} {spiId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, spiId.Value));

            SafetyPerformanceIndicator? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToSafetyPerformanceIndicator(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<SafetyPerformanceIndicator>.Success(response);
            }
            else
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<SafetyPerformanceIndicator>> GetSafetyPerformanceIndicatorByCodeAsync(
        string code, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_GetByCode} {code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPICode, code));

            SafetyPerformanceIndicator? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToSafetyPerformanceIndicator(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<SafetyPerformanceIndicator>.Success(response);
            }
            else
            {
                return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> GetSafetyPerformanceIndicatorsByTypeAsync(
        string indicatorType, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_GetByType} Type: {indicatorType}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_GetByType, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIIndicatorType, indicatorType));

            List<SafetyPerformanceIndicator> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var spi = Mappers.MapToSafetyPerformanceIndicator(reader);
                    response.Add(spi);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<SafetyPerformanceIndicator>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> GetSafetyPerformanceIndicatorsByDepartmentAsync(
        string department, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_SafetyPerformanceIndicator_GetByDepartment} Department: {department}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_SafetyPerformanceIndicator_GetByDepartment, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSPIResponsibleDepartment, department));

            List<SafetyPerformanceIndicator> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var spi = Mappers.MapToSafetyPerformanceIndicator(reader);
                    response.Add(spi);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<SafetyPerformanceIndicator>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }

    #endregion
}