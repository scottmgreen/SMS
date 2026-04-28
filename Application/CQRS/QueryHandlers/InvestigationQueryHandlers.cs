//-----------------------------------------------------------------------
// <copyright file="InvestigationQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS investigation data retrieval and tracking logic.
//                  Defines contract for application services ensuring clean architecture
//                  boundaries and dependency inversion compliance.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// INVESTIGATION QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetInvestigationByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetInvestigationByCodeQuery, Result<Investigation>>
{
    private readonly InvestigationService _investigationService;
    private readonly ILogger<GetInvestigationByCodeQueryHandler> _logger;

    public GetInvestigationByCodeQueryHandler(InvestigationService investigationService, ILogger<GetInvestigationByCodeQueryHandler> logger)
    {
        _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Investigation>> HandleAsync(GetInvestigationByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetInvestigationByCodeQuery for Code: {Code}", request.InvestigationId.Value);
            var result = await _investigationService.GetInvestigationByCodeAsync(new InvestigationID(request.InvestigationId.Value), ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError($"Error processing GetInvestigationByCodeQuery for Code: {request.InvestigationId.Value}", ApplicationEventIds.Error, ex);
            return Result<Investigation>.Failure<Investigation>(DomainErrors.InvestigationError.NotFound);
        }
    }
}

public class GetAllInvestigationsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllInvestigationsQuery, Result<List<Investigation>>>
{
    private readonly InvestigationService _investigationService;
    private readonly ILogger<GetAllInvestigationsQueryHandler> _logger;

    public GetAllInvestigationsQueryHandler(InvestigationService investigationService, ILogger<GetAllInvestigationsQueryHandler> logger)
    {
        _investigationService = investigationService ?? throw new ArgumentNullException(nameof(investigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Investigation>>> HandleAsync(GetAllInvestigationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("✅ Clean Architecture: Processing GetAllInvestigationsQuery");
            var result = await _investigationService.GetAllInvestigationsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllInvestigationsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Investigation>>.Failure<List<Investigation>>(DomainErrors.InvestigationError.NullOrEmpty);
        }
    }
}
