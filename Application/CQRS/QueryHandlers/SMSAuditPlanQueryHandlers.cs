//-----------------------------------------------------------------------
// <copyright file="SMSAuditPlanQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for SMS audit data retrieval and reporting logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// SMS AUDIT PLAN QUERY HANDLERS
// =============================================

/// <summary>
/// Query handler for getting all SMS Audit Plans
/// </summary>
public class GetAllSMSAuditPlansQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSMSAuditPlansQuery, Result<List<SMSAuditPlan>>>
{
    private readonly SMSAuditPlanDataService _dataService;
    private readonly ILogger<GetAllSMSAuditPlansQueryHandler> _logger;

    public GetAllSMSAuditPlansQueryHandler(
        SMSAuditPlanDataService dataService,
        ILogger<GetAllSMSAuditPlansQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditPlan>>> HandleAsync(GetAllSMSAuditPlansQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllSMSAuditPlansQuery");

            var result = await _dataService.GetAllAuditPlansAsync(ct);

            if (result.IsSuccess)
            {
                var auditPlans = result.Value;

                // Apply filters if specified
                if (!string.IsNullOrEmpty(request.StatusFilter))
                {
                    auditPlans = auditPlans.Where(ap => ap.Status.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.AuditTypeFilter))
                {
                    auditPlans = auditPlans.Where(ap => ap.AuditType.Equals(request.AuditTypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.DepartmentFilter))
                {
                    auditPlans = auditPlans.Where(ap => ap.ResponsibleDepartment.Equals(request.DepartmentFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (request.StartDateFrom.HasValue)
                {
                    auditPlans = auditPlans.Where(ap => ap.PlannedStartDate >= request.StartDateFrom.Value).ToList();
                }

                if (request.StartDateTo.HasValue)
                {
                    auditPlans = auditPlans.Where(ap => ap.PlannedStartDate <= request.StartDateTo.Value).ToList();
                }

                return Result<List<SMSAuditPlan>>.Success(auditPlans);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllSMSAuditPlansQuery", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("QUERY_FAILED", "Failed to get audit plans"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Plan by Code
/// </summary>
public class GetSMSAuditPlanByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSAuditPlanByCodeQuery, Result<SMSAuditPlan>>
{
    private readonly SMSAuditPlanDataService _dataService;
    private readonly ILogger<GetSMSAuditPlanByCodeQueryHandler> _logger;

    public GetSMSAuditPlanByCodeQueryHandler(
        SMSAuditPlanDataService dataService,
        ILogger<GetSMSAuditPlanByCodeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditPlan>> HandleAsync(GetSMSAuditPlanByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditPlanByCodeQuery for Code: {AuditPlanCode}", request.AuditPlanCode);

            var result = await _dataService.GetAuditPlanByCodeAsync(request.AuditPlanCode, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditPlanByCodeQuery for Code: {AuditPlanCode}", ApplicationEventIds.Error, ex);
            return Result<SMSAuditPlan>.Failure<SMSAuditPlan>(new Error("QUERY_FAILED", "Failed to get audit plan"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Plans by Type
/// </summary>
public class GetSMSAuditPlansByTypeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSAuditPlansByTypeQuery, Result<List<SMSAuditPlan>>>
{
    private readonly SMSAuditPlanDataService _dataService;
    private readonly ILogger<GetSMSAuditPlansByTypeQueryHandler> _logger;

    public GetSMSAuditPlansByTypeQueryHandler(
        SMSAuditPlanDataService dataService,
        ILogger<GetSMSAuditPlansByTypeQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditPlan>>> HandleAsync(GetSMSAuditPlansByTypeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditPlansByTypeQuery for Type: {AuditType}", request.AuditType);

            var result = await _dataService.GetAuditPlansByTypeAsync(request.AuditType, request.StatusFilter, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditPlansByTypeQuery for Type: {AuditType}", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("QUERY_FAILED", "Failed to get audit plans by type"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Plans by Department
/// </summary>
public class GetSMSAuditPlansByDepartmentQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSAuditPlansByDepartmentQuery, Result<List<SMSAuditPlan>>>
{
    private readonly SMSAuditPlanDataService _dataService;
    private readonly ILogger<GetSMSAuditPlansByDepartmentQueryHandler> _logger;

    public GetSMSAuditPlansByDepartmentQueryHandler(
        SMSAuditPlanDataService dataService,
        ILogger<GetSMSAuditPlansByDepartmentQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditPlan>>> HandleAsync(GetSMSAuditPlansByDepartmentQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditPlansByDepartmentQuery for Department: {Department}", request.Department);

            var result = await _dataService.GetAuditPlansByDepartmentAsync(request.Department, request.StatusFilter, ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditPlansByDepartmentQuery for Department: {Department}", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("QUERY_FAILED", "Failed to get audit plans by department"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Plans requiring approval
/// </summary>
public class GetSMSAuditPlansRequiringApprovalQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSAuditPlansRequiringApprovalQuery, Result<List<SMSAuditPlan>>>
{
    private readonly SMSAuditPlanDataService _dataService;
    private readonly ILogger<GetSMSAuditPlansRequiringApprovalQueryHandler> _logger;

    public GetSMSAuditPlansRequiringApprovalQueryHandler(
        SMSAuditPlanDataService dataService,
        ILogger<GetSMSAuditPlansRequiringApprovalQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SMSAuditPlan>>> HandleAsync(GetSMSAuditPlansRequiringApprovalQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditPlansRequiringApprovalQuery");

            var result = await _dataService.GetAuditPlansRequiringApprovalAsync(ct);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditPlansRequiringApprovalQuery", ApplicationEventIds.Error, ex);
            return Result<List<SMSAuditPlan>>.Failure<List<SMSAuditPlan>>(new Error("QUERY_FAILED", "Failed to get audit plans requiring approval"));
        }
    }
}

/// <summary>
/// Query handler for getting SMS Audit Calendar data
/// </summary>
public class GetSMSAuditCalendarQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSMSAuditCalendarQuery, Result<SMSAuditCalendarData>>
{
    private readonly SMSAuditPlanDataService _dataService;
    private readonly ILogger<GetSMSAuditCalendarQueryHandler> _logger;

    public GetSMSAuditCalendarQueryHandler(
        SMSAuditPlanDataService dataService,
        ILogger<GetSMSAuditCalendarQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SMSAuditCalendarData>> HandleAsync(GetSMSAuditCalendarQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetSMSAuditCalendarQuery for date range: {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);

            var result = await _dataService.GetAuditCalendarDataAsync(
                request.StartDate,
                request.EndDate,
                request.DepartmentFilter,
                request.AuditTypeFilter,
                request.AuditorFilter,
                ct);

            if (result.IsSuccess)
            {
                // Convert single calendar event to aggregate calendar data model
                var applicationModel = result.Value.ToApplicationModel();
                return Result<SMSAuditCalendarData>.Success(applicationModel);
            }

            return Result<SMSAuditCalendarData>.Failure<SMSAuditCalendarData>(result.Error);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetSMSAuditCalendarQuery", ApplicationEventIds.Error, ex);
            return Result<SMSAuditCalendarData>.Failure<SMSAuditCalendarData>(new Error("QUERY_FAILED", "Failed to get audit calendar data"));
        }
    }
}
