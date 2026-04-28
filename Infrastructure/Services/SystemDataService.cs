//-----------------------------------------------------------------------
// <copyright file="SystemDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: System data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Infrastructure.Services;

/// <summary>
/// System Data Service providing business operations for System entities
/// </summary>
public class SystemDataService : BaseDataService<SystemDataService>
{
    private readonly ILogger<SystemDataService> _logger;
    private readonly string _logheader;
    private SMSSystemRepository _repo;

    public SystemDataService(ILogger<SystemDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, SMSSystemRepository repo) : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader}  {repo.GetType().Name} ");
        _repo = repo;
    }


    public Task<Result<bool>> AddAuditLogEntryAsync(AuditLogEntry auditlogentry, CancellationToken ct = default)
    {
        return _repo.AddAuditLogEntryAsync(auditlogentry, ct);
    }

    public Task<Result<int>> GetAdminPasscodeAsync()
    {
        return _repo.GetAdminPasscodeAsync();
    }

    public Task<Result<bool>> GetFeatureEnabledAsync(string featurename)
    {
        return _repo.GetFeatureEnabledAsync(featurename);
    }
}

