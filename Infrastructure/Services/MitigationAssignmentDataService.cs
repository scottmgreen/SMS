//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Mitigation assignment data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Mitigation Assignment Data Service providing business operations for MitigationAssignment entities
/// </summary>
public class MitigationAssignmentDataService : BaseDataService<MitigationAssignmentDataService>
{
    private readonly ILogger<MitigationAssignmentDataService> _logger;
    private readonly string _logheader;
    private readonly MitigationAssignmentRepository _repo;

    public MitigationAssignmentDataService(ILogger<MitigationAssignmentDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, MitigationAssignmentRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<MitigationAssignment>> CreateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        return _repo.CreateMitigationAssignmentAsync(mitigationAssignment, ct);
    }

    public Task<Result<MitigationAssignment>> GetMitigationAssignmentByIdAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        return _repo.GetMitigationAssignmentByIdAsync(id, ct);
    }

    public Task<Result<List<MitigationAssignment>>> GetAllMitigationAssignmentsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllMitigationAssignmentsAsync(ct);
    }

    public Task<Result<MitigationAssignment>> UpdateMitigationAssignmentAsync(MitigationAssignment mitigationAssignment, CancellationToken ct = default)
    {
        return _repo.UpdateMitigationAssignmentAsync(mitigationAssignment, ct);
    }

    public Task<Result<bool>> DeleteMitigationAssignmentAsync(MitigationAssignmentID id, CancellationToken ct = default)
    {
        return _repo.DeleteMitigationAssignmentAsync(id, ct);
    }
}

