//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: CQRS query handlers for Safety Performance Indicator operations
//                  providing data retrieval and analysis capabilities for the SMS.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;

namespace SMS_Application.CQRS.QueryHandlers;

/// <summary>
/// Query handler for retrieving all Safety Performance Indicators
/// </summary>
public class GetAllSafetyPerformanceIndicatorsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetAllSafetyPerformanceIndicatorsQuery, Result<List<SafetyPerformanceIndicator>>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetAllSafetyPerformanceIndicatorsQueryHandler> _logger;

    public GetAllSafetyPerformanceIndicatorsQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetAllSafetyPerformanceIndicatorsQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<SafetyPerformanceIndicator>>> HandleAsync(GetAllSafetyPerformanceIndicatorsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? Processing GetAllSafetyPerformanceIndicatorsQuery");

            var result = await _spiService.GetAllSafetyPerformanceIndicatorsAsync(ct);

            if (result.IsSuccess && result.Value != null)
            {
                var spis = result.Value.ToList();

                // Apply filters if specified
                if (!string.IsNullOrEmpty(request.StatusFilter))
                {
                    spis = spis.Where(spi => spi.Status.Name.Equals(request.StatusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.TypeFilter))
                {
                    spis = spis.Where(spi => spi.IndicatorType.Name.Equals(request.TypeFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (!string.IsNullOrEmpty(request.DepartmentFilter))
                {
                    spis = spis.Where(spi => spi.ResponsibleDepartment.Equals(request.DepartmentFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                _logger.LogInformation("? Retrieved {Count} SPIs", spis.Count);
                return Result<List<SafetyPerformanceIndicator>>.Success(spis);
            }
            else
            {
                _logger.LogWarning("?? Failed to retrieve SPIs: {Error}", result.Error?.Message);
                return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(result.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error processing GetAllSafetyPerformanceIndicatorsQuery");
            return Result<List<SafetyPerformanceIndicator>>.Failure<List<SafetyPerformanceIndicator>>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for retrieving Safety Performance Indicator by ID
/// </summary>
public class GetSafetyPerformanceIndicatorByIdQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSafetyPerformanceIndicatorByIdQuery, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSafetyPerformanceIndicatorByIdQueryHandler> _logger;

    public GetSafetyPerformanceIndicatorByIdQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSafetyPerformanceIndicatorByIdQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(GetSafetyPerformanceIndicatorByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? Processing GetSafetyPerformanceIndicatorByIdQuery for ID: {Id}", request.SPIId?.Value);

            var result = await _spiService.GetSafetyPerformanceIndicatorByIdAsync(request.SPIId, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? Retrieved SPI with ID: {Id}", request.SPIId?.Value);
            }
            else
            {
                _logger.LogWarning("?? Failed to retrieve SPI with ID {Id}: {Error}", request.SPIId?.Value, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error processing GetSafetyPerformanceIndicatorByIdQuery for ID: {Id}", request.SPIId?.Value);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }
}

/// <summary>
/// Query handler for retrieving Safety Performance Indicator by Code
/// </summary>
public class GetSafetyPerformanceIndicatorByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetSafetyPerformanceIndicatorByCodeQuery, Result<SafetyPerformanceIndicator>>
{
    private readonly SafetyPerformanceIndicatorService _spiService;
    private readonly ILogger<GetSafetyPerformanceIndicatorByCodeQueryHandler> _logger;

    public GetSafetyPerformanceIndicatorByCodeQueryHandler(
        SafetyPerformanceIndicatorService spiService,
        ILogger<GetSafetyPerformanceIndicatorByCodeQueryHandler> logger)
    {
        _spiService = spiService ?? throw new ArgumentNullException(nameof(spiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<SafetyPerformanceIndicator>> HandleAsync(GetSafetyPerformanceIndicatorByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("?? Processing GetSafetyPerformanceIndicatorByCodeQuery for Code: {Code}", request.Code);

            var result = await _spiService.GetSafetyPerformanceIndicatorByCodeAsync(request.Code, ct);

            if (result.IsSuccess)
            {
                _logger.LogInformation("? Retrieved SPI with Code: {Code}", request.Code);
            }
            else
            {
                _logger.LogWarning("?? Failed to retrieve SPI with Code {Code}: {Error}", request.Code, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error processing GetSafetyPerformanceIndicatorByCodeQuery for Code: {Code}", request.Code);
            return Result<SafetyPerformanceIndicator>.Failure<SafetyPerformanceIndicator>(DomainErrors.SPIError.NotFound);
        }
    }
}