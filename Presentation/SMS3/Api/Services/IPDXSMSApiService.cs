//-----------------------------------------------------------------------
// <copyright file="IPDXSMSApiService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service interface for PDXSMS external API business logic operations.
//                  Provides abstraction for external API operations following
//                  Clean Architecture principles with proper separation of concerns.
// </copyright>
//-----------------------------------------------------------------------

using SMS3.Api.Models;
using SMS_Shared.Common;

namespace SMS3.Api.Services
{
    /// <summary>
    /// Service interface for PDXSMS external API operations
    /// </summary>
    public interface IPDXSMSApiService
    {
        /// <summary>
        /// Process an external report submission with full business logic
        /// </summary>
        /// <param name="request">The API request with report data</param>
        /// <param name="httpContext">HTTP context for additional processing info</param>
        /// <returns>Result containing the API response or error</returns>
        Task<Result<PDXSMSReportApiResponse>> ProcessReportSubmissionAsync(PDXSMSReportApiRequestV1 request, HttpContext httpContext);

        /// <summary>
        /// Validate an API request for business rule compliance
        /// </summary>
        /// <param name="request">The request to validate</param>
        /// <returns>Validation result with error details if validation fails</returns>
        Task<Result<bool>> ValidateRequestAsync(PDXSMSReportApiRequestV1 request);

        /// <summary>
        /// Process file attachments with security validation
        /// </summary>
        /// <param name="attachments">List of file attachments</param>
        /// <param name="hazardCode">Associated hazard code</param>
        /// <param name="reportCode">Associated report code</param>
        /// <returns>Processing statistics</returns>
        Task<(int ProcessedFiles, int FailedFiles)> ProcessAttachmentsAsync(
            List<FileAttachment>? attachments, 
            string hazardCode, 
            string reportCode);

        /// <summary>
        /// Process an external report submission with full business logic (v2)
        /// </summary>
        /// <param name="request">The API request with report data for v2</param>
        /// <param name="httpContext">HTTP context for additional processing info</param>
        /// <returns>Result containing the API response or error</returns>
        Task<Result<PDXSMSReportApiResponse>> ProcessReportSubmissionAsyncV2(PDXSMSReportApiRequestV2 request, HttpContext httpContext);

        /// <summary>
        /// Get full report status/details by tracking ID (v2)
        /// </summary>
        /// <param name="trackingId">Tracking ID code</param>
        /// <param name="httpContext">HTTP context</param>
        /// <returns>Comprehensive report status/details response</returns>
        Task<Result<PDXSMSReportStatusApiResponseV2>> GetReportStatusByTrackingIdAsync(string trackingId, HttpContext httpContext);
    }
}