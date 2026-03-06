//-----------------------------------------------------------------------
// <copyright file="HazardFileQueries.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query definitions for SMS hazard data retrieval and analysis operations.
//                  Defines query objects for read operations in the CQRS pattern.
//                  Queries retrieve data without causing side effects.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Models;

namespace SMS_Application.Messaging.Queries;

// =============================================
// HAZARD FILE QUERIES - Following Exact SMS Pattern
// =============================================


public class GetHazardFileByCodeQuery : BaseQueryBundle, IRequest<Result<HazardFile>>
{
    public string Code { get; set; }

    public GetHazardFileByCodeQuery(string code)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
}

public class GetHazardFilesByHazardCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string HazardCode { get; set; }
    public bool IncludeFileData { get; set; }
    public string? Category { get; set; }

    public GetHazardFilesByHazardCodeQuery(string hazardCode, bool includeFileData = false, string? category = null)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        IncludeFileData = includeFileData;
        Category = category;
    }
}

public class GetHazardFilesByReportCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string ReportCode { get; set; }
    public bool IncludeFileData { get; set; }

    public GetHazardFilesByReportCodeQuery(string reportCode, bool includeFileData = false)
    {
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
        IncludeFileData = includeFileData;
    }
}

public class GetHazardFileDataQuery : BaseQueryBundle, IRequest<Result<HazardFile>>
{
    public string Code { get; set; }

    public GetHazardFileDataQuery(string code)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }
}

public class GetActiveHazardFilesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public GetActiveHazardFilesQuery()
    {
    }
}

public class SearchHazardFilesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string? HazardCode { get; set; }
    public string? ReportCode { get; set; }
    public string? FileType { get; set; }
    public string? Category { get; set; }
    public string? SearchText { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool IncludeConfidential { get; set; }
    public int MaxResults { get; set; }

    public SearchHazardFilesQuery(
        string? hazardCode = null,
        string? reportCode = null,
        string? fileType = null,
        string? category = null,
        string? searchText = null,
        string? uploadedBy = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        bool includeConfidential = false,
        int maxResults = 100)
    {
        HazardCode = hazardCode;
        ReportCode = reportCode;
        FileType = fileType;
        Category = category;
        SearchText = searchText;
        UploadedBy = uploadedBy;
        DateFrom = dateFrom;
        DateTo = dateTo;
        IncludeConfidential = includeConfidential;
        MaxResults = maxResults;
    }
}

public class GetHazardFileStatisticsQuery : BaseQueryBundle, IRequest<Result<HazardFileStatistics>>
{
    public string HazardCode { get; set; }

    public GetHazardFileStatisticsQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}

public class GetHazardPhotosQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string HazardCode { get; set; }

    public GetHazardPhotosQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}

public class GetHazardDocumentsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string HazardCode { get; set; }

    public GetHazardDocumentsQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}

public class GetHazardVideosQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string HazardCode { get; set; }

    public GetHazardVideosQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}

public class GetConfidentialHazardFilesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>
{
    public string HazardCode { get; set; }

    public GetConfidentialHazardFilesQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }
}
