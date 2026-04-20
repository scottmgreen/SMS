//-----------------------------------------------------------------------
// <copyright file="ReportValidationQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS report data retrieval and search logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// REPORT VALIDATION QUERY HANDLERS
// =============================================

public class GetReportValidationByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetReportValidationByIdQuery, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _reportValidationDataService;
    private readonly ILogger<GetReportValidationByIdQueryHandler> _logger;

    public GetReportValidationByIdQueryHandler(ReportValidationDataService reportValidationDataService, ILogger<GetReportValidationByIdQueryHandler> logger)
    {
        _reportValidationDataService = reportValidationDataService ?? throw new ArgumentNullException(nameof(reportValidationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(GetReportValidationByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetReportValidationByIdQuery for ID: {Id}", request.ReportValidationId);
            var result = await _reportValidationDataService.GetReportValidationByIdAsync(request.ReportValidationId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetReportValidationByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
        }
    }
}


public class GetReportValidationByReportIdQueryHandler : BaseQueryBundle, IRequestHandler<GetReportValidationByReportIdQuery, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _reportValidationDataService;
    private readonly ILogger<GetReportValidationByReportIdQueryHandler> _logger;

    public GetReportValidationByReportIdQueryHandler(ReportValidationDataService reportValidationDataService, ILogger<GetReportValidationByReportIdQueryHandler> logger)
    {
        _reportValidationDataService = reportValidationDataService ?? throw new ArgumentNullException(nameof(reportValidationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ReportValidation>> HandleAsync(GetReportValidationByReportIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetReportValidationByIdQuery for ID: {Id}", request.ReportId);
            var result = await _reportValidationDataService.GetReportValidationByReportIdAsync(request.ReportId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetReportValidationByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NotFound);
        }
    }
}

public class GetAllReportValidationsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllReportValidationsQuery, Result<List<ReportValidation>>>
{
    private readonly ReportValidationDataService _reportValidationDataService;
    private readonly ILogger<GetAllReportValidationsQueryHandler> _logger;

    public GetAllReportValidationsQueryHandler(ReportValidationDataService reportValidationDataService, ILogger<GetAllReportValidationsQueryHandler> logger)
    {
        _reportValidationDataService = reportValidationDataService ?? throw new ArgumentNullException(nameof(reportValidationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ReportValidation>>> HandleAsync(GetAllReportValidationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllReportValidationsQuery");
            var result = await _reportValidationDataService.GetAllReportValidationsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllReportValidationsQuery", ApplicationEventIds.Error, ex);
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NullOrEmpty);
        }
    }
}

/// <summary>
/// Query handler for getting validated SMS risks by date range
/// </summary>
public class GetValidatedSMSRisksByDateQueryHandler : BaseQueryBundle, IRequestHandler<GetValidatedSMSRisksByDateQuery, Result<List<ReportValidation>>>
{
    private readonly ReportValidationDataService _reportValidationDataService;
    private readonly ILogger<GetValidatedSMSRisksByDateQueryHandler> _logger;

    public GetValidatedSMSRisksByDateQueryHandler(
        ReportValidationDataService reportValidationDataService, 
        ILogger<GetValidatedSMSRisksByDateQueryHandler> logger)
    {
        _reportValidationDataService = reportValidationDataService ?? throw new ArgumentNullException(nameof(reportValidationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ReportValidation>>> HandleAsync(GetValidatedSMSRisksByDateQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("🔍 Processing GetValidatedSMSRisksByDateQuery for {FromDate} to {ToDate}", 
                request.FromDate, request.ToDate);

            // Get all report validations first
            var allValidationsResult = await _reportValidationDataService.GetAllReportValidationsAsync(ct).ConfigureAwait(false);

            if (allValidationsResult.IsFailure)
            {
                _logger.LogError("❌ Failed to retrieve report validations: {Error}", allValidationsResult.Error?.Message);
                return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(allValidationsResult.Error);
            }

            // Filter for SMS_RISK validations within date range
            var validatedRisks = allValidationsResult.Value
                .Where(rv => rv.ValidationDecision == ValidationDecision.SmsRisk.Value && 
                           rv.ValidatedDate.HasValue &&
                           rv.ValidatedDate.Value >= request.FromDate &&
                           rv.ValidatedDate.Value <= request.ToDate)
                .ToList();

            _logger.LogInformation("✅ Found {Count} validated SMS risks between {FromDate} and {ToDate}", 
                validatedRisks.Count, request.FromDate.ToString("yyyy-MM-dd"), request.ToDate.ToString("yyyy-MM-dd"));

            return Result<List<ReportValidation>>.Success(validatedRisks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error processing GetValidatedSMSRisksByDateQuery for {FromDate} to {ToDate}", 
                request.FromDate, request.ToDate);
            return Result<List<ReportValidation>>.Failure<List<ReportValidation>>(DomainErrors.ReportError.NotFound);
        }
    }
}
