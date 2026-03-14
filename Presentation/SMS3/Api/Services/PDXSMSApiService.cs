//-----------------------------------------------------------------------
// <copyright file="PDXSMSApiService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Implementation of PDXSMS external API business logic with comprehensive validation and security.
//                  Provides business logic implementation for external API operations
//                  following Clean Architecture with proper error handling and logging.
// </copyright>
//-----------------------------------------------------------------------

using SMS3.Api.Models;
using SMS3.Api.Services;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS3.Api.Services
{
    /// <summary>
    /// Implementation of PDXSMS external API business logic
    /// </summary>
    public class PDXSMSApiService : IPDXSMSApiService
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PDXSMSApiService> _logger;

        public PDXSMSApiService(IMediator mediator, ILogger<PDXSMSApiService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<PDXSMSReportApiResponse>> ProcessReportSubmissionAsync(
            PDXSMSReportApiRequest request, 
            HttpContext httpContext)
        {
            try
            {
                // Step 1: Validate request
                var validationResult = await ValidateRequestAsync(request);
                if (validationResult.IsFailure)
                {
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(validationResult.Error);
                }

                // Step 2: Create parent report
                var reportResult = await CreateReportAsync(request);
                if (reportResult.IsFailure)
                {
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(reportResult.Error);
                }

                var actualReportCode = reportResult.Value.Code;
                _logger.LogInformation("?? External report created: {ReportCode}", actualReportCode);

                // Step 3: Create associated hazard
                var hazardResult = await CreateHazardAsync(request, actualReportCode);
                if (hazardResult.IsFailure)
                {
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(hazardResult.Error);
                }

                var createdHazard = hazardResult.Value;
                _logger.LogInformation("?? External hazard created: {HazardCode} for Report: {ReportCode}", 
                    createdHazard.Code, actualReportCode);

                // Step 4: Create hazard location if coordinates provided
                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    await CreateHazardLocationAsync(request, createdHazard.Code);
                }

                // Step 5: Create tracking record
                var trackingResult = await CreateTrackingAsync(createdHazard);
                if (trackingResult.IsFailure)
                {
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(trackingResult.Error);
                }

                var actualTrackingCode = trackingResult.Value.TrackingCode;
                _logger.LogInformation("?? External tracking created: {TrackingCode}", actualTrackingCode);

                // Step 6: Process file attachments
                var (processedFiles, failedFiles) = await ProcessAttachmentsAsync(
                    request.Attachments, createdHazard.Code, actualReportCode);

                // Step 7: Build success response
                var response = new PDXSMSReportApiResponse
                {
                    TrackingId = actualTrackingCode,
                    HazardId = createdHazard.Code,
                    ReportId = actualReportCode,
                    SubmissionDateTime = DateTime.UtcNow,
                    Status = "Submitted",
                    Message = $"Confidential report submitted successfully. Tracking ID: {actualTrackingCode}",
                    TrackingUrl = $"https://{httpContext.Request.Host}/ConfidentialReporting/TrackStatus/{actualTrackingCode}",
                    ProcessedFiles = processedFiles,
                    FailedFiles = failedFiles
                };

                _logger.LogInformation("?? PDX SMS API report submitted successfully. " +
                    "TrackingId: {TrackingId}, ReportId: {ReportId}, HazardId: {HazardId}, Files: {ProcessedFiles}/{TotalFiles}", 
                    actualTrackingCode, actualReportCode, createdHazard.Code, processedFiles, 
                    request.Attachments?.Count ?? 0);

                return Result<PDXSMSReportApiResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Unexpected error processing external confidential report submission");
                return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(
                    new Error("PDXSMS.ProcessingFailed", "An unexpected error occurred while processing your confidential report."));
            }
        }

        public async Task<Result<bool>> ValidateRequestAsync(PDXSMSReportApiRequest request)
        {
            var validationErrors = new List<string>();
            
            if (string.IsNullOrEmpty(request.HazardType))
                validationErrors.Add("hazardType is required");
            
            if (string.IsNullOrEmpty(request.Description))
                validationErrors.Add("description is required");
            
            if (string.IsNullOrEmpty(request.LocationDescription))
                validationErrors.Add("location is required");

            if (request.Description?.Length > 2000)
                validationErrors.Add("description cannot exceed 2000 characters");

            if (validationErrors.Any())
            {
                var errorMessage = $"Validation failed: {string.Join(", ", validationErrors)}";
                return Result<bool>.Failure<bool>(new Error("PDXSMS.ValidationFailed", errorMessage));
            }

            return Result<bool>.Success(true);
        }

        public async Task<(int ProcessedFiles, int FailedFiles)> ProcessAttachmentsAsync(
            List<FileAttachment>? attachments, 
            string hazardCode, 
            string reportCode)
        {
            var processedFiles = 0;
            var failedFiles = 0;

            if (attachments?.Any() != true)
                return (processedFiles, failedFiles);

            _logger.LogInformation("Processing {Count} attachments for external hazard {HazardCode}", 
                attachments.Count, hazardCode);

            foreach (var attachment in attachments)
            {
                try
                {
                    // Validate attachment
                    if (string.IsNullOrEmpty(attachment.Base64Content))
                    {
                        _logger.LogWarning("Skipping empty attachment: {FileName}", attachment.FileName);
                        failedFiles++;
                        continue;
                    }

                    // Convert and validate base64
                    byte[] fileData;
                    try
                    {
                        fileData = Convert.FromBase64String(attachment.Base64Content);
                    }
                    catch (FormatException)
                    {
                        _logger.LogWarning("Invalid base64 content for attachment: {FileName}", attachment.FileName);
                        failedFiles++;
                        continue;
                    }

                    // Check file size (10MB limit)
                    if (fileData.Length > 10 * 1024 * 1024)
                    {
                        _logger.LogWarning("File too large (>10MB): {FileName}", attachment.FileName);
                        failedFiles++;
                        continue;
                    }

                    // Create hazard file entity
                    var hazardFile = new HazardFile(new HazardFileID("HF-0000"))
                    {
                        Code = "HF-0000", // Database will generate actual code
                        HazardCode = hazardCode,
                        ReportCode = reportCode,
                        FileName = attachment.FileName,
                        FileType = Path.GetExtension(attachment.FileName)?.TrimStart('.') ?? "unknown",
                        ContentType = attachment.ContentType ?? "application/octet-stream",
                        FileSizeBytes = fileData.Length,
                        StorageType = "Database",
                        FileData = fileData,
                        UploadedBy = "EXTERNAL_SYSTEM",
                        UploadedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsConfidential = true,
                        CreatedBy = "EXTERNAL_SYSTEM",
                        CreatedDate = DateTime.UtcNow
                    };

                    var createFileCommand = new CreateHazardFileCommand(hazardFile);
                    var fileResult = await _mediator.SendAsync(createFileCommand, CancellationToken.None);

                    if (fileResult.IsSuccess)
                    {
                        processedFiles++;
                        _logger.LogInformation("? Processed attachment: {FileName} ({Size} bytes)", 
                            attachment.FileName, fileData.Length);
                    }
                    else
                    {
                        failedFiles++;
                        _logger.LogWarning("? Failed to save attachment: {FileName}. Error: {Error}", 
                            attachment.FileName, fileResult.Error?.Message);
                    }
                }
                catch (Exception fileEx)
                {
                    failedFiles++;
                    _logger.LogWarning(fileEx, "? Exception processing attachment: {FileName}", attachment.FileName);
                }
            }

            _logger.LogInformation("?? File processing completed: {Processed} successful, {Failed} failed", 
                processedFiles, failedFiles);

            return (processedFiles, failedFiles);
        }

        #region Private Helper Methods

        private async Task<Result<Report>> CreateReportAsync(PDXSMSReportApiRequest request)
        {
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000", // Database will generate actual code
                Name = $"External - {request.HazardCategory} - {request.HazardType}",
                Description = request.Description,
                SubmittedBy = "EXTERNAL_SYSTEM",
                SubmittedDate = DateTime.UtcNow,
                SubmittingDepartment = request.SubmittingDepartment ?? "EXTERNAL_API",
                SubmittingDepartmentJobFunction = request.SubmittingDepartmentJobFunction ?? "API_SUBMISSION",
                IncidentDateTime = request.IncidentDateTime ?? DateTime.UtcNow,
                IsAnonymous = request.IsAnonymous ?? true,
                ReportContactName = request.ReportContactName ?? string.Empty,
                ReportContactCell = request.ReportContactCell ?? string.Empty,
                ReportContactEmail = request.ReportContactEmail ?? string.Empty,
                Stage = "INITIAL",
                Status = ReportStatus.NeedsValidation,
                CreatedBy = "EXTERNAL_SYSTEM",
                CreatedDate = DateTime.UtcNow
            };

            return await _mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);
        }

        private async Task<Result<Hazard>> CreateHazardAsync(PDXSMSReportApiRequest request, string reportCode)
        {
            var hazard = new Hazard(new HazardID("HZ-0000"))
            {
                Code = "HZ-0000", // Database will generate actual code
                Name = $"External - {request.HazardCategory} - {request.HazardType}",
                Description = request.Description,
                HazardCategory = request.HazardCategory ?? "EXTERNAL",
                HazardType = request.HazardType,
                ReportCode = reportCode,
                IsInitialHazard = true,
                Status = HazardStatus.InitialRiskAssessment,
                CreatedBy = "EXTERNAL_SYSTEM",
                CreatedDate = DateTime.UtcNow
            };

            return await _mediator.SendAsync(new CreateHazardCommand(hazard), CancellationToken.None);
        }

        private async Task<Result<HazardLocation>> CreateHazardLocationAsync(PDXSMSReportApiRequest request, string hazardCode)
        {
            var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"))
            {
                HazardCode = hazardCode,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Description = request.LocationDescription
            };

            return await _mediator.SendAsync(new CreateHazardLocationCommand(hazardLocation), CancellationToken.None);
        }

        private async Task<Result<HazardReportTracking>> CreateTrackingAsync(Hazard hazard)
        {
            var hazardReportTracking = new HazardReportTracking(new HazardReportTrackingID("HT-TEMP"))
            {
                HazardCode = hazard.Code,
                ReportCode = hazard.ReportCode,
                TrackingCode = "HT-0000" // Database will generate actual tracking code
            };

            return await _mediator.SendAsync(new CreateHazardReportTrackingCommand(hazardReportTracking), CancellationToken.None);
        }

        #endregion
    }
}