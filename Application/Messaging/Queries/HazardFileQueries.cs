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
// HAZARD FILE QUERIES WITH AUDIT TRACKING - Following Exact SMS Pattern
// =============================================

public class GetHazardFileByCodeQuery : BaseQueryBundle, IRequest<Result<HazardFile>>, IReadQuery
{
    public string Code { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardFileByCodeQuery(string code)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:{Code}";
    }

    public string GetAccessType()
    {
        return "GetById";
    }
}

public class GetHazardFilesByHazardCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    public string HazardCode { get; set; }
    public bool IncludeFileData { get; set; }
    public string? Category { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardFilesByHazardCodeQuery(string hazardCode, bool includeFileData = false, string? category = null)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        IncludeFileData = includeFileData;
        Category = category;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:ByHazard:{HazardCode}" + (Category != null ? $":Category:{Category}" : "");
    }

    public string GetAccessType()
    {
        return "GetByHazard";
    }
}

public class GetHazardFilesByReportCodeQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    public string ReportCode { get; set; }
    public bool IncludeFileData { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardFilesByReportCodeQuery(string reportCode, bool includeFileData = false)
    {
        ReportCode = reportCode ?? throw new ArgumentNullException(nameof(reportCode));
        IncludeFileData = includeFileData;
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:ByReport:{ReportCode}";
    }

    public string GetAccessType()
    {
        return "GetByReport";
    }
}

public class GetHazardFileDataQuery : BaseQueryBundle, IRequest<Result<HazardFile>>, IReadQuery
{
    public string Code { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardFileDataQuery(string code)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:Data:{Code}";
    }

    public string GetAccessType()
    {
        return "GetFileData";
    }
}

public class GetActiveHazardFilesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetActiveHazardFilesQuery()
    {
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return "HazardFile:Active";
    }

    public string GetAccessType()
    {
        return "GetActive";
    }
}

public class SearchHazardFilesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
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
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

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

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        var parts = new List<string> { "HazardFile:Search" };
        if (!string.IsNullOrEmpty(HazardCode)) parts.Add($"Hazard:{HazardCode}");
        if (!string.IsNullOrEmpty(ReportCode)) parts.Add($"Report:{ReportCode}");
        if (!string.IsNullOrEmpty(SearchText)) parts.Add($"Text:{SearchText[..Math.Min(20, SearchText.Length)]}");
        return string.Join(":", parts);
    }

    public string GetAccessType()
    {
        return "Search";
    }
}

public class GetHazardFileStatisticsQuery : BaseQueryBundle, IRequest<Result<HazardFileStatistics>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardFileStatisticsQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:Statistics:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetStatistics";
    }
}

public class GetHazardPhotosQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardPhotosQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:Photos:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetPhotos";
    }
}

public class GetHazardDocumentsQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardDocumentsQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:Documents:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetDocuments";
    }
}

public class GetHazardVideosQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetHazardVideosQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:Videos:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetVideos";
    }
}

public class GetConfidentialHazardFilesQuery : BaseQueryBundle, IRequest<Result<IEnumerable<HazardFile>>>, IReadQuery
{
    public string HazardCode { get; set; }
    
    // Audit properties for query tracking
    public string AccessedBy { get; private set; } = string.Empty;
    public DateTime? AccessedDate { get; private set; }

    public GetConfidentialHazardFilesQuery(string hazardCode)
    {
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
    }

    // IReadQuery implementation
    public void SetAccessedBy(string userId, DateTime timestamp)
    {
        AccessedBy = userId;
        AccessedDate = timestamp;
    }

    public string GetResourceIdentifier()
    {
        return $"HazardFile:Confidential:{HazardCode}";
    }

    public string GetAccessType()
    {
        return "GetConfidential";
    }
}
