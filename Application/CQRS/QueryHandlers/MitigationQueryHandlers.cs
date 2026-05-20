//-----------------------------------------------------------------------
// <copyright file="MitigationQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing data retrieval logic for SMS read operations.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// MITIGATION QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetMitigationByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetMitigationByCodeQuery, Result<Mitigation>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<GetMitigationByCodeQueryHandler> _logger;

    public GetMitigationByCodeQueryHandler(IMitigationService mitigationService, ILogger<GetMitigationByCodeQueryHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Mitigation>> HandleAsync(GetMitigationByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            if (request?.MitigationId is null)
            {
                _logger.LogApplicationError("GetMitigationByIdQuery received with null MitigationId", ApplicationEventIds.Error, null);
                return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
            }

            _logger.LogInformation(" Processing GetMitigationByIdQuery for ID: {Id}", request.MitigationId.Value);

            var result = await _mitigationService.GetMitigationByCodeAsync(new MitigationID(request.MitigationId.Value), ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully retrieved Mitigation with ID: {Id}", request.MitigationId.Value);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve Mitigation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetMitigationByIdQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving Mitigation", ApplicationEventIds.Error, ex);
            return Result<Mitigation>.Failure<Mitigation>(DomainErrors.MitigationError.NotFound);
        }
    }
}

public class GetAllMitigationsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllMitigationsQuery, Result<List<Mitigation>>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<GetAllMitigationsQueryHandler> _logger;

    public GetAllMitigationsQueryHandler(IMitigationService mitigationService, ILogger<GetAllMitigationsQueryHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Mitigation>>> HandleAsync(GetAllMitigationsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(" Processing GetAllMitigationsQuery");

            var result = await _mitigationService.GetAllMitigationsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(" Successfully retrieved {Count} Mitigations", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogApplicationError("Failed to retrieve Mitigations. Error: {Error}", ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllMitigationsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while retrieving all Mitigations", ApplicationEventIds.Error, ex);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NotFound);
        }
    }
}

/// <summary>
/// Handler for getting mitigations by hazard code
/// </summary>
public class GetMitigationsByHazardCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetMitigationsByHazardCodeQuery, Result<List<Mitigation>>>
{
    private readonly IMitigationService _mitigationService;
    private readonly ILogger<GetMitigationsByHazardCodeQueryHandler> _logger;

    public GetMitigationsByHazardCodeQueryHandler(IMitigationService mitigationService, ILogger<GetMitigationsByHazardCodeQueryHandler> logger)
    {
        _mitigationService = mitigationService ?? throw new ArgumentNullException(nameof(mitigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Mitigation>>> HandleAsync(GetMitigationsByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(" Processing GetMitigationsByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _mitigationService.GetMitigationsByHazardCodeAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetMitigationsByHazardCodeQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<List<Mitigation>>.Failure<List<Mitigation>>(DomainErrors.MitigationError.NullOrEmpty);
        }
    }
}
