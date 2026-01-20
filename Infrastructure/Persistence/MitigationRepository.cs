using SMS_Domain.Entities;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;
using SMS_Infrastructure.Common;
using SMS_Infrastructure.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace SMS_Infrastructure.Persistence;

public sealed class MitigationRepository : BaseRepository<MitigationRepository, Mitigation>
{
    private readonly ILogger<MitigationRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public MitigationRepository(ILogger<MitigationRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Mitigation Repository Initialized");
    }

    public async Task<Result<Mitigation>> CreateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            if (mitigation is null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_Mitigation_Insert} Code:{mitigation.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Core Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCode, mitigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationHazardCode, mitigation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationName, mitigation.Name ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationDescription, mitigation.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationType, mitigation.Type ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationStatus, mitigation.Status ?? "Proposed"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationPriority, mitigation.Priority ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationRiskAssessmentCode, mitigation.RiskAssessmentCode ?? (object)DBNull.Value));
            
            // Timeline Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTargetDate, mitigation.TargetDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationImplementationDate, mitigation.ImplementationDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCompletionDate, mitigation.CompletionDate ?? (object)DBNull.Value));
            
            // Assignment Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignedDepartment, mitigation.AssignedDepartment ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignedTo, mitigation.AssignedTo ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationApprovedBy, mitigation.ApprovedBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationApprovedDate, mitigation.ApprovedDate ?? (object)DBNull.Value));
            
            // Progress Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationProgress, mitigation.Progress));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationProgressNotes, mitigation.ProgressNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationLastProgressUpdate, mitigation.LastProgressUpdate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationProgressUpdatedBy, mitigation.ProgressUpdatedBy ?? (object)DBNull.Value));
            
            // Cost Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEstimatedCost, mitigation.EstimatedCost ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualCost, mitigation.ActualCost ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationResourceRequirements, mitigation.ResourceRequirements ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEstimatedHours, mitigation.EstimatedHours ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualHours, mitigation.ActualHours ?? (object)DBNull.Value));
            
            // Effectiveness Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessRating, mitigation.EffectivenessRating ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessNotes, mitigation.EffectivenessNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessReviewDate, mitigation.EffectivenessReviewDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessReviewedBy, mitigation.EffectivenessReviewedBy ?? (object)DBNull.Value));
            
            // Monitoring Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationMonitoringRequirements, mitigation.MonitoringRequirements ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationMonitoringFrequency, mitigation.MonitoringFrequency ?? (object)DBNull.Value));
            
            // Risk Reduction Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationExpectedSeverityReduction, mitigation.ExpectedSeverityReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationExpectedLikelihoodReduction, mitigation.ExpectedLikelihoodReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualSeverityReduction, mitigation.ActualSeverityReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualLikelihoodReduction, mitigation.ActualLikelihoodReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationResidualRiskLevel, mitigation.ResidualRiskLevel ?? (object)DBNull.Value));
            
            // Dependency Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationPrerequisites, mitigation.Prerequisites ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationDependencies, mitigation.Dependencies ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationHasDependencies, mitigation.HasDependencies));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationIsPrerequisite, mitigation.IsPrerequisite));
            
            // Planning Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationImplementationPlan, mitigation.ImplementationPlan ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCommunicationPlan, mitigation.CommunicationPlan ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTrainingRequirements, mitigation.TrainingRequirements ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationDocumentationUpdates, mitigation.DocumentationUpdates ?? (object)DBNull.Value));
            
            // Testing Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTestingProcedure, mitigation.TestingProcedure ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTestingCompletedDate, mitigation.TestingCompletedDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTestingResults, mitigation.TestingResults ?? (object)DBNull.Value));
            
            // Validation Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationValidationRequired, mitigation.ValidationRequired));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationValidationDate, mitigation.ValidationDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationValidatedBy, mitigation.ValidatedBy ?? (object)DBNull.Value));
            
            // Additional Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationNotes, mitigation.Notes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationLessonsLearned, mitigation.LessonsLearned ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationRecommendationsForFuture, mitigation.RecommendationsForFuture ?? (object)DBNull.Value));
            
            // Audit Fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            var newID = new SqlParameter(ParameterNames.pmNewID, SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter(ParameterNames.pmNewMitigationCode, SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            MitigationID mitigationId = new (newCodeValue);

            return await GetMitigationByCodeAsync(mitigationId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.CreateFailed);
        }
    }

    public async Task<Result<Mitigation>> GetMitigationByCodeAsync(MitigationID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Mitigation_GetByCode} {code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            Mitigation? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToMitigation(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Mitigation>.Success(response);
            }
            else
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<Mitigation>>> GetAllMitigationsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Mitigation_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Mitigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var mitigation = Mappers.MapToMitigation(reader);
                    response.Add(mitigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Mitigation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }

    public async Task<Result<Mitigation>> UpdateMitigationAsync(Mitigation mitigation, CancellationToken ct = default)
    {
        try
        {
            if (mitigation is null)
            {
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Mitigation_Update} ID:{mitigation.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Primary Key
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationIdCorrected, mitigation.Id.Value));
            
            // Core Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCode, mitigation.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationHazardCode, mitigation.HazardCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationName, mitigation.Name ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationDescription, mitigation.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationType, mitigation.Type ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationStatus, mitigation.Status ?? "Proposed"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationPriority, mitigation.Priority ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationRiskAssessmentCode, mitigation.RiskAssessmentCode ?? (object)DBNull.Value));
            
            // Timeline Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTargetDate, mitigation.TargetDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationImplementationDate, mitigation.ImplementationDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCompletionDate, mitigation.CompletionDate ?? (object)DBNull.Value));
            
            // Assignment Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignedDepartment, mitigation.AssignedDepartment ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationAssignedTo, mitigation.AssignedTo ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationApprovedBy, mitigation.ApprovedBy ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationApprovedDate, mitigation.ApprovedDate ?? (object)DBNull.Value));
            
            // Progress Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationProgress, mitigation.Progress));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationProgressNotes, mitigation.ProgressNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationLastProgressUpdate, mitigation.LastProgressUpdate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationProgressUpdatedBy, mitigation.ProgressUpdatedBy ?? (object)DBNull.Value));
            
            // Cost Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEstimatedCost, mitigation.EstimatedCost ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualCost, mitigation.ActualCost ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationResourceRequirements, mitigation.ResourceRequirements ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEstimatedHours, mitigation.EstimatedHours ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualHours, mitigation.ActualHours ?? (object)DBNull.Value));
            
            // Effectiveness Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessRating, mitigation.EffectivenessRating ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessNotes, mitigation.EffectivenessNotes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessReviewDate, mitigation.EffectivenessReviewDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationEffectivenessReviewedBy, mitigation.EffectivenessReviewedBy ?? (object)DBNull.Value));
            
            // Monitoring Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationMonitoringRequirements, mitigation.MonitoringRequirements ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationMonitoringFrequency, mitigation.MonitoringFrequency ?? (object)DBNull.Value));
            
            // Risk Reduction Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationExpectedSeverityReduction, mitigation.ExpectedSeverityReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationExpectedLikelihoodReduction, mitigation.ExpectedLikelihoodReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualSeverityReduction, mitigation.ActualSeverityReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationActualLikelihoodReduction, mitigation.ActualLikelihoodReduction ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationResidualRiskLevel, mitigation.ResidualRiskLevel ?? (object)DBNull.Value));
            
            // Dependency Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationPrerequisites, mitigation.Prerequisites ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationDependencies, mitigation.Dependencies ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationHasDependencies, mitigation.HasDependencies));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationIsPrerequisite, mitigation.IsPrerequisite));
            
            // Planning Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationImplementationPlan, mitigation.ImplementationPlan ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationCommunicationPlan, mitigation.CommunicationPlan ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTrainingRequirements, mitigation.TrainingRequirements ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationDocumentationUpdates, mitigation.DocumentationUpdates ?? (object)DBNull.Value));
            
            // Testing Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTestingProcedure, mitigation.TestingProcedure ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTestingCompletedDate, mitigation.TestingCompletedDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationTestingResults, mitigation.TestingResults ?? (object)DBNull.Value));
            
            // Validation Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationValidationRequired, mitigation.ValidationRequired));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationValidationDate, mitigation.ValidationDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationValidatedBy, mitigation.ValidatedBy ?? (object)DBNull.Value));
            
            // Additional Properties
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationNotes, mitigation.Notes ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationLessonsLearned, mitigation.LessonsLearned ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmMitigationRecommendationsForFuture, mitigation.RecommendationsForFuture ?? (object)DBNull.Value));
            
            // Audit Fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, "SYSTEM"));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetMitigationByCodeAsync((MitigationID)mitigation.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteMitigationAsync(MitigationID id, CancellationToken ct = default)
    {
        try
        {
            if (id is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_Mitigation_Delete} ID:{id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_Delete, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmId,    id.Value));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureDeleteItemError($"{_logheader} {ex.Message}", null);
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.DeleteFailed);
        }
    }

    /// <summary>
    /// Gets all mitigations for a specific hazard code - Required for populating Hazard.Mitigations list
    /// </summary>
    public async Task<Result<List<Mitigation>>> GetMitigationsByHazardCodeAsync(string hazardCode, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hazardCode))
            {
                return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Mitigation_GetByHazardCode} HazardCode:{hazardCode}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Mitigation_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardCode, hazardCode));

            List<Mitigation> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var mitigation = Mappers.MapToMitigation(reader);
                    response.Add(mitigation);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Mitigation>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }
}