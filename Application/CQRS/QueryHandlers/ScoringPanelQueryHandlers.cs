//-----------------------------------------------------------------------
// <copyright file="ScoringPanelQueryHandlers.cs" company="SMS Safety Management System">
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
// SCORING PANEL QUERY HANDLERS
// =============================================

public class GetScoringPanelByIdQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetScoringPanelByIdQuery, Result<ScoringPanel>>
{
    private readonly IScoringPanelService _scoringPanelService;
    private readonly ILogger<GetScoringPanelByIdQueryHandler> _logger;

    public GetScoringPanelByIdQueryHandler(IScoringPanelService scoringPanelService, ILogger<GetScoringPanelByIdQueryHandler> logger)
    {
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ScoringPanel>> HandleAsync(GetScoringPanelByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetScoringPanelByIdQuery for ID: {Id}", request.ScoringPanelId);
            var result = await _scoringPanelService.GetScoringPanelByIdAsync(request.ScoringPanelId, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetScoringPanelByIdQuery for ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ScoringPanel>.Failure<ScoringPanel>(DomainErrors.ScoringPanelError.NotFound);
        }
    }
}

public class GetAllScoringPanelsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllScoringPanelsQuery, Result<List<ScoringPanel>>>
{
    private readonly IScoringPanelService _scoringPanelService;
    private readonly ILogger<GetAllScoringPanelsQueryHandler> _logger;

    public GetAllScoringPanelsQueryHandler(IScoringPanelService scoringPanelService, ILogger<GetAllScoringPanelsQueryHandler> logger)
    {
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ScoringPanel>>> HandleAsync(GetAllScoringPanelsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetAllScoringPanelsQuery");
            var result = await _scoringPanelService.GetAllScoringPanelsAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllScoringPanelsQuery", ApplicationEventIds.Error, ex);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }
}

public class GetScoringPanelsByHazardCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetScoringPanelsByHazardCodeQuery, Result<List<ScoringPanel>>>
{
    private readonly IScoringPanelService _scoringPanelService;
    private readonly ILogger<GetScoringPanelsByHazardCodeQueryHandler> _logger;

    public GetScoringPanelsByHazardCodeQueryHandler(IScoringPanelService scoringPanelService, ILogger<GetScoringPanelsByHazardCodeQueryHandler> logger)
    {
        _scoringPanelService = scoringPanelService ?? throw new ArgumentNullException(nameof(scoringPanelService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<ScoringPanel>>> HandleAsync(GetScoringPanelsByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetScoringPanelsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _scoringPanelService.GetScoringPanelsByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetScoringPanelsByHazardCodeQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<List<ScoringPanel>>.Failure<List<ScoringPanel>>(DomainErrors.ScoringPanelError.NullOrEmpty);
        }
    }
}

