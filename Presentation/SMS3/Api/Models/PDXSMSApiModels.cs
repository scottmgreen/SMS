//-----------------------------------------------------------------------
// <copyright file="PDXSMSApiModels.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: API data transfer objects for external SMS confidential reporting integration.
//                  Provides structured data models for external system integration
//                  with comprehensive validation and documentation.
// </copyright>
//-----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

using SMS_Domain.Enums;

namespace SMS3.Api.Models;

    /// <summary>
    /// Request model for external confidential report submissions (API v2)
    /// Omits HazardCategory and HazardType, which are not required in v2
    /// </summary>
    public record PDXSMSReportApiRequestV2
    {
        [Required]
        [StringLength(5000)]
        public string HazardDescription { get; init; } = string.Empty;

        public string? ReportSubmittedBy { get; init; }
        public DateTime? ReportSubmittedDate { get; init; }
        public string? ReportSubmittingDepartment { get; init; } = "EXTERNAL_API_SOURCE";
        public string? ReportSubmittingDepartmentJobFunction { get; init; } = "EXTERNAL_API_SOURCE";
        public bool? ReportIsAnonymous { get; init; }
        public string? ReportContactName { get; init; }
        public string? ReportContactCell { get; init; }
        public string? ReportContactEmail { get; init; }
        public string? ReportContactCompany { get; init; }
        [Required]
        public string LocationDescription { get; init; } = string.Empty;
        public decimal? LocationLatitude { get; init; }
        public decimal? LocationLongitude { get; init; }
        public DateTime? HazardIncidentDateTime { get; init; }
        [JsonConverter(typeof(SingleOrArrayConverter<FileAttachment>))]
        public List<FileAttachment>? ReportAttachments { get; init; }
    }

    /// <summary>
    /// Request model for external confidential report submissions
    /// </summary>
    public record PDXSMSReportApiRequestV1
    {
        /// <summary>
        /// The category of the hazard (required). Use GET /api/pdxsms/hazard-categories to see all valid values.
        /// Examples: "INCIDENT", "FOD", "WILDLIFE", "OPERATIONAL_CHANGE"
        /// </summary>
        /// <example>INCIDENT</example>
        [Required]
        public string HazardCategory { get; init; } = SMS_Domain.Enums.HazardCategory.Default.Value;

        /// <summary>
        /// Specific type of hazard being reported (required). Must match a valid value for the selected category.
        /// Use GET /api/pdxsms/hazard-types?category={categoryValue} to see valid types for a category.
        /// Examples: "AIRCRAFT_INCIDENT", "BIRD_STRIKE", "METAL_FOD"
        /// </summary>
        /// <example>AIRCRAFT_INCIDENT</example>
        [Required]
        public string HazardType { get; init; } = SMS_Domain.Enums.HazardType.Default.Value;

        /// <summary>
        /// Detailed description of the hazard or incident (required, max 2000 characters)
        /// </summary>
        /// <example>Aircraft experienced engine failure during takeoff roll, aborting takeoff safely</example>
        [Required]
        [StringLength(5000)]
        public string HazardDescription { get; init; } = string.Empty;

        public string? ReportSubmittedBy { get; init; }
        public DateTime? ReportSubmittedDate { get; init; }
        public string? ReportSubmittingDepartment { get; init; } = "EXTERNAL_API_SOURCE";
        public string? ReportSubmittingDepartmentJobFunction { get; init; } = "EXTERNAL_API_SOURCE";
        public bool? ReportIsAnonymous { get; init; }
        public string? ReportContactName { get; init; }
        public string? ReportContactCell { get; init; }
        public string? ReportContactEmail { get; init; }

        public string? ReportContactCompany { get; init; }
        /// <summary>
        /// Location where the hazard occurred (required)
        /// </summary>
        /// <example>Runway 28L, approximately 2000ft from threshold</example>
        [Required]
        public string LocationDescription { get; init; } = string.Empty;

        /// <summary>
        /// Optional latitude coordinate for precise location (decimal degrees)
        /// </summary>
        /// <example>45.5898</example>
        public decimal? LocationLatitude { get; init; }

        /// <summary>
        /// Optional longitude coordinate for precise location (decimal degrees)
        /// </summary>
        /// <example>-122.5951</example>
        public decimal? LocationLongitude { get; init; }

        /// <summary>
        /// Optional incident date/time (if not provided, current time is used)
        /// </summary>
        /// <example>2024-01-15T14:30:00Z</example>
        public DateTime? HazardIncidentDateTime { get; init; }

        /// <summary>
        /// Optional file attachments as base64 encoded strings (max 10MB per file)
        /// </summary>
        [JsonConverter(typeof(SingleOrArrayConverter<FileAttachment>))]
        public List<FileAttachment>? ReportAttachments { get; init; }
    }

    



    /// <summary>
    /// Response model for confidential report submissions - Enhanced for production
    /// </summary>
    public record PDXSMSReportApiResponse
    {
        /// <summary>
        /// Database-generated tracking ID for the submitted report
        /// </summary>
        public string TrackingId { get; init; } = string.Empty;
        
        /// <summary>
        /// Database-generated hazard ID
        /// </summary>
        public string HazardId { get; init; } = string.Empty;
        
        /// <summary>
        /// Database-generated report ID
        /// </summary>
        public string ReportId { get; init; } = string.Empty;
        
        /// <summary>
        /// Timestamp when the submission was processed
        /// </summary>
        public DateTime ReportSubmissionDateTime { get; init; }
        
        /// <summary>
        /// Processing status
        /// </summary>
        public string Status { get; init; } = string.Empty;
        
        /// <summary>
        /// Success or error message
        /// </summary>
        public string Message { get; init; } = string.Empty;
        
        /// <summary>
        /// Direct URL to track the status of this report
        /// </summary>
        /// <example></example>
        
        
        /// <summary>
        /// Number of files successfully processed
        /// </summary>
        public int ProcessedFiles { get; init; } = 0;
        
        /// <summary>
        /// Number of files that failed to process
        /// </summary>
        public int FailedFiles { get; init; } = 0;
    }

    /// <summary>
    /// Response model for report status/details by tracking id (API v2)
    /// Mirrors data shown in HazardReportSearchResult for external consumers.
    /// </summary>
    public record PDXSMSReportStatusApiResponseV2
    {
        public string TrackingId { get; init; } = string.Empty;
        public string ReportId { get; init; } = string.Empty;
        public string HazardId { get; init; } = string.Empty;
        public bool IsAnonymous { get; init; }

        public string SubmittedBy { get; init; } = string.Empty;
        public DateTime? SubmittedDate { get; init; }
        public string SubmittingDepartment { get; init; } = string.Empty;
        public string SubmittingDepartmentJobFunction { get; init; } = string.Empty;

        public string HazardCategory { get; init; } = string.Empty;
        public string HazardType { get; init; } = string.Empty;
        public string HazardDescription { get; init; } = string.Empty;
        public string ReportStatus { get; init; } = string.Empty;
        public string HazardStatus { get; init; } = string.Empty;

        public string ContactName { get; init; } = string.Empty;
        public string ContactCell { get; init; } = string.Empty;
        public string ContactEmail { get; init; } = string.Empty;

        public string ReportValidationCode { get; init; } = string.Empty;
        public string ReportValidationDecision { get; init; } = string.Empty;
        public string ReportValidationDecisionDisplay { get; init; } = string.Empty;
        public DateTime? ReportValidationDate { get; init; }

        public bool IsHazardCategoryValidated { get; init; }
        public bool IsHazardTypeValidated { get; init; }
        public bool IsHazardLocationValidated { get; init; }
        public string HazardLocationValidationText { get; init; } = string.Empty;

        public string RiskAssessmentCode { get; init; } = string.Empty;
        public string RiskAssessmentAssessmentType { get; init; } = string.Empty;
        public string RiskAssessmentStatus { get; init; } = string.Empty;
        public string RiskAssessmentStage { get; init; } = string.Empty;
        public int? RiskAssessmentCurrentStep { get; init; }
        public DateTime? RiskAssessmentUpdatedDate { get; init; }

        public string MitigationCode { get; init; } = string.Empty;
        public string MitigationStatus { get; init; } = string.Empty;
        public decimal? MitigationProgress { get; init; }
        public DateTime? MitigationUpdatedDate { get; init; }

        public string LocationCode { get; init; } = string.Empty;
        public decimal? LocationLatitude { get; init; }
        public decimal? LocationLongitude { get; init; }
        public string LocationDescription { get; init; } = string.Empty;
        public DateTime? LocationDateSelected { get; init; }

        public int SubmittedFileCount { get; init; }
    }

    public record PDXSMSAttachmentSummary
    {
        public string FileId { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public string FileType { get; init; } = string.Empty;
        public long FileSizeBytes { get; init; }
        public bool IsConfidential { get; init; }
        public DateTime? UploadedDate { get; init; }
    }

    /// <summary>
    /// File attachment model for API submissions
    /// </summary>
    public record FileAttachment
    {
        public string? FileUri { get; init; }

        [Required]
        public string FileName { get; init; } = string.Empty;
        
        public string? Base64Content { get; init; }
        
        public string? ContentType { get; init; }
    }

    /// <summary>
    /// API error response model for consistent error handling
    /// </summary>
    public record ApiErrorResponse
    {
        public string Error { get; init; } = string.Empty;
        public List<string> Details { get; init; } = new();
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
        public string RequestId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Reference data response models
    /// </summary>
    public record HazardCategoryResponse
    {
        public string Value { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int SortOrder { get; init; }
    }

    public record HazardTypeResponse
    {
        public string Value { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string GuidanceText { get; init; } = string.Empty;
        public bool RequiresRegulatoryReporting { get; init; }
    }

    public sealed class SingleOrArrayConverter<TItem> : JsonConverter<List<TItem>>
    {
        public override List<TItem> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                return JsonSerializer.Deserialize<List<TItem>>(ref reader, options) ?? new List<TItem>();
            }

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var item = JsonSerializer.Deserialize<TItem>(ref reader, options);
                return item is null ? new List<TItem>() : new List<TItem> { item };
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return new List<TItem>();
            }

            throw new JsonException($"Unexpected token {reader.TokenType} when parsing {typeof(TItem).Name} list.");
        }

        public override void Write(Utf8JsonWriter writer, List<TItem> value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
