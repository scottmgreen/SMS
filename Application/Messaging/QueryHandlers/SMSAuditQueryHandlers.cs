using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Infrastructure.Services;
using SMS_Shared.Common;
using SMS_Application.Common;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS AUDIT QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting all SMS Audits
/// </summary>
public class GetAllSMSAuditsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSAuditsQuery, Result<List<SMSAudit>>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetAllSMSAuditsQueryHandler> _logger;

    public GetAllSMSAuditsQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetAllSMSAuditsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAudit>>> HandleAsync(GetAllSMSAuditsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSAuditsQuery");

            var result = await _dataService.GetAllAuditsAsync(ct);

            if (result.IsSuccess)
            {
                var audits = result.Value;

                // Apply filters if specified
                if (!string.IsNullOrEmpty(request.StatusFilter))
                {
                    audits = audits.Where(a => a.Status.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.AuditTypeFilter))
                {
                    audits = audits.Where(a => a.AuditType.Equals(request.AuditTypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.DepartmentFilter))
                {
                    audits = audits.Where(a => a.ResponsibleDepartment.Equals(request.DepartmentFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.AuditorFilter))
                {
                    audits = audits.Where(a => a.LeadAuditor.Equals(request.AuditorFilter, StringComparison.OrdinalIgnoreCase) ||
                                             a.AuditorTeam.Contains(request.AuditorFilter)).ToList();
                }

                if (request.StartDateFrom.HasValue)
                {
                    audits = audits.Where(a => a.ScheduledStartDate >= request.StartDateFrom.Value).ToList();
                }

                if (request.StartDateTo.HasValue)
                {
                    audits = audits.Where(a => a.ScheduledStartDate <= request.StartDateTo.Value).ToList();
                }

                return Result<List<SMSAudit>>.Success(audits);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllSMSAuditsQuery");
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("QUERY_FAILED", "Failed to get audits"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit by Code
/// </summary>
public class GetSMSAuditByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditByCodeQuery, Result<SMSAudit>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetSMSAuditByCodeQueryHandler> _logger;

    public GetSMSAuditByCodeQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetSMSAuditByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAudit>> HandleAsync(GetSMSAuditByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditByCodeQuery for Code: {AuditCode}", request.AuditCode);

            var result = await _dataService.GetAuditByCodeAsync(request.AuditCode, request.IncludeFindings, request.IncludeEvidence, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSAuditByCodeQuery for Code: {AuditCode}", request.AuditCode);
            return Result<SMSAudit>.Failure<SMSAudit>(new Error("QUERY_FAILED", "Failed to get audit"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audits by Plan
/// </summary>
public class GetSMSAuditsByPlanQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditsByPlanQuery, Result<List<SMSAudit>>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetSMSAuditsByPlanQueryHandler> _logger;

    public GetSMSAuditsByPlanQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetSMSAuditsByPlanQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAudit>>> HandleAsync(GetSMSAuditsByPlanQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditsByPlanQuery for Plan: {AuditPlanCode}", request.AuditPlanCode);

            var result = await _dataService.GetAuditsByPlanAsync(request.AuditPlanCode, request.StatusFilter, request.IncludeFindings, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSAuditsByPlanQuery for Plan: {AuditPlanCode}", request.AuditPlanCode);
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("QUERY_FAILED", "Failed to get audits by plan"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audits by Status
/// </summary>
public class GetSMSAuditsByStatusQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditsByStatusQuery, Result<List<SMSAudit>>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetSMSAuditsByStatusQueryHandler> _logger;

    public GetSMSAuditsByStatusQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetSMSAuditsByStatusQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAudit>>> HandleAsync(GetSMSAuditsByStatusQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditsByStatusQuery for Status: {Status}", request.Status);

            var result = await _dataService.GetAuditsByStatusAsync(request.Status, request.DepartmentFilter, request.IncludeFindings, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSAuditsByStatusQuery for Status: {Status}", request.Status);
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("QUERY_FAILED", "Failed to get audits by status"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audits by Auditor
/// </summary>
public class GetSMSAuditsByAuditorQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditsByAuditorQuery, Result<List<SMSAudit>>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetSMSAuditsByAuditorQueryHandler> _logger;

    public GetSMSAuditsByAuditorQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetSMSAuditsByAuditorQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAudit>>> HandleAsync(GetSMSAuditsByAuditorQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditsByAuditorQuery for Auditor: {Auditor}", request.Auditor);

            var result = await _dataService.GetAuditsByAuditorAsync(
                request.Auditor, 
                request.StatusFilter, 
                request.StartDateFrom, 
                request.StartDateTo, 
                ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSAuditsByAuditorQuery for Auditor: {Auditor}", request.Auditor);
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("QUERY_FAILED", "Failed to get audits by auditor"));
        }
    }
}

/// <summary>
/// Query handler for getting Overdue SMS Audits
/// </summary>
public class GetOverdueSMSAuditsQueryHandler : BaseQueryBundle, IRequestHandler<GetOverdueSMSAuditsQuery, Result<List<SMSAudit>>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetOverdueSMSAuditsQueryHandler> _logger;

    public GetOverdueSMSAuditsQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetOverdueSMSAuditsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAudit>>> HandleAsync(GetOverdueSMSAuditsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetOverdueSMSAuditsQuery");

            var result = await _dataService.GetOverdueAuditsAsync(request.DepartmentFilter, request.AuditorFilter, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetOverdueSMSAuditsQuery");
            return Result<List<SMSAudit>>.Failure<List<SMSAudit>>(new Error("QUERY_FAILED", "Failed to get overdue audits"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Execution Dashboard
/// </summary>
public class GetSMSAuditExecutionDashboardQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditExecutionDashboardQuery, Result<SMSAuditExecutionDashboard>>
{
    private readonly SMSAuditDataService _dataService;
    private readonly ILogger<GetSMSAuditExecutionDashboardQueryHandler> _logger;

    public GetSMSAuditExecutionDashboardQueryHandler(
        SMSAuditDataService dataService,
        ILogger<GetSMSAuditExecutionDashboardQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditExecutionDashboard>> HandleAsync(GetSMSAuditExecutionDashboardQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditExecutionDashboardQuery");

            var result = await _dataService.GetAuditExecutionDashboardAsync(
                request.StartDate,
                request.EndDate,
                request.DepartmentFilter,
                request.AuditorFilter,
                ct);

            if (result.IsSuccess)
            {
                // Convert Domain model to Application model
                var applicationModel = result.Value.ToApplicationModel();
                return Result<SMSAuditExecutionDashboard>.Success(applicationModel);
            }

            return Result<SMSAuditExecutionDashboard>.Failure<SMSAuditExecutionDashboard>(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetSMSAuditExecutionDashboardQuery");
            return Result<SMSAuditExecutionDashboard>.Failure<SMSAuditExecutionDashboard>(new Error("QUERY_FAILED", "Failed to get audit execution dashboard"));
        }
    }
}