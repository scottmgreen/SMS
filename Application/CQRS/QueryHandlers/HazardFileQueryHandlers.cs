//-----------------------------------------------------------------------
// <copyright file="HazardFileQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers implementing SMS hazard data retrieval and analysis logic.
//                  Implements query handlers for processing read operations.
//                  Retrieves and transforms data for presentation layer consumption.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Queries;

namespace SMS_Application.QueryHandlers;

// =============================================
// HAZARD FILE QUERY HANDLERS - Clean Architecture Pattern
// =============================================

public class GetHazardFileByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardFileByCodeQuery, Result<HazardFile>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardFileByCodeQueryHandler> _logger;

    public GetHazardFileByCodeQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardFileByCodeQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(GetHazardFileByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardFileByCodeQuery for Code: {Code}", request.Code);
            var result = await _hazardFileService.GetHazardFileByCodeAsync(request.Code, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFileByCodeQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardFilesByHazardCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardFilesByHazardCodeQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardFilesByHazardCodeQueryHandler> _logger;

    public GetHazardFilesByHazardCodeQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardFilesByHazardCodeQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardFilesByHazardCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardFilesByHazardCodeQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileService.GetHazardFilesByHazardCodeAsync(request.HazardCode, request.IncludeFileData, request.Category, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFilesByHazardCodeQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardFilesByReportCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardFilesByReportCodeQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardFilesByReportCodeQueryHandler> _logger;

    public GetHazardFilesByReportCodeQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardFilesByReportCodeQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardFilesByReportCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardFilesByReportCodeQuery for ReportCode: {ReportCode}", request.ReportCode);
            var result = await _hazardFileService.GetHazardFilesByReportCodeAsync(request.ReportCode, request.IncludeFileData, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFilesByReportCodeQuery for ReportCode: {ReportCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardFileDataQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardFileDataQuery, Result<HazardFile>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardFileDataQueryHandler> _logger;

    public GetHazardFileDataQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardFileDataQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<HazardFile>> HandleAsync(GetHazardFileDataQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardFileDataQuery for Code: {Code}", request.Code);
            var result = await _hazardFileService.GetHazardFileDataAsync(request.Code, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardFileDataQuery for Code: {Code}", ApplicationEventIds.Error, ex);
            return Result<HazardFile>.Failure<HazardFile>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetActiveHazardFilesQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetActiveHazardFilesQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetActiveHazardFilesQueryHandler> _logger;

    public GetActiveHazardFilesQueryHandler(IHazardFileService hazardFileService, ILogger<GetActiveHazardFilesQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetActiveHazardFilesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetActiveHazardFilesQuery");
            var result = await _hazardFileService.GetActiveHazardFilesAsync(ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetActiveHazardFilesQuery", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class SearchHazardFilesQueryHandler : BaseQueryBundle, IBaseRequestHandler<SearchHazardFilesQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<SearchHazardFilesQueryHandler> _logger;

    public SearchHazardFilesQueryHandler(IHazardFileService hazardFileService, ILogger<SearchHazardFilesQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(SearchHazardFilesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing SearchHazardFilesQuery with criteria");
            var result = await _hazardFileService.SearchHazardFilesAsync(
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

public class GetHazardPhotosQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardPhotosQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardPhotosQueryHandler> _logger;

    public GetHazardPhotosQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardPhotosQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardPhotosQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardPhotosQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileService.GetHazardPhotosAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardPhotosQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardDocumentsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardDocumentsQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardDocumentsQueryHandler> _logger;

    public GetHazardDocumentsQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardDocumentsQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardDocumentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardDocumentsQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileService.GetHazardDocumentsAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardDocumentsQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetHazardVideosQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetHazardVideosQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetHazardVideosQueryHandler> _logger;

    public GetHazardVideosQueryHandler(IHazardFileService hazardFileService, ILogger<GetHazardVideosQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetHazardVideosQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetHazardVideosQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileService.GetHazardVideosAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetHazardVideosQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

public class GetConfidentialHazardFilesQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetConfidentialHazardFilesQuery, Result<IEnumerable<HazardFile>>>
{
    private readonly IHazardFileService _hazardFileService;
    private readonly ILogger<GetConfidentialHazardFilesQueryHandler> _logger;

    public GetConfidentialHazardFilesQueryHandler(IHazardFileService hazardFileService, ILogger<GetConfidentialHazardFilesQueryHandler> logger)
    {
        _hazardFileService = hazardFileService ?? throw new ArgumentNullException(nameof(hazardFileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<HazardFile>>> HandleAsync(GetConfidentialHazardFilesQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation(" Processing GetConfidentialHazardFilesQuery for HazardCode: {HazardCode}", request.HazardCode);
            var result = await _hazardFileService.GetConfidentialHazardFilesAsync(request.HazardCode, ct).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error processing GetConfidentialHazardFilesQuery for HazardCode: {HazardCode}", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<HazardFile>>.Failure<IEnumerable<HazardFile>>(DomainErrors.HazardFileError.NotFound);
        }
    }
}

