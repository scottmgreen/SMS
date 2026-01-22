using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD QUERY HANDLERS
// =============================================

public class GetHazardByIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardByIdQuery, Result<Hazard>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly HazardLocationDataService _locationDataService;
    private readonly ILogger<GetHazardByIdQueryHandler> _logger;

    public GetHazardByIdQueryHandler(HazardDataService hazardDataService, HazardLocationDataService hazardLocationDataService, ILogger<GetHazardByIdQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _locationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(GetHazardByIdQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardByIdQuery for Code: {Code}", request.HazardId);
            var result = await _hazardDataService.GetHazardByCodeAsync(request.HazardId, ct).ConfigureAwait(false);
            var location = await _locationDataService.GetHazardLocationsByHazardCodeAsync(request.HazardId.Value, ct);
            result.Value.HazardLocation = location.Value.FirstOrDefault();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardByIdQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }
}

public class GetAllHazardsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllHazardsQuery, Result<List<Hazard>>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetAllHazardsQueryHandler> _logger;

    public GetAllHazardsQueryHandler(HazardDataService hazardDataService, HazardLocationDataService hazardLocationDataService, ILogger<GetAllHazardsQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Hazard>>> HandleAsync(GetAllHazardsQuery request, CancellationToken ct = default)
    {
        try
        {

            _logger.LogInformation("Processing GetAllHazardsQuery");
            var result = await _hazardDataService.GetAllHazardsAsync(ct).ConfigureAwait(false);
            List<Hazard> hazards = new();
            foreach (Hazard hz in result.Value)
            {
                string code = hz.Code;
                hz.HazardLocation = _hazardLocationDataService.GetHazardLocationsByHazardCodeAsync(code).Result.Value.FirstOrDefault();
                hazards.Add(hz);
            }

            return hazards;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllHazardsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }
}

public class GetHazardsByReportIdQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardsByReportIdQuery, Result<List<Hazard>>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetHazardsByReportIdQueryHandler> _logger;

    public GetHazardsByReportIdQueryHandler(HazardDataService hazardDataService, HazardLocationDataService hazardLocationDataService, ILogger<GetHazardsByReportIdQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task<Result<List<Hazard>>> HandleAsync(GetHazardsByReportIdQuery request, CancellationToken ct = default)
    {
        try
        {
            ReportID reportid = request.ReportId;

            _logger.LogInformation("Processing GetAllHazardsByReportIdQuery");
            var result = await _hazardDataService.GetHazardsByReportIdAsync(reportid, ct).ConfigureAwait(false);
            List<Hazard> hazards = new();
            foreach (Hazard hz in result.Value)
            {
                string code = hz.Code;
                hz.HazardLocation = _hazardLocationDataService.GetHazardLocationsByHazardCodeAsync(code).Result.Value.FirstOrDefault();
                hazards.Add(hz);
            }


            return hazards;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetAllHazardsQuery", ApplicationEventIds.Error, ex);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }
}

public class GetHazardsByReportCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardsByReportCodeQuery, Result<List<Hazard>>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly HazardLocationDataService _hazardLocationDataService;
    private readonly ILogger<GetHazardsByReportCodeQueryHandler> _logger;

    public GetHazardsByReportCodeQueryHandler(HazardDataService hazardDataService, HazardLocationDataService hazardLocationDataService, ILogger<GetHazardsByReportCodeQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _hazardLocationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<Hazard>>> HandleAsync(GetHazardsByReportCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardsByReportCodeQuery for ReportCode: {ReportCode}", request.ReportCode);

            // Convert report code to ReportID and use existing method
            var reportId = new ReportID(request.ReportCode);

            // ? FIXED: Use the clean method that doesn't include complex mitigation joins
            var result = await _hazardDataService.GetHazardsByReportIdAsync(reportId, ct).ConfigureAwait(false);

            if (result.IsFailure || result.Value == null)
            {
                return result;
            }

            // Enhance hazards with location data (if needed)
            List<Hazard> hazards = new();
            foreach (Hazard hz in result.Value)
            {
                string code = hz.Code;
                var locationResult = await _hazardLocationDataService.GetHazardLocationsByHazardCodeAsync(code, ct);
                if (locationResult.IsSuccess && locationResult.Value?.Any() == true)
                {
                    hz.HazardLocation = locationResult.Value.FirstOrDefault();
                }
                hazards.Add(hz);
            }

            // ? PERFORMANCE NOTE: If you need mitigations, load them separately:
            // var hazardsWithMitigations = await _hazardDataService.GetHazardsByReportIdWithMitigationsAsync(reportId, ct);

            return Result<List<Hazard>>.Success(hazards);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardsByReportCodeQuery for ReportCode: {ReportCode}", ApplicationEventIds.Error, ex);
            return Result<List<Hazard>>.Failure<List<Hazard>>(DomainErrors.HazardError.NullOrEmpty);
        }
    }
}

public class GetHazardByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardByCodeQuery, Result<Hazard>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly HazardLocationDataService _locationDataService;
    private readonly ILogger<GetHazardByCodeQueryHandler> _logger;

    public GetHazardByCodeQueryHandler(HazardDataService hazardDataService, HazardLocationDataService hazardLocationDataService, ILogger<GetHazardByCodeQueryHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _locationDataService = hazardLocationDataService ?? throw new ArgumentNullException(nameof(hazardLocationDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(GetHazardByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardByCodeQuery for Code: {Code}", request.Code);

            // Convert string code to HazardID and use existing method
            var hazardId = new HazardID(request.Code);
            var result = await _hazardDataService.GetHazardByCodeAsync(hazardId, ct).ConfigureAwait(false);

            if (result.IsSuccess && result.Value != null)
            {
                var location = await _locationDataService.GetHazardLocationsByHazardCodeAsync(request.Code, ct);
                if (location.IsSuccess && location.Value?.Any() == true)
                {
                    result.Value.HazardLocation = location.Value.FirstOrDefault();
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NotFound);
        }
    }
}