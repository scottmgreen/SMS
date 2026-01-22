using Microsoft.Extensions.Logging;

using SMS_Application.Messaging.Queries;

namespace SMS_Application.Messaging.QueryHandlers;

// =============================================
// HAZARD FILE QUERY HANDLERS - Following Exact SMS Pattern
// =============================================


public class GetHazardFileByCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardFileByCodeQuery, Result<HazardFile>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardFileByCodeQueryHandler> _logger;

    public GetHazardFileByCodeQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardFileByCodeQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(GetHazardFileByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardFileByCodeQuery for Code: {Code}", request.Code);
            var result = await _hazardFileDataService.GetHazardFileByCodeAsync(request.Code, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFileByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardFilesByHazardCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardFilesByHazardCodeQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardFilesByHazardCodeQueryHandler> _logger;

    public GetHazardFilesByHazardCodeQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardFilesByHazardCodeQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardFilesByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardFilesByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileDataService.GetHazardFilesByHazardCodeAsync(request.HazardCode, request.IncludeFileData, request.Category, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFilesByHazardCodeQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardFilesByReportCodeQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardFilesByReportCodeQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardFilesByReportCodeQueryHandler> _logger;

    public GetHazardFilesByReportCodeQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardFilesByReportCodeQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardFilesByReportCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardFilesByReportCodeQuery for ReportCode: {ReportCode}", request.ReportCode);
            var result = await _hazardFileDataService.GetHazardFilesByReportCodeAsync(request.ReportCode, request.IncludeFileData, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFilesByReportCodeQuery for ReportCode: {ReportCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardFileDataQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardFileDataQuery, Result<HazardFile>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardFileDataQueryHandler> _logger;

    public GetHazardFileDataQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardFileDataQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(GetHazardFileDataQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardFileDataQuery for Code: {Code}", request.Code);
            var result = await _hazardFileDataService.GetHazardFileDataAsync(request.Code, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFileDataQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetActiveHazardFilesQueryHandler : BaseQueryBundle, IRequestHandler<GetActiveHazardFilesQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetActiveHazardFilesQueryHandler> _logger;

    public GetActiveHazardFilesQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetActiveHazardFilesQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetActiveHazardFilesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetActiveHazardFilesQuery");
            var result = await _hazardFileDataService.GetActiveHazardFilesAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetActiveHazardFilesQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class SearchHazardFilesQueryHandler : BaseQueryBundle, IRequestHandler<SearchHazardFilesQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<SearchHazardFilesQueryHandler> _logger;

    public SearchHazardFilesQueryHandler(HazardFileDataService hazardFileDataService, ILogger<SearchHazardFilesQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(SearchHazardFilesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing SearchHazardFilesQuery with criteria");
            var result = await _hazardFileDataService.SearchHazardFilesAsync(
                request.HazardCode, request.ReportCode, request.FileType, request.Category,
                request.SearchText, request.UploadedBy, request.DateFrom, request.DateTo,
                request.IncludeConfidential, request.MaxResults, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing SearchHazardFilesQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}



public class GetHazardPhotosQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardPhotosQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardPhotosQueryHandler> _logger;

    public GetHazardPhotosQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardPhotosQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardPhotosQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardPhotosQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileDataService.GetHazardPhotosAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardPhotosQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardDocumentsQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardDocumentsQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardDocumentsQueryHandler> _logger;

    public GetHazardDocumentsQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardDocumentsQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardDocumentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardDocumentsQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileDataService.GetHazardDocumentsAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardDocumentsQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardVideosQueryHandler : BaseQueryBundle, IRequestHandler<GetHazardVideosQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetHazardVideosQueryHandler> _logger;

    public GetHazardVideosQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetHazardVideosQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardVideosQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetHazardVideosQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileDataService.GetHazardVideosAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardVideosQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetConfidentialHazardFilesQueryHandler : BaseQueryBundle, IRequestHandler<GetConfidentialHazardFilesQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly HazardFileDataService _hazardFileDataService;
    private readonly ILogger<GetConfidentialHazardFilesQueryHandler> _logger;

    public GetConfidentialHazardFilesQueryHandler(HazardFileDataService hazardFileDataService, ILogger<GetConfidentialHazardFilesQueryHandler> logger)
    {
        _hazardFileDataService = hazardFileDataService ?? throw new ArgumentNullException(nameof(hazardFileDataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetConfidentialHazardFilesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetConfidentialHazardFilesQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileDataService.GetConfidentialHazardFilesAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetConfidentialHazardFilesQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}