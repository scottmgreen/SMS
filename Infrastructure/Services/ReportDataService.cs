//-----------------------------------------------------------------------
// <copyright file="ReportDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Report data service providing business operations for SMS domain entities.
//                  Infrastructure layer service implementing data access patterns
//                  through repositories while maintaining clean architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

/// <summary>
/// Report Data Service providing business operations for Report entities
/// </summary>
public class ReportDataService : BaseDataService<ReportDataService>, IReportDataService
{
    private readonly ILogger<ReportDataService> _logger;
    private readonly string _logheader;
    private readonly ReportRepository _repo;

    public ReportDataService(ILogger<ReportDataService> logger, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ReportRepository repo)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repo = repo;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repo.GetType().Name}");
    }

    public Task<Result<Report>> CreateReportAsync(Report report, CancellationToken ct = default)
    {
        return _repo.CreateReportAsync(report, ct);
    }

    public Task<Result<Report>> GetReportByCodeAsync(ReportID code, CancellationToken ct = default)
    {
        return _repo.GetReportByCodeAsync(code, ct);
    }

    public Task<Result<string>> GetTrackingIDByReportCodeAsync(ReportID code, CancellationToken ct = default)
    {
        return _repo.GetTrackingIDByReportCodeAsync(code, ct);
    }

    public Task<Result<List<Report>>> GetAllReportsAsync(CancellationToken ct = default)
    {
        return _repo.GetAllReportsAsync(ct);
    }

    public Task<Result<Report>> UpdateReportAsync(Report report, CancellationToken ct = default)
    {
        return _repo.UpdateReportAsync(report, ct);
    }

    public Task<Result<bool>> DeleteReportAsync(ReportID id, CancellationToken ct = default)
    {
        return _repo.DeleteReportAsync(id, ct);
    }
}

