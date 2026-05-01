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

namespace SMS3.Api.Models
{
    /// <summary>
    /// Request model for external confidential report submissions
    /// </summary>
    public record PDXSMSReportApiRequest
    {
        /// <summary>
        /// The category of the hazard (required). Use GET /api/pdxsms/hazard-categories to see all valid values.
        /// Examples: "INCIDENT", "FOD", "WILDLIFE", "OPERATIONAL_CHANGE"
        /// </summary>
        /// <example>INCIDENT</example>
        [Required]
        public string HazardCategory { get; init; } = string.Empty;

        /// <summary>
        /// Specific type of hazard being reported (required). Must match a valid value for the selected category.
        /// Use GET /api/pdxsms/hazard-types?category={categoryValue} to see valid types for a category.
        /// Examples: "AIRCRAFT_INCIDENT", "BIRD_STRIKE", "METAL_FOD"
        /// </summary>
        /// <example>AIRCRAFT_INCIDENT</example>
        [Required]
        public string HazardType { get; init; } = string.Empty;

        /// <summary>
        /// Detailed description of the hazard or incident (required, max 2000 characters)
        /// </summary>
        /// <example>Aircraft experienced engine failure during takeoff roll, aborting takeoff safely</example>
        [Required]
        [StringLength(2000)]
        public string Description { get; init; } = string.Empty;

        public string? SubmittedBy { get; init; }
        public DateTime? SubmittedDate { get; init; }
        public string? SubmittingDepartment { get; init; } = "EXTERNAL_SYSTEM";
        public string? SubmittingDepartmentJobFunction { get; init; } = "EXTERNAL_SYSTEM";
        public bool? IsAnonymous { get; init; }
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
        public decimal? Latitude { get; init; }

        /// <summary>
        /// Optional longitude coordinate for precise location (decimal degrees)
        /// </summary>
        /// <example>-122.5951</example>
        public decimal? Longitude { get; init; }

        /// <summary>
        /// Optional incident date/time (if not provided, current time is used)
        /// </summary>
        /// <example>2024-01-15T14:30:00Z</example>
        public DateTime? IncidentDateTime { get; init; }

        /// <summary>
        /// Optional file attachments as base64 encoded strings (max 10MB per file)
        /// </summary>
        public List<FileAttachment>? Attachments { get; init; }
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
        public DateTime SubmissionDateTime { get; init; }
        
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
        /// <example>https://your-sms-domain.com/ConfidentialReporting/TrackStatus/HT-2024-001234</example>
        public string TrackingUrl { get; init; } = string.Empty;
        
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
    /// File attachment model for API submissions
    /// </summary>
    public record FileAttachment
    {
        [Required]
        public string FileName { get; init; } = string.Empty;
        
        [Required]
        public string Base64Content { get; init; } = string.Empty;
        
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
}