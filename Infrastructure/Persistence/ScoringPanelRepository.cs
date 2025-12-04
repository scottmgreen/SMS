using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

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
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelSMSUserCode, scoringPanel.SMSUserCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelLikelihood, scoringPanel.Likelihood));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelSeverity, scoringPanel.Severity));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelScore, scoringPanel.Score));
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
            ScoringPanelID scoringPanelId = new (newCodeValue);

            return await GetScoringPanelByIdAsync(scoringPanelId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.CreateFailed);
        }
    }

    public async Task<Result<ScoringPanel>> GetScoringPanelByIdAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_ScoringPanel_GetById} {id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_GetById, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId, id.Value));

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
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelSMSUserCode, scoringPanel.SMSUserCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelLikelihood, scoringPanel.Likelihood));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelSeverity, scoringPanel.Severity));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmScoringPanelScore, scoringPanel.Score));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, scoringPanel.UpdatedBy ?? "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetScoringPanelByIdAsync((ScoringPanelID)scoringPanel.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteScoringPanelAsync(ScoringPanelID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_ScoringPanel_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_ScoringPanel_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.ScoringPanelError.DeleteFailed);
        }
    }
}