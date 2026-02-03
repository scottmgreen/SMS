using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

public sealed class RiskAssessmentRepository : BaseRepository<RiskAssessmentRepository, RiskAssessment>, IRiskAssessmentRepository
{
    private readonly ILogger<RiskAssessmentRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public RiskAssessmentRepository(ILogger<RiskAssessmentRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} RiskAssessment Repository Initialized");
    }

    public async Task<Result<RiskAssessment>> CreateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            if (riskAssessment is null)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_RiskAssessment_Insert} Code:{riskAssessment.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // ✅ CORRECTED: Updated parameters to match the fixed stored procedure exactly
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessment.Code ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentName, riskAssessment.Name ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentDescription, riskAssessment.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentHazardCode, riskAssessment.HazardCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentType, riskAssessment.AssessmentType.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, riskAssessment.Status.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStage, riskAssessment.Stage ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemDescription, riskAssessment.SystemDescription ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemBoundaries, riskAssessment.SystemBoundaries ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemPurpose, riskAssessment.SystemPurpose ?? (object)DBNull.Value));

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonnelFactors, riskAssessment.FiveMPersonnel ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmProcedureFactors, riskAssessment.FiveMProcedures ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentFactors, riskAssessment.FiveMEquipment ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmResourceFactors, riskAssessment.FiveMResources ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEnvironmentFactors, riskAssessment.FiveMPhysicalEnvironment ?? (object)DBNull.Value));

            // Core fields with proper null handling
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLeadAssessorId, riskAssessment.LeadAssessorId ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCurrentStep, riskAssessment.CurrentStep));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCategory, riskAssessment.RiskAssessmentCategory.ToString()));

            // Audit fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedBy, riskAssessment.CreatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow));

            // Output parameters
            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewRiskAssessmentCode", SqlDbType.NVarChar, 60) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            RiskAssessmentID riskAssessmentId = new RiskAssessmentID(newCodeValue);

            return await GetRiskAssessmentByCodeAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create RiskAssessment: {Code}", riskAssessment?.Code ?? "NULL");
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.CreateFailed);
        }
    }

    public async Task<Result<RiskAssessment>> GetRiskAssessmentByCodeAsync(RiskAssessmentID riskAssessmentCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving RiskAssessment by Code: {Code}", riskAssessmentCode);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, riskAssessmentCode.Value));

            RiskAssessment? riskAssessment = null;

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    riskAssessment = Mappers.MapToRiskAssessment(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (riskAssessment == null)
            {
                _logger.LogWarning("RiskAssessment not found with Code: {Code}", riskAssessmentCode);
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
            }

            _logger.LogInformation("Successfully retrieved RiskAssessment: {Code}", riskAssessmentCode);
            return Result<RiskAssessment>.Success(riskAssessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessment by ID: {Code}", riskAssessmentCode);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }

    public async Task<Result<List<RiskAssessment>>> GetRiskAssessmentsByHazardCodeAsync(HazardID hazardCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving RiskAssessment by Hazard Code: {Code}", hazardCode);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_GetByHazardCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentHazardCode, hazardCode.Value));

            List<RiskAssessment> response = new();

            await sql.OpenAsync(cancellationToken).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var riskAssessment = Mappers.MapToRiskAssessment(reader);
                    response.Add(riskAssessment);
                }


            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<RiskAssessment>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve RiskAssessments by Code: {Code}", hazardCode);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }




    public async Task<Result<List<RiskAssessment>>> GetAllRiskAssessmentsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_RiskAssessment_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<RiskAssessment> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var riskAssessment = Mappers.MapToRiskAssessment(reader);
                    response.Add(riskAssessment);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<RiskAssessment>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NullOrEmpty);
        }
    }

    public async Task<Result<RiskAssessment>> UpdateRiskAssessmentAsync(RiskAssessment riskAssessment, CancellationToken ct = default)
    {
        try
        {
            if (riskAssessment is null)
            {
                return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_Update} ID:{riskAssessment.Id}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Core parameters (required)
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessment.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentName, riskAssessment.Name ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentDescription, riskAssessment.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentHazardCode, riskAssessment.HazardCode ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentType, riskAssessment.AssessmentType.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, riskAssessment.Status.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStage, riskAssessment.Stage ?? (object)DBNull.Value));

            // Enhanced core fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLeadAssessorId, riskAssessment.LeadAssessorId ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPrimaryHazardId, riskAssessment.PrimaryHazardId ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCategory, riskAssessment.RiskAssessmentCategory.ToString()));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCurrentStep, riskAssessment.CurrentStep));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCompletedDate, riskAssessment.CompletedDate ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCompletedBy, riskAssessment.CompletedBy ?? (object)DBNull.Value));
            
            // Step 1 - System Description Fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemDescription, riskAssessment.SystemDescription ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemBoundaries, riskAssessment.SystemBoundaries ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemPurpose, riskAssessment.SystemPurpose ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonnelFactors, riskAssessment.FiveMPersonnel ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentFactors, riskAssessment.FiveMEquipment ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmProcedureFactors, riskAssessment.FiveMProcedures ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmResourceFactors, riskAssessment.FiveMResources ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEnvironmentFactors, riskAssessment.FiveMPhysicalEnvironment ?? (object)DBNull.Value));

            // Step 3 - Risk Analysis Fields
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisMethod, riskAssessment.RiskAnalysisMethod ?? (object)DBNull.Value));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskCriteria, riskAssessment.RiskCriteria ?? (object)DBNull.Value));

            // Step 4 - Risk Assessment Fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFinalSeverityScore, riskAssessment.FinalSeverityScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFinalLikelihoodScore, riskAssessment.FinalLikelihoodScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFinalRiskLevel, riskAssessment.FinalRiskLevel ?? (object)DBNull.Value));
            

            // Step 5 - Implementation Fields
            

            // Progress Tracking Fields
            var completedStepsString = string.Join(",", riskAssessment.CompletedSteps);
            var completionPercentage = riskAssessment.CompletedSteps.Count * 20; // 5 steps = 100%

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCompletedSteps, completedStepsString));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCompletionPercentage, completionPercentage));

            // Audit fields
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, riskAssessment.UpdatedBy));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByCodeAsync(new RiskAssessmentID(riskAssessment.Id.Value), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates Step 1 - System Description data specifically
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateStep1Async(
        RiskAssessmentID riskAssessmentId,
        string leadAssessorId,
        string systemDescription,
        string systemBoundaries,
        string systemPurpose,
        string fiveMPersonnel,
        string fiveMEquipment,
        string fiveMProcedures,
        string fiveMResources,
        string fiveMPhysicalEnvironment,
        string fiveMOperationalEnvironment,
        string updatedBy ,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_UpdateStep1} ID:{riskAssessmentId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_UpdateStep1, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessmentId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLeadAssessorId, leadAssessorId));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemDescription, systemDescription));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemBoundaries, systemBoundaries));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemPurpose, systemPurpose));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonnelFactors, fiveMPersonnel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEquipmentFactors, fiveMEquipment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmProcedureFactors, fiveMProcedures));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmResourceFactors, fiveMResources));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmEnvironmentFactors, fiveMPhysicalEnvironment ?? fiveMOperationalEnvironment));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, updatedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByCodeAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Step 1 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates Step 3 - Risk Analysis data specifically
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateStep3Async(
        RiskAssessmentID riskAssessmentId,
        string riskAnalysisMethod,
        string riskCriteria,
        string updatedBy ,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_UpdateStep3} ID:{riskAssessmentId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_UpdateStep3, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessmentId));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisMethod, riskAnalysisMethod));
            //cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskCriteria, riskCriteria));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, updatedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByCodeAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Step 3 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates Step 4 - Risk Assessment data specifically
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateStep4Async(
        RiskAssessmentID riskAssessmentId,
        int? finalSeverityScore,
        int? finalLikelihoodScore,
        string finalRiskLevel,
        string updatedBy ,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_UpdateStep4} ID:{riskAssessmentId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_UpdateStep4, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessmentId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFinalSeverityScore, finalSeverityScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFinalLikelihoodScore, finalLikelihoodScore ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmFinalRiskLevel, finalRiskLevel));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, updatedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByCodeAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Step 4 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates Step 5 - Implementation data specifically
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateStep5Async(
        RiskAssessmentID riskAssessmentId,
        string updatedBy ,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_UpdateStep5} ID:{riskAssessmentId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_UpdateStep5, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessmentId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, updatedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByCodeAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update Step 5 for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    /// <summary>
    /// Updates progress tracking data specifically
    /// </summary>
    public async Task<Result<RiskAssessment>> UpdateProgressAsync(
        RiskAssessmentID riskAssessmentId,
        int currentStep,
        string completedSteps,
        int completionPercentage,
        string status = null,
        string stage = null,
        string updatedBy = "SYSTEM",
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_RiskAssessment_UpdateProgress} ID:{riskAssessmentId}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_UpdateProgress, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessmentId.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCurrentStep, currentStep));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCompletedSteps, completedSteps));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCompletionPercentage, completionPercentage));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, status ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStage, stage ?? (object)DBNull.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, updatedBy));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetRiskAssessmentByCodeAsync(riskAssessmentId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update progress for RiskAssessment: {Id}", riskAssessmentId);
            return Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteRiskAssessmentAsync(RiskAssessmentID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_RiskAssessment_Delete} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_RiskAssessment_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.RiskAssessmentError.DeleteFailed);
        }
    }

    #region Interface Implementation

    public async Task<Result<RiskAssessment>> GetByCodeAsync(RiskAssessmentID id)
    {
        return await GetRiskAssessmentByCodeAsync(id);
    }

    public async Task<Result<RiskAssessment>> AddAsync(RiskAssessment riskAssessment)
    {
        return await CreateRiskAssessmentAsync(riskAssessment);
    }

    public async Task<Result<bool>> UpdateAsync(RiskAssessment riskAssessment)
    {
        var result = await UpdateRiskAssessmentAsync(riskAssessment);
        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure<bool>(result.Error);
    }

    public async Task<Result<bool>> DeleteAsync(RiskAssessmentID id)
    {
        return await DeleteRiskAssessmentAsync(id);
    }

    public async Task<Result<IEnumerable<RiskAssessment>>> GetAllAsync()
    {
        var result = await GetAllRiskAssessmentsAsync();
        return result.IsSuccess
            ? Result<IEnumerable<RiskAssessment>>.Success(result.Value.AsEnumerable())
            : Result<IEnumerable<RiskAssessment>>.Failure<IEnumerable<RiskAssessment>>(result.Error);
    }

    public async Task<Result<IEnumerable<RiskAssessment>>> GetByLeadAssessorAsync(string leadAssessorId)
    {
        // TODO: Implement proper filter by lead assessor
        // For now, return all assessments
        return await GetAllAsync();
    }

    public async Task<Result<IEnumerable<RiskAssessment>>> GetByStatusAsync(RiskAssessmentStatus status)
    {
        // TODO: Implement proper filter by status
        // For now, return all assessments
        return await GetAllAsync();
    }

    public async Task<Result<IEnumerable<RiskAssessment>>> GetByHazardIdAsync(string hazardId)
    {
        // TODO: Implement proper filter by hazard ID
        // For now, return all assessments
        return await GetAllAsync();
    }

    public async Task<Result<IEnumerable<RiskAssessment>>> GetActiveAssessmentsAsync()
    {
        // TODO: Implement proper filter for active assessments
        // For now, return all assessments
        return await GetAllAsync();
    }

    public async Task<Result<IEnumerable<RiskAssessment>>> GetResidualAssessmentsAsync(string parentAssessmentId)
    {
        // TODO: Implement proper filter for residual assessments
        // For now, return all assessments
        return await GetAllAsync();
    }

    #endregion
}