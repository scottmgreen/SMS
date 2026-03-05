//-----------------------------------------------------------------------
// <copyright file="SMSAuditQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS audit data retrieval and reporting logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS AUDIT QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting all SMS Audits
/// </summary>
public class GetAllSMSAuditsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSAuditsQuery, Result<List<SMSAudit>>>
{
    private readonly SMSAuditService _dataService;
    private readonly ILogger<GetAllSMSAuditsQueryHandler> _logger;

    public GetAllSMSAuditsQueryHandler(
        SMSAuditService dataService,
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
            _logger.LogApplicationError("Error processing GetAllSMSAuditsQuery", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetSMSAuditByCodeQuery for Code: {AuditCode}", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetSMSAuditsByPlanQuery for Plan: {AuditPlanCode}", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetSMSAuditsByStatusQuery for Status: {Status}", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetSMSAuditsByAuditorQuery for Auditor: {Auditor}", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetOverdueSMSAuditsQuery", ApplicationEventIds.Error, ex);
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
            _logger.LogApplicationError("Error processing GetSMSAuditExecutionDashboardQuery", ApplicationEventIds.Error, ex);
            return Result<SMSAuditExecutionDashboard>.Failure<SMSAuditExecutionDashboard>(new Error("QUERY_FAILED", "Failed to get audit execution dashboard"));
        }
    }
}

// =============================================
// SMS AUDIT FINDING QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting SMS Audit Findings by Audit Code
/// </summary>
public class GetSMSAuditFindingsByAuditCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditFindingsByAuditCodeQuery, Result<List<SMSAuditFinding>>>
{
    private readonly SMSAuditFindingDataService _dataService;
    private readonly ILogger<GetSMSAuditFindingsByAuditCodeQueryHandler> _logger;

    public GetSMSAuditFindingsByAuditCodeQueryHandler(
        SMSAuditFindingDataService dataService,
        ILogger<GetSMSAuditFindingsByAuditCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditFinding>>> HandleAsync(GetSMSAuditFindingsByAuditCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditFindingsByAuditCodeQuery for AuditCode: {AuditCode}", request.AuditCode);

            var result = await _dataService.GetFindingsByAuditCodeAsync(request.AuditCode, ct);

            if (result.IsSuccess)
            {
                var findings = result.Value;

                // Apply filters if specified
                if (!string.IsNullOrEmpty(request.StatusFilter))
                {
                    findings = findings.Where(f => f.Status.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.SeverityFilter))
                {
                    findings = findings.Where(f => f.Severity.Equals(request.SeverityFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Result<List<SMSAuditFinding>>.Success(findings);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditFindingsByAuditCodeQuery for AuditCode: {AuditCode}", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditFinding>>.Failure<List<SMSAuditFinding>>(new Error("QUERY_FAILED", "Failed to get audit findings"));
        }
    }
}

/// <summary>
/// Query handler for getting all SMS Audit Findings
/// </summary>
public class GetAllSMSAuditFindingsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSAuditFindingsQuery, Result<List<SMSAuditFinding>>>
{
    private readonly SMSAuditFindingDataService _dataService;
    private readonly ILogger<GetAllSMSAuditFindingsQueryHandler> _logger;

    public GetAllSMSAuditFindingsQueryHandler(
        SMSAuditFindingDataService dataService,
        ILogger<GetAllSMSAuditFindingsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditFinding>>> HandleAsync(GetAllSMSAuditFindingsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSAuditFindingsQuery");

            var result = await _dataService.GetAllFindingsAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSAuditFindingsQuery", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditFinding>>.Failure<List<SMSAuditFinding>>(new Error("QUERY_FAILED", "Failed to get all audit findings"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Finding by Code
/// </summary>
public class GetSMSAuditFindingByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditFindingByCodeQuery, Result<SMSAuditFinding>>
{
    private readonly SMSAuditFindingDataService _dataService;
    private readonly ILogger<GetSMSAuditFindingByCodeQueryHandler> _logger;

    public GetSMSAuditFindingByCodeQueryHandler(
        SMSAuditFindingDataService dataService,
        ILogger<GetSMSAuditFindingByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditFinding>> HandleAsync(GetSMSAuditFindingByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditFindingByCodeQuery for FindingCode: {FindingCode}", request.FindingCode);

            var result = await _dataService.GetFindingByCodeAsync(request.FindingCode, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditFindingByCodeQuery for FindingCode: {FindingCode}", ApplicationEventIds.Error, ex);
            return Result<SMSAuditFinding>.Failure<SMSAuditFinding>(new Error("QUERY_FAILED", "Failed to get audit finding"));
        }
    }
}

/// <summary>
/// Query handler for getting overdue SMS Audit Findings
/// </summary>
public class GetOverdueSMSAuditFindingsQueryHandler : BaseQueryBundle, IRequestHandler<GetOverdueSMSAuditFindingsQuery, Result<List<SMSAuditFinding>>>
{
    private readonly SMSAuditFindingDataService _dataService;
    private readonly ILogger<GetOverdueSMSAuditFindingsQueryHandler> _logger;

    public GetOverdueSMSAuditFindingsQueryHandler(
        SMSAuditFindingDataService dataService,
        ILogger<GetOverdueSMSAuditFindingsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditFinding>>> HandleAsync(GetOverdueSMSAuditFindingsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetOverdueSMSAuditFindingsQuery");

            var result = await _dataService.GetOverdueFindingsAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetOverdueSMSAuditFindingsQuery", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditFinding>>.Failure<List<SMSAuditFinding>>(new Error("QUERY_FAILED", "Failed to get overdue audit findings"));
        }
    }
}

// =============================================
// SMS AUDIT EVIDENCE QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting SMS Audit Evidence by Audit Code
/// </summary>
public class GetSMSAuditEvidenceByAuditCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditEvidenceByAuditCodeQuery, Result<List<SMSAuditEvidence>>>
{
    private readonly SMSAuditEvidenceDataService _dataService;
    private readonly ILogger<GetSMSAuditEvidenceByAuditCodeQueryHandler> _logger;

    public GetSMSAuditEvidenceByAuditCodeQueryHandler(
        SMSAuditEvidenceDataService dataService,
        ILogger<GetSMSAuditEvidenceByAuditCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditEvidence>>> HandleAsync(GetSMSAuditEvidenceByAuditCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditEvidenceByAuditCodeQuery for AuditCode: {AuditCode}", request.AuditCode);

            var result = await _dataService.GetEvidenceByAuditCodeAsync(request.AuditCode, request.IncludeArchived, ct);

            if (result.IsSuccess)
            {
                var evidence = result.Value;

                // Apply filters if specified
                if (!string.IsNullOrEmpty(request.EvidenceTypeFilter))
                {
                    evidence = evidence.Where(e => e.EvidenceType.Equals(request.EvidenceTypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return Result<List<SMSAuditEvidence>>.Success(evidence);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditEvidenceByAuditCodeQuery for AuditCode: {AuditCode}", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditEvidence>>.Failure<List<SMSAuditEvidence>>(new Error("QUERY_FAILED", "Failed to get audit evidence"));
        }
    }
}

/// <summary>
/// Query handler for getting all SMS Audit Evidence
/// </summary>
public class GetAllSMSAuditEvidenceQueryHandler : BaseQueryBundle, IRequestHandler<GetAllSMSAuditEvidenceQuery, Result<List<SMSAuditEvidence>>>
{
    private readonly SMSAuditEvidenceDataService _dataService;
    private readonly ILogger<GetAllSMSAuditEvidenceQueryHandler> _logger;

    public GetAllSMSAuditEvidenceQueryHandler(
        SMSAuditEvidenceDataService dataService,
        ILogger<GetAllSMSAuditEvidenceQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditEvidence>>> HandleAsync(GetAllSMSAuditEvidenceQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSAuditEvidenceQuery");

            var result = await _dataService.GetAllEvidenceAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSAuditEvidenceQuery", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditEvidence>>.Failure<List<SMSAuditEvidence>>(new Error("QUERY_FAILED", "Failed to get all audit evidence"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Evidence by Code
/// </summary>
public class GetSMSAuditEvidenceByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditEvidenceByCodeQuery, Result<SMSAuditEvidence>>
{
    private readonly SMSAuditEvidenceDataService _dataService;
    private readonly ILogger<GetSMSAuditEvidenceByCodeQueryHandler> _logger;

    public GetSMSAuditEvidenceByCodeQueryHandler(
        SMSAuditEvidenceDataService dataService,
        ILogger<GetSMSAuditEvidenceByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditEvidence>> HandleAsync(GetSMSAuditEvidenceByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditEvidenceByCodeQuery for EvidenceCode: {EvidenceCode}", request.EvidenceCode);

            var result = await _dataService.GetEvidenceByCodeAsync(request.EvidenceCode, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditEvidenceByCodeQuery for EvidenceCode: {EvidenceCode}", ApplicationEventIds.Error, ex);
            return Result<SMSAuditEvidence>.Failure<SMSAuditEvidence>(new Error("QUERY_FAILED", "Failed to get audit evidence"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Evidence by Finding Code
/// </summary>
public class GetSMSAuditEvidenceByFindingCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditEvidenceByFindingCodeQuery, Result<List<SMSAuditEvidence>>>
{
    private readonly SMSAuditEvidenceDataService _dataService;
    private readonly ILogger<GetSMSAuditEvidenceByFindingCodeQueryHandler> _logger;

    public GetSMSAuditEvidenceByFindingCodeQueryHandler(
        SMSAuditEvidenceDataService dataService,
        ILogger<GetSMSAuditEvidenceByFindingCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditEvidence>>> HandleAsync(GetSMSAuditEvidenceByFindingCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditEvidenceByFindingCodeQuery for FindingCode: {FindingCode}", request.FindingCode);

            var result = await _dataService.GetEvidenceByFindingCodeAsync(request.FindingCode, request.IncludeArchived, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditEvidenceByFindingCodeQuery for FindingCode: {FindingCode}", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditEvidence>>.Failure<List<SMSAuditEvidence>>(new Error("QUERY_FAILED", "Failed to get audit evidence by finding"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Checklist Items by Audit Code
/// </summary>
public class GetSMSAuditChecklistItemsByAuditCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetSMSAuditChecklistItemsByAuditCodeQuery, Result<List<SMSAuditChecklistItem>>>
{
    private readonly ILogger<GetSMSAuditChecklistItemsByAuditCodeQueryHandler> _logger;

    public GetSMSAuditChecklistItemsByAuditCodeQueryHandler(
        ILogger<GetSMSAuditChecklistItemsByAuditCodeQueryHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditChecklistItem>>> HandleAsync(GetSMSAuditChecklistItemsByAuditCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditChecklistItemsByAuditCodeQuery for AuditCode: {AuditCode}", request.AuditCode);

            // For now, return an empty list since the checklist feature isn't fully implemented
            // TODO: Implement when SMSAuditChecklistDataService is available
            return await Task.FromResult(Result<List<SMSAuditChecklistItem>>.Success(new List<SMSAuditChecklistItem>()));
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditChecklistItemsByAuditCodeQuery for AuditCode: {AuditCode}", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditChecklistItem>>.Failure<List<SMSAuditChecklistItem>>(new Error("QUERY_FAILED", "Failed to get audit checklist items"));
        }
    }
}
