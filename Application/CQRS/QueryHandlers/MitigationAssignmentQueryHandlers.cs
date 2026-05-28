//-----------------------------------------------------------------------
// <copyright file="MitigationAssignmentQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing data retrieval logic for SMS read operations.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;

using SMS_Application.Queries;

namespace SMS_Application.QueryHandlers;

// =============================================
// MITIGATION ASSIGNMENT QUERY HANDLERS
// =============================================

public class GetMitigationAssignmentByIdQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetMitigationAssignmentByIdQuery, Result<MitigationAssignment>>
{
    private readonly MitigationAssignmentDataService _mitigationAssignmentDataService;
    private readonly ILogger<GetMitigationAssignmentByIdQueryHandler> _logger;

    public GetMitigationAssignmentByIdQueryHandler(MitigationAssignmentDataService mitigationAssignmentDataService, ILogger<GetMitigationAssignmentByIdQueryHandler> logger)
    {
        _mitigationAssignmentDataService = mitigationAssignmentDataService ?? throw new ArgumentNullException(nameof(mitigationAssignmentDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<MitigationAssignment>> HandleAsync(GetMitigationAssignmentByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetMitigationAssignmentByIdQuery for ID: {Id}", request.MitigationAssignmentId);
            var result = await _mitigationAssignmentDataService.GetMitigationAssignmentByIdAsync(request.MitigationAssignmentId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetMitigationAssignmentByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<MitigationAssignment>.Failure<MitigationAssignment>(DomainErrors.MitigationAssignmentError.NotFound);
        }
    }
}

public class GetAllMitigationAssignmentsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllMitigationAssignmentsQuery, Result<List<MitigationAssignment>>>
{
    private readonly MitigationAssignmentDataService _mitigationAssignmentDataService;
    private readonly ILogger<GetAllMitigationAssignmentsQueryHandler> _logger;

    public GetAllMitigationAssignmentsQueryHandler(MitigationAssignmentDataService mitigationAssignmentDataService, ILogger<GetAllMitigationAssignmentsQueryHandler> logger)
    {
        _mitigationAssignmentDataService = mitigationAssignmentDataService ?? throw new ArgumentNullException(nameof(mitigationAssignmentDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<MitigationAssignment>>> HandleAsync(GetAllMitigationAssignmentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllMitigationAssignmentsQuery");
            var result = await _mitigationAssignmentDataService.GetAllMitigationAssignmentsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllMitigationAssignmentsQuery", ApplicationEventIds.Error, ex);
            return Result<List<MitigationAssignment>>.Failure<List<MitigationAssignment>>(DomainErrors.MitigationAssignmentError.NullOrEmpty);
        }
    }
}

