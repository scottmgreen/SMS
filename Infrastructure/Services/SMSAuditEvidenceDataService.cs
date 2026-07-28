//-----------------------------------------------------------------------
// <copyright file="SMSAuditEvidenceDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Audit Evidence data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------



using SMS_Domain.Entities;
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

/// <summary>
/// SMS Audit Evidence Data Service providing business operations for SMSAuditEvidence entities
/// </summary>
public class SMSAuditEvidenceDataService : BaseDataService<SMSAuditEvidenceDataService>
{
    private readonly ILogger<SMSAuditEvidenceDataService> _logger;
    private readonly string _logheader;
    private readonly SMSAuditEvidenceRepository _repo;

    public SMSAuditEvidenceDataService(
        ILogger<SMSAuditEvidenceDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSAuditEvidenceRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} SMSAuditEvidenceDataService");
    }

    /// <summary>
    /// Creates a new SMS Audit Evidence
    /// </summary>
    public async Task<Result<SMSAuditEvidence>> CreateAuditEvidenceAsync(SMSAuditEvidence auditEvidence, CancellationToken ct = default)
    {
        return await _repo.CreateSMSAuditEvidenceAsync(auditEvidence, ct);
    }

    /// <summary>
    /// Updates an existing SMS Audit Evidence
    /// </summary>
    public async Task<Result<SMSAuditEvidence>> UpdateAuditEvidenceAsync(SMSAuditEvidence auditEvidence, CancellationToken ct = default)
    {
        return await _repo.UpdateSMSAuditEvidenceAsync(auditEvidence, ct);
    }

    /// <summary>
    /// Deletes an SMS Audit Evidence
    /// </summary>
    public async Task<Result<bool>> DeleteAuditEvidenceAsync(string evidenceCode, string deletedBy, string reason, CancellationToken ct = default)
    {
        // Get the evidence by code first to get the ID
        var evidenceResult = await _repo.GetSMSAuditEvidenceByCodeAsync(evidenceCode, ct);
        if (evidenceResult.IsFailure)
        {
            return Result<bool>.Failure<bool>(evidenceResult.Error);
        }

        return await _repo.DeleteSMSAuditEvidenceAsync(evidenceCode, ct);
    }

    /// <summary>
    /// Gets all SMS Audit Evidence
    /// </summary>
    public async Task<Result<List<SMSAuditEvidence>>> GetAllAuditEvidenceAsync(CancellationToken ct = default)
    {
        var result = await _repo.GetAllSMSAuditEvidenceAsync(ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditEvidence>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditEvidence>>.Failure<List<SMSAuditEvidence>>(result.Error);
    }

    /// <summary>
    /// Gets an SMS Audit Evidence by Code
    /// </summary>
    public async Task<Result<SMSAuditEvidence>> GetAuditEvidenceByCodeAsync(string evidenceCode, CancellationToken ct = default)
    {
        return await _repo.GetSMSAuditEvidenceByCodeAsync(evidenceCode, ct);
    }

    /// <summary>
    /// Gets SMS Audit Evidence by audit code
    /// </summary>
    public async Task<Result<List<SMSAuditEvidence>>> GetEvidenceByAuditCodeAsync(string auditCode, bool includeArchived = false, CancellationToken ct = default)
    {
        var result = await _repo.GetSMSAuditEvidenceByAuditCodeAsync(auditCode, includeArchived, ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditEvidence>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditEvidence>>.Failure<List<SMSAuditEvidence>>(result.Error);
    }

    /// <summary>
    /// Gets SMS Audit Evidence by finding code
    /// </summary>
    public async Task<Result<List<SMSAuditEvidence>>> GetEvidenceByFindingCodeAsync(string findingCode, bool includeArchived = false, CancellationToken ct = default)
    {
        var result = await _repo.GetSMSAuditEvidenceByFindingCodeAsync(findingCode, includeArchived, ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditEvidence>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditEvidence>>.Failure<List<SMSAuditEvidence>>(result.Error);
    }

    /// <summary>
    /// Archives SMS Audit Evidence
    /// </summary>
    public async Task<Result<bool>> ArchiveEvidenceAsync(string evidenceCode, string archivedBy = "", CancellationToken ct = default)
    {
        return await _repo.ArchiveSMSAuditEvidenceAsync(evidenceCode, archivedBy, ct);
    }

    /// <summary>
    /// Gets all evidence - wrapper for repository method
    /// </summary>
    public async Task<Result<List<SMSAuditEvidence>>> GetAllEvidenceAsync(CancellationToken ct = default)
    {
        return await GetAllAuditEvidenceAsync(ct);
    }

    /// <summary>
    /// Gets evidence by code - wrapper for repository method
    /// </summary>
    public async Task<Result<SMSAuditEvidence>> GetEvidenceByCodeAsync(string evidenceCode, CancellationToken ct = default)
    {
        return await GetAuditEvidenceByCodeAsync(evidenceCode, ct);
    }

    /// <summary>
    /// Creates evidence - wrapper for repository method
    /// </summary>
    public async Task<Result<SMSAuditEvidence>> CreateEvidenceAsync(SMSAuditEvidence evidence, CancellationToken ct = default)
    {
        return await CreateAuditEvidenceAsync(evidence, ct);
    }

    /// <summary>
    /// Updates evidence - wrapper for repository method
    /// </summary>
    public async Task<Result<SMSAuditEvidence>> UpdateEvidenceAsync(SMSAuditEvidence evidence, CancellationToken ct = default)
    {
        return await UpdateAuditEvidenceAsync(evidence, ct);
    }

    /// <summary>
    /// Deletes evidence - wrapper for repository method
    /// </summary>
    public async Task<Result<bool>> DeleteEvidenceAsync(string evidenceCode, CancellationToken ct = default)
    {
        return await _repo.DeleteSMSAuditEvidenceAsync(evidenceCode, ct);
    }

    // TODO: Implement when additional stored procedures are available
    /// <summary>
    /// Gets SMS Audit Evidence by type
    /// </summary>
    public Task<Result<List<SMSAuditEvidence>>> GetAuditEvidenceByTypeAsync(string evidenceType, string? confidentialityLevel = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditEvidence_GetByType");
    }

    /// <summary>
    /// Gets SMS Audit Evidence by collector
    /// </summary>
    public Task<Result<List<SMSAuditEvidence>>> GetAuditEvidenceByCollectorAsync(string collectedBy, DateTime? collectionDateFrom = null,
        DateTime? collectionDateTo = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditEvidence_GetByCollector");
    }

    /// <summary>
    /// Gets unverified SMS Audit Evidence
    /// </summary>
    public Task<Result<List<SMSAuditEvidence>>> GetUnverifiedAuditEvidenceAsync(string? auditCodeFilter = null, string? evidenceTypeFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditEvidence_GetUnverified");
    }

    /// <summary>
    /// Gets archived SMS Audit Evidence
    /// </summary>
    public Task<Result<List<SMSAuditEvidence>>> GetArchivedAuditEvidenceAsync(DateTime? archivedAfter = null, string? retentionReasonFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditEvidence_GetArchived");
    }

    /// <summary>
    /// Gets SMS Audit Evidence statistics for dashboard
    /// </summary>
    public Task<Result<SMSAuditEvidenceStatistics>> GetAuditEvidenceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null,
        string? auditCodeFilter = null, string? evidenceTypeFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditEvidence_GetStatistics");
    }

    /// <summary>
    /// Uploads audit evidence file
    /// </summary>
    public Task<Result<SMSAuditEvidence>> UploadEvidenceFileAsync(SMSAuditEvidence auditEvidence, byte[] fileData, CancellationToken ct = default)
    {
        // TODO: Implement when file storage service is ready
        throw new NotImplementedException("File upload implementation pending - requires file storage service");
    }

    /// <summary>
    /// Downloads audit evidence file
    /// </summary>
    public Task<Result<byte[]>> DownloadEvidenceFileAsync(string evidenceCode, CancellationToken ct = default)
    {
        // TODO: Implement when file storage service is ready
        throw new NotImplementedException("File download implementation pending - requires file storage service");
    }
}
