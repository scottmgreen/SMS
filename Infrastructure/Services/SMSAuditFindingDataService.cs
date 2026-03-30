//-----------------------------------------------------------------------
// <copyright file="SMSAuditFindingDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS Audit Finding data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------



using SMS_Domain.Entities;
using SMS_Infrastructure.Persistence;

namespace SMS_Infrastructure.Services;

/// <summary>
/// SMS Audit Finding Data Service providing business operations for SMSAuditFinding entities
/// </summary>
public class SMSAuditFindingDataService : BaseDataService<SMSAuditFindingDataService>
{
    private readonly ILogger<SMSAuditFindingDataService> _logger;
    private readonly string _logheader;
    private readonly SMSAuditFindingRepository _repo;

    public SMSAuditFindingDataService(
        ILogger<SMSAuditFindingDataService> logger,
        IServiceScopeFactory serviceScopeFactory,
        IConfiguration configuration,
        SMSAuditFindingRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} SMSAuditFindingDataService");
    }

    /// <summary>
    /// Creates a new SMS Audit Finding
    /// </summary>
    public async Task<Result<SMSAuditFinding>> CreateAuditFindingAsync(SMSAuditFinding auditFinding, CancellationToken ct = default)
    {
        return await _repo.CreateSMSAuditFindingAsync(auditFinding, ct);
    }

    /// <summary>
    /// Updates an existing SMS Audit Finding
    /// </summary>
    public async Task<Result<SMSAuditFinding>> UpdateAuditFindingAsync(SMSAuditFinding auditFinding, CancellationToken ct = default)
    {
        return await _repo.UpdateSMSAuditFindingAsync(auditFinding, ct);
    }

    /// <summary>
    /// Deletes an SMS Audit Finding
    /// </summary>
    public async Task<Result<bool>> DeleteAuditFindingAsync(string findingCode, string deletedBy, string reason, CancellationToken ct = default)
    {
        // Get the finding by code first to get the ID
        var findingResult = await _repo.GetSMSAuditFindingByCodeAsync(findingCode, ct);
        if (findingResult.IsFailure)
        {
            return Result<bool>.Failure<bool>(findingResult.Error);
        }

        return await _repo.DeleteSMSAuditFindingAsync(findingResult.Value.Id.Value, ct);
    }

    /// <summary>
    /// Gets all SMS Audit Findings
    /// </summary>
    public async Task<Result<List<SMSAuditFinding>>> GetAllAuditFindingsAsync(CancellationToken ct = default)
    {
        var result = await _repo.GetAllSMSAuditFindingsAsync(ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditFinding>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditFinding>>.Failure<List<SMSAuditFinding>>(result.Error);
    }

    /// <summary>
    /// Gets an SMS Audit Finding by Code
    /// </summary>
    public async Task<Result<SMSAuditFinding>> GetAuditFindingByCodeAsync(string findingCode, CancellationToken ct = default)
    {
        return await _repo.GetSMSAuditFindingByCodeAsync(findingCode, ct);
    }

    /// <summary>
    /// Gets SMS Audit Findings by audit code
    /// </summary>
    public async Task<Result<List<SMSAuditFinding>>> GetFindingsByAuditCodeAsync(string auditCode, CancellationToken ct = default)
    {
        var result = await _repo.GetSMSAuditFindingsByAuditCodeAsync(auditCode, ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditFinding>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditFinding>>.Failure<List<SMSAuditFinding>>(result.Error);
    }

    /// <summary>
    /// Gets SMS Audit Findings by severity
    /// </summary>
    public Task<Result<List<SMSAuditFinding>>> GetAuditFindingsBySeverityAsync(string severity, string? statusFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready - requires pr_SMSAuditFinding_GetBySeverity
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditFinding_GetBySeverity");
    }

    /// <summary>
    /// Gets SMS Audit Findings by status
    /// </summary>
    public Task<Result<List<SMSAuditFinding>>> GetAuditFindingsByStatusAsync(string status, string? departmentFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready - requires pr_SMSAuditFinding_GetByStatus
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditFinding_GetByStatus");
    }

    /// <summary>
    /// Gets SMS Audit Findings by responsible person
    /// </summary>
    public Task<Result<List<SMSAuditFinding>>> GetAuditFindingsByResponsiblePersonAsync(string responsiblePerson, string? statusFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready - requires pr_SMSAuditFinding_GetByResponsiblePerson
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditFinding_GetByResponsiblePerson");
    }

    /// <summary>
    /// Gets overdue SMS Audit Findings
    /// </summary>
    public async Task<Result<List<SMSAuditFinding>>> GetOverdueFindingsAsync(CancellationToken ct = default)
    {
        var result = await _repo.GetOverdueSMSAuditFindingsAsync(ct);
        if (result.IsSuccess)
        {
            return Result<List<SMSAuditFinding>>.Success(result.Value.ToList());
        }
        return Result<List<SMSAuditFinding>>.Failure<List<SMSAuditFinding>>(result.Error);
    }

    /// <summary>
    /// Gets SMS Audit Findings requiring verification
    /// </summary>
    public Task<Result<List<SMSAuditFinding>>> GetAuditFindingsRequiringVerificationAsync(string? departmentFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready - requires pr_SMSAuditFinding_GetRequiringVerification
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditFinding_GetRequiringVerification");
    }

    /// <summary>
    /// Gets all findings - wrapper method
    /// </summary>
    public async Task<Result<List<SMSAuditFinding>>> GetAllFindingsAsync(CancellationToken ct = default)
    {
        return await GetAllAuditFindingsAsync(ct);
    }

    /// <summary>
    /// Gets finding by code - wrapper method
    /// </summary>
    public async Task<Result<SMSAuditFinding>> GetFindingByCodeAsync(string findingCode, CancellationToken ct = default)
    {
        return await GetAuditFindingByCodeAsync(findingCode, ct);
    }

    /// <summary>
    /// Creates finding - wrapper method
    /// </summary>
    public async Task<Result<SMSAuditFinding>> CreateFindingAsync(SMSAuditFinding finding, CancellationToken ct = default)
    {
        return await CreateAuditFindingAsync(finding, ct);
    }

    /// <summary>
    /// Updates finding - wrapper method
    /// </summary>
    public async Task<Result<SMSAuditFinding>> UpdateFindingAsync(SMSAuditFinding finding, CancellationToken ct = default)
    {
        return await UpdateAuditFindingAsync(finding, ct);
    }

    /// <summary>
    /// Deletes finding - wrapper method
    /// </summary>
    public async Task<Result<bool>> DeleteFindingAsync(string findingCode, CancellationToken ct = default)
    {
        return await _repo.DeleteSMSAuditFindingAsync(findingCode, ct);
    }

    /// <summary>
    /// Gets SMS Audit Finding statistics for dashboard
    /// </summary>
    public Task<Result<SMSAuditFindingStatistics>> GetAuditFindingStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null,
        string? departmentFilter = null, string? auditTypeFilter = null, CancellationToken ct = default)
    {
        // TODO: Implement when repository is ready - requires pr_SMSAuditFinding_GetStatistics
        throw new NotImplementedException("Repository implementation pending - requires pr_SMSAuditFinding_GetStatistics");
    }
}
