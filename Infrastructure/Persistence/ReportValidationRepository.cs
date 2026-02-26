//-----------------------------------------------------------------------
// <copyright file="ReportValidationRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS reportvalidation entities with reporting and validation workflows.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class ReportValidationRepository : BaseRepository<ReportValidationRepository, ReportValidation>
{
    private readonly ILogger<ReportValidationRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public ReportValidationRepository(ILogger<ReportValidationRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} ReportValidation Repository Initialized");
    }

    public async Task<Result<ReportValidation>> CreateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        try
        {
            if (reportValidation is null)
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_ReportValidation_Insert} Code:{reportValidation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ReportValidation_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add ALL parameters to match complete field set (except fldi_ID which is auto-generated)
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationCode, reportValidation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationReportCode, reportValidation.ReportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationDecision, reportValidation.ValidationDecision));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationStatus, reportValidation.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationStage, reportValidation.Stage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationType, reportValidation.ValidationType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationComments, reportValidation.ValidationComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationValidatedBy, reportValidation.ValidatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationValidatedDate, reportValidation.ValidatedDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, reportValidation.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, reportValidation.CreatedDate));

            // Output parameters
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewReportValidationCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            ReportValidationID reportValidationId = new(newCodeValue);

            return await GetReportValidationByIdAsync(reportValidationId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.CreateFailed);
        }
    }

    public async Task<Result<ReportValidation>> GetReportValidationByIdAsync(ReportValidationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_ReportValidation_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ReportValidation_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            ReportValidation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToReportValidation(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<ReportValidation>.Success(response);
            }
            else
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<ReportValidation>> GetReportValidationByReportIdAsync(ReportID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_ReportValidation_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ReportValidation_GetByReportId, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            ReportValidation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToReportValidation(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<ReportValidation>.Success(response);
            }
            else
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }





    public async Task<Result<List<ReportValidation>>> GetAllReportValidationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_ReportValidation_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ReportValidation_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<ReportValidation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var reportValidation = Mappers.MapToReportValidation(reader);
                    response.Add(reportValidation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<ReportValidation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }

    public async Task<Result<ReportValidation>> UpdateReportValidationAsync(ReportValidation reportValidation, CancellationToken ct = default)
    {
        try
        {
            if (reportValidation is null)
            {
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_ReportValidation_Update} Code:{reportValidation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ReportValidation_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add ALL parameters to match complete field set (except fldi_ID which is never updated)
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationCode, reportValidation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationReportCode, reportValidation.ReportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationDecision, reportValidation.ValidationDecision));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationStatus, reportValidation.Status.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationStage, reportValidation.Stage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationType, reportValidation.ValidationType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationComments, reportValidation.ValidationComments));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationValidatedBy, reportValidation.ValidatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmReportValidationValidatedDate, reportValidation.ValidatedDate));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, reportValidation.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, reportValidation.UpdatedDate ?? DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            var rowsAffected = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            if (rowsAffected > 0)
            {
                return await GetReportValidationByIdAsync((ReportValidationID)reportValidation.Id, ct).ConfigureAwait(false);
            }

            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteReportValidationAsync(ReportValidationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_ReportValidation_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ReportValidation_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}
