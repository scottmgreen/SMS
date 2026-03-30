//-----------------------------------------------------------------------
// <copyright file="ScoringPanelRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS scoringpanel entities supporting risk evaluation panel management.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Scoring Panel operations
/// </summary>
public sealed class ScoringPanelRepository : BaseRepository<ScoringPanelRepository, ScoringPanel>
{
    private readonly ILogger<ScoringPanelRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public ScoringPanelRepository(ILogger<ScoringPanelRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} ScoringPanel Repository Initialized");
    }

    public async Task<Result<ScoringPanel>> CreateScoringPanelAsync(ScoringPanel scoringPanel, CancellationToken ct = default)
    {
        try
        {
            if (scoringPanel is null)
            {
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_ScoringPanel_Insert} Code:{scoringPanel.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelCode, scoringPanel.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelHazardCode, scoringPanel.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelRiskAssessmentCode, scoringPanel.RiskAssessmentCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelSMSUserCode, scoringPanel.SMSUserCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialLikelihood, scoringPanel.InitialLikelihood ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialSeverity, scoringPanel.InitialSeverity ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialScore, scoringPanel.InitialScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualLikelihood, scoringPanel.ResidualLikelihood ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualSeverity, scoringPanel.ResidualSeverity ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualScore, scoringPanel.ResidualScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, scoringPanel.CreatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewScoringPanelCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            ScoringPanelID scoringPanelId = new(newCodeValue);

            return await GetScoringPanelByCodeAsync(scoringPanelId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.CreateFailed);
        }
    }

    public async Task<Result<ScoringPanel>> GetScoringPanelByCodeAsync(ScoringPanelID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_ScoringPanel_GetByCode} {code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            ScoringPanel? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToScoringPanel(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<ScoringPanel>.Success(response);
            }
            else
            {
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<ScoringPanel>>> GetAllScoringPanelsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_ScoringPanel_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<ScoringPanel> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var scoringPanel = Mappers.MapToScoringPanel(reader);
                    response.Add(scoringPanel);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<ScoringPanel>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }

    public async Task<Result<List<ScoringPanel>>> GetScoringPanelsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrEmpty(hazardCode))
            {
                return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_ScoringPanel_GetByHazardCode} HazardCode:{hazardCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelHazardCode, hazardCode));

            List<ScoringPanel> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var scoringPanel = Mappers.MapToScoringPanel(reader);
                    response.Add(scoringPanel);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<ScoringPanel>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }

    public async Task<Result<ScoringPanel>> UpdateScoringPanelAsync(ScoringPanel scoringPanel, CancellationToken ct = default)
    {
        try
        {
            if (scoringPanel is null)
            {
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_ScoringPanel_Update} ID:{scoringPanel.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, scoringPanel.Id.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelCode, scoringPanel.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelHazardCode, scoringPanel.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelRiskAssessmentCode, scoringPanel.RiskAssessmentCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelSMSUserCode, scoringPanel.SMSUserCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialLikelihood, scoringPanel.InitialLikelihood ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialSeverity, scoringPanel.InitialSeverity ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialScore, scoringPanel.InitialScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelInitialRationale, scoringPanel.InitialRationale));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualLikelihood, scoringPanel.ResidualLikelihood ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualSeverity, scoringPanel.ResidualSeverity ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualScore, scoringPanel.ResidualScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelResidualRationale, scoringPanel.ResidualRationale));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, scoringPanel.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetScoringPanelByCodeAsync((ScoringPanelID)scoringPanel.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteScoringPanelAsync(ScoringPanelID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_ScoringPanel_Delete} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.DeleteFailed);
        }
    }
}
