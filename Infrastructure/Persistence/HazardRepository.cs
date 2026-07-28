//-----------------------------------------------------------------------
// <copyright file="HazardRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository implementing data access operations for SMS hazard entities with comprehensive CRUD and business query operations.
//                  Repository implementation providing data access operations
//                  with stored procedure integration and entity mapping.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using SMS_Domain.Errors;

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Hazard operations
/// </summary>
public sealed class HazardRepository : BaseRepository<HazardRepository, Hazard>, IHazardRepository
{
    private readonly ILogger<HazardRepository> _logger;
    private readonly string _logheader;
    private readonly string _connectionString;

    public HazardRepository(ILogger<HazardRepository> logger, ILogSupport logsupport, IConfiguration configuration)
        : base(logger, logsupport, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _connectionString = ConnectionString;
        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} Hazard Repository Initialized");
    }

    #region Interface Implementation

    public async Task<Result<Hazard>> GetByIdAsync(HazardID code)
    {
        return await GetHazardByCodeAsync(code);
    }

    public async Task<Result<Hazard>> AddAsync(Hazard hazard)
    {
        return await CreateHazardAsync(hazard);
    }

    public async Task<Result<bool>> UpdateAsync(Hazard hazard)
    {
        var result = await UpdateHazardAsync(hazard);
        return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure<bool>(result.Error);
    }

    public async Task<Result<bool>> DeleteAsync(HazardID id)
    {
        return await DeleteHazardAsync(id);
    }

    public async Task<Result<IEnumerable<Hazard>>> GetAllAsync()
    {
        var result = await GetAllHazardsAsync();
        return result.IsSuccess
            ? Result<IEnumerable<Hazard>>.Success(result.Value.AsEnumerable())
            : Result<IEnumerable<Hazard>>.Failure<IEnumerable<Hazard>>(result.Error);
    }

        

    public async Task<Result<IEnumerable<Hazard>>> GetByReportCodeAsync(string reportCode)
    {
        // TODO: Implement proper filter by report code
        // For now, return all hazards
        return await GetAllAsync();
    }

    #endregion

    #region Existing Implementation Methods

    public async Task<Result<Hazard>> CreateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        try
        {
            if (hazard is null)
            {
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            // ?? DEBUG: Log the actual values before sending to stored proc
            _logger.LogInfrastructureInformation("REPO DEBUG - Hazard.CreatedBy: '{CreatedBy}', Code: '{Code}'",
                hazard.CreatedBy ?? "NULL", hazard.Code ?? "NULL");

            _logger.LogInfrastructurePostItem($"{_logheader} {StoredProcs.pr_Hazard_Insert} Code:{hazard.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Hazard_Insert, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardCode, hazard.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardName, hazard.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardIsInitialHazard, hazard.IsInitialHazard));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardTitle, hazard.HazardTitle));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardDescription, hazard.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardRiskLevel, hazard.HazardRiskLevel.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardType, hazard.HazardType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardCategory, hazard.HazardCategory));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportCode, hazard.ReportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardInitialRiskMatrixCode, hazard.InitialRiskMatrixCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardInitialAverageScore, hazard.InitialAverageScore));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardResidualRiskMatrixCode, hazard.ResidualRiskMatrixCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardResidualAverageScore, hazard.ResidualAverageScore));

            // ?? DEBUG: Log what we're about to send to the stored proc
            var createdByParam = DataAccess.Parameter(ParameterNames.pmCreatedBy, hazard.CreatedBy);
            var createdDateParam = DataAccess.Parameter(ParameterNames.pmCreatedDate, DateTime.UtcNow);

            
            cmd.Parameters.Add(createdByParam);
            cmd.Parameters.Add(createdDateParam);

            var newID = new SqlParameter("@pNewID", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var newCode = new SqlParameter("@pNewHazardCode", SqlDbType.NVarChar, 50) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(newID);
            cmd.Parameters.Add(newCode);

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            int newIdValue = (int)newID.Value;
            string newCodeValue = Convert.ToString(newCode.Value) ?? string.Empty;
            HazardID hazardId = new(newCodeValue);

            // ?? DEBUG: Log what we got back
            _logger.LogInfrastructureInformation("STORED PROC RESULT - NewID: {NewID}, NewCode: '{NewCode}'", newIdValue, newCodeValue);

            return await GetHazardByCodeAsync(hazardId, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePostItemError($"{_logheader} {ex.Message}", null);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }

    public async Task<Result<Hazard>> GetHazardByCodeAsync(HazardID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Hazard_GetByCode} {code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Hazard_GetByCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            Hazard? response = null;

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    response = Mappers.MapToHazard(reader);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            if (response is not null)
            {
                return Result<Hazard>.Success(response);
            }
            else
            {
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
            }
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }

    public async Task<Result<List<Hazard>>> GetHazardsByReportCodeAsync(ReportID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInfrastructureGetItem($"{_logheader} {StoredProcs.pr_Hazard_GetByReportCode} {code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Hazard_GetByReportCode, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, code.Value));

            List<Hazard> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var hazard = Mappers.MapToHazard(reader);
                    response.Add(hazard);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Hazard>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} {ex.Message}", null);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    /// <summary>
    /// Get hazards with their associated mitigations loaded separately
    /// This avoids the complex JOIN and ensures clean data mapping
    /// </summary>
    public async Task<Result<List<Hazard>>> GetHazardsByReportCodeWithMitigationsAsync(ReportID reportId, CancellationToken ct = default)
    {
        try
        {
            // First, get the hazards (clean, no joins)
            var hazardsResult = await GetHazardsByReportCodeAsync(reportId, ct);
            if (hazardsResult.IsFailure || hazardsResult.Value == null)
            {
                return hazardsResult;
            }

            var hazards = hazardsResult.Value;

            // Then, load mitigations separately for each hazard if needed
            // NOTE: Only load mitigations if specifically requested to avoid performance issues
            foreach (var hazard in hazards)
            {
                // You can add mitigation loading logic here if needed
                // var mitigations = await _mitigationRepository.GetByHazardCodeAsync(hazard.Code, ct);
                // hazard.LoadMitigations(mitigations);
            }

            return Result<List<Hazard>>.Success(hazards);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemError($"{_logheader} Error loading hazards with mitigations: {ex.Message}", null);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }





    public async Task<Result<List<Hazard>>> GetAllHazardsAsync(CancellationToken ct = default)
    {
        try
        {
            _logger.LogInfrastructureGetItems($"{_logheader} {StoredProcs.pr_Hazard_GetAll}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Hazard_GetAll, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            List<Hazard> response = new();

            await sql.OpenAsync(ct).ConfigureAwait(false);
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
            {
                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var hazard = Mappers.MapToHazard(reader);
                    response.Add(hazard);
                }
            }
            await sql.CloseAsync().ConfigureAwait(false);

            return Result<List<Hazard>>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructureGetItemsError($"{_logheader} {ex.Message}", null);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }

    public async Task<Result<Hazard>> UpdateHazardAsync(Hazard hazard, CancellationToken ct = default)
    {
        try
        {
            if (hazard is null)
            {
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInfrastructurePutItem($"{_logheader} {StoredProcs.pr_Hazard_Update} Code:{hazard.Code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Hazard_Update, sql)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCode, hazard.Code));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardName, hazard.Name));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardIsInitialHazard, hazard.IsInitialHazard));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardTitle, hazard.HazardTitle));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardDescription, hazard.Description));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardRiskLevel, hazard.HazardRiskLevel.Value));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardType, hazard.HazardType));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardCategory, hazard.HazardCategory));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardReportCode, hazard.ReportCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardInitialRiskMatrixCode, hazard.InitialRiskMatrixCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardInitialAverageScore, hazard.InitialAverageScore));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardResidualRiskMatrixCode, hazard.ResidualRiskMatrixCode));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmHazardResidualAverageScore, hazard.ResidualAverageScore));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedBy, hazard.UpdatedBy ?? hazard.CreatedBy ?? string.Empty));
            cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmUpdatedDate, DateTime.UtcNow));

            await sql.OpenAsync(ct).ConfigureAwait(false);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            await sql.CloseAsync().ConfigureAwait(false);

            return await GetHazardByCodeAsync((HazardID)hazard.Id, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogInfrastructurePutItemError($"{_logheader} {ex.Message}", null);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }

    public async Task<Result<bool>> DeleteHazardAsync(HazardID code, CancellationToken ct = default)
    {
        try
        {
            if (code is null)
            {
                return Result<bool>.Failure<bool>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInfrastructureDeleteItem($"{_logheader} {StoredProcs.pr_Hazard_Delete} Code:{code}", null);

            using SqlConnection sql = new(_connectionString);
            using SqlCommand cmd = new(StoredProcs.pr_Hazard_Delete, sql)
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
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }

    #endregion
}


