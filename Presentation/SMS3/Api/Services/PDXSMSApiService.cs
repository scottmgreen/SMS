//-----------------------------------------------------------------------
// <copyright file="PDXSMSApiService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Implementation of PDXSMS external API business logic with comprehensive validation and security.
//                  Provides business logic implementation for external API operations
//                  following Clean Architecture with proper error handling and logging.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.IdentityModel.Tokens;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;

using SMS_Shared.Common;

using SMS3.Api.Models;
using SMS3.Api.Services;


namespace SMS3.Api.Services
{
    /// <summary>
    /// Implementation of PDXSMS external API business logic
    /// </summary>
    public class PDXSMSApiService : IPDXSMSApiService
    {
        private readonly IBaseMediator _mediator;
        private readonly ILogger<PDXSMSApiService> _logger;

        public PDXSMSApiService(IBaseMediator mediator, ILogger<PDXSMSApiService> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<PDXSMSReportApiResponse>> ProcessReportSubmissionAsync(
            PDXSMSReportApiRequestV1 request, 
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
                if (request.LocationLatitude.HasValue && request.LocationLongitude.HasValue)
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
                    request.ReportAttachments, createdHazard.Code, actualReportCode);

                // Step 7: Build success response
                var response = new PDXSMSReportApiResponse
                {
                    TrackingId = actualTrackingCode,
                    HazardId = createdHazard.Code,
                    ReportId = actualReportCode,
                    ReportSubmissionDateTime = DateTime.UtcNow,
                    Status = "Submitted",
                    Message = $"Confidential report submitted successfully. Tracking ID: {actualTrackingCode}",
                    TrackingUrl = $"https://{httpContext.Request.Host}/ExternalReporting/TrackStatus/{actualTrackingCode}",
                    ProcessedFiles = processedFiles,
                    FailedFiles = failedFiles
                };

                _logger.LogInformation("?? PDX SMS API report submitted successfully. " +
                    "TrackingId: {TrackingId}, ReportId: {ReportId}, HazardId: {HazardId}, Files: {ProcessedFiles}/{TotalFiles}", 
                    actualTrackingCode, actualReportCode, createdHazard.Code, processedFiles, 
                    request.ReportAttachments?.Count ?? 0);

                return Result<PDXSMSReportApiResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "?? Unexpected error processing external confidential report submission");
                return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(
                    new Error("PDXSMS.ProcessingFailed", "An unexpected error occurred while processing your confidential report."));
            }
        }

        public async Task<Result<PDXSMSReportApiResponse>> ProcessReportSubmissionAsyncV2(
    PDXSMSReportApiRequestV2 request,
    HttpContext httpContext)
        {
            try
            {
                // Step 1: Validate request (reuse logic, but only for v2 fields)
                var validationErrors = new List<string>();
                if (string.IsNullOrEmpty(request.HazardDescription))
                    validationErrors.Add("description is required");

                if (string.IsNullOrEmpty(request.LocationDescription))
                    validationErrors.Add("location is required");

                if (request.HazardDescription?.Length > 10000)
                    validationErrors.Add("description cannot exceed 10000 characters");

                if (validationErrors.Any())
                {
                    var errorMessage = $"Validation failed: {string.Join(", ", validationErrors)}";
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(
                        new Error("PDXSMS.ValidationFailed", errorMessage));
                }

                // Step 2: Create parent report (no hazard category/type)
                var report = new Report(new ReportID("RP-0000"))
                {
                    Code = "RP-0000",
                    Name = $"EXTERNAL/V2",
                    Description = request.HazardDescription,
                    SubmittedBy = "EXTERNAL_SYSTEM",
                    SubmittedDate = DateTime.UtcNow,
                    SubmittingDepartment = request.ReportSubmittingDepartment ?? string.Empty,
                    SubmittingDepartmentJobFunction = request.ReportSubmittingDepartmentJobFunction ?? string.Empty,
                    IncidentDateTime = request.HazardIncidentDateTime ?? DateTime.UtcNow,
                    IsAnonymous = request.ReportIsAnonymous ?? true,
                    ReportContactName = request.ReportContactName ?? string.Empty,
                    ReportContactCell = request.ReportContactCell ?? string.Empty,
                    ReportContactEmail = request.ReportContactEmail ?? string.Empty,
                    Stage = "INITIAL",
                    Status = ReportStatus.NeedsValidation,
                    CreatedBy = "EXTERNAL_SYSTEM",
                    CreatedDate = DateTime.UtcNow
                };
                var reportResult = await _mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);
                if (reportResult.IsFailure)
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(reportResult.Error);

                var actualReportCode = reportResult.Value.Code;
                _logger.LogInformation("V2 External report created: {ReportCode}", actualReportCode);

                // Step 3: Create associated hazard (no category/type)
                var hazard = new Hazard(new HazardID("HZ-0000"))
                {
                    Code = "HZ-0000",
                    Name = $"EXTERNAL/V2",
                    Description = request.HazardDescription,
                    HazardCategory = HazardCategory.Default.Value,
                    HazardType = HazardType.Default.Value,
                    ReportCode = actualReportCode,
                    IsInitialHazard = true,
                    Status = HazardStatus.InitialRiskAssessment,
                    CreatedBy = "EXTERNAL_SYSTEM",
                    CreatedDate = DateTime.UtcNow
                };
                var hazardResult = await _mediator.SendAsync(new CreateHazardCommand(hazard), CancellationToken.None);
                if (hazardResult.IsFailure)
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(hazardResult.Error);

                var createdHazard = hazardResult.Value;
                _logger.LogInformation("V2 External hazard created: {HazardCode} for Report: {ReportCode}",
                    createdHazard.Code, actualReportCode);

                // Step 4: Create hazard location if coordinates provided
                if (request.LocationLatitude.HasValue && request.LocationLongitude.HasValue)
                {
                    var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"))
                    {
                        HazardCode = createdHazard.Code,
                        Latitude = request.LocationLatitude,
                        Longitude = request.LocationLongitude,
                        Description = request.LocationDescription
                    };
                    await _mediator.SendAsync(new CreateHazardLocationCommand(hazardLocation), CancellationToken.None);
                }

                // Step 5: Create tracking record
                var trackingResult = await CreateTrackingAsync(createdHazard);
                if (trackingResult.IsFailure)
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(trackingResult.Error);

                var actualTrackingCode = trackingResult.Value.TrackingCode;
                _logger.LogInformation("V2 External tracking created: {TrackingCode}", actualTrackingCode);

                // Step 6: Process file attachments
                var (processedFiles, failedFiles) = await ProcessAttachmentsAsync(
                    request.ReportAttachments, createdHazard.Code, actualReportCode);

                // Step 7: Build success response
                var response = new PDXSMSReportApiResponse
                {
                    TrackingId = actualTrackingCode,
                    HazardId = createdHazard.Code,
                    ReportId = actualReportCode,
                    ReportSubmissionDateTime = DateTime.UtcNow,
                    Status = "Submitted",
                    Message = $"Confidential report submitted successfully. Tracking ID: {actualTrackingCode}",
                    TrackingUrl = $"https://{httpContext.Request.Host}/ExternalReporting/TrackStatus/{actualTrackingCode}",
                    ProcessedFiles = processedFiles,
                    FailedFiles = failedFiles
                };

                _logger.LogInformation("PDX SMS API v2 report submitted successfully. " +
                    "TrackingId: {TrackingId}, ReportId: {ReportId}, HazardId: {HazardId}, Files: {ProcessedFiles}/{TotalFiles}",
                    actualTrackingCode, actualReportCode, createdHazard.Code, processedFiles,
                    request.ReportAttachments?.Count ?? 0);

                return Result<PDXSMSReportApiResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing external confidential report submission (v2)");
                return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(
                    new Error("PDXSMS.ProcessingFailed", "An unexpected error occurred while processing your confidential report."));
            }
        }

        public async Task<Result<bool>> ValidateRequestAsync(PDXSMSReportApiRequestV1 request)
        {
            var validationErrors = new List<string>();
            if (string.IsNullOrEmpty(request.HazardCategory))
                validationErrors.Add("hazardCategory is required");

            if (string.IsNullOrEmpty(request.HazardType))
                validationErrors.Add("hazardType is required");
            
            if (string.IsNullOrEmpty(request.HazardDescription))
                validationErrors.Add("description is required");

            // Check if latitude is provided and valid
            if (!request.LocationLatitude.HasValue)
            {
                validationErrors.Add("LocationLatitude is required");
            }
            else if (request.LocationLatitude.Value < -90 || request.LocationLatitude.Value > 90)
            {
                validationErrors.Add("LocationLatitude must be between -90 and 90 degrees");
            }

            // Check if longitude is provided and valid  
            if (!request.LocationLongitude.HasValue)
            {
                validationErrors.Add("LocationLongitude is required");
            }
            else if (request.LocationLongitude.Value < -180 || request.LocationLongitude.Value > 180)
            {
                validationErrors.Add("LocationLongitude must be between -180 and 180 degrees");
            }

            // Optional: Check for obviously invalid coordinates (like 0,0 if that's not valid for your use case)
            if (request.LocationLatitude.HasValue && request.LocationLongitude.HasValue &&
                request.LocationLatitude.Value == 0 && request.LocationLongitude.Value == 0)
            {
                validationErrors.Add("Location coordinates (0, 0) are not valid for this application");
            }

            if (string.IsNullOrEmpty(request.LocationDescription))
                validationErrors.Add("location is required");

            if (request.HazardDescription?.Length > 2000)
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

        private async Task<Result<Report>> CreateReportAsync(PDXSMSReportApiRequestV1 request)
        {
            var report = new Report(new ReportID("RP-0000"))
            {
                Code = "RP-0000", // Database will generate actual code
                Name = $"{request.HazardCategory}/{request.HazardType}",
                Description = request.HazardDescription,
                SubmittedBy = "EXTERNAL_SYSTEM",
                SubmittedDate = DateTime.UtcNow,
                SubmittingDepartment = request.ReportSubmittingDepartment ?? string.Empty,
                SubmittingDepartmentJobFunction = request.ReportSubmittingDepartmentJobFunction ?? string.Empty,
                IncidentDateTime = request.HazardIncidentDateTime ?? DateTime.UtcNow,
                IsAnonymous = request.ReportIsAnonymous ?? true,
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

        private async Task<Result<Hazard>> CreateHazardAsync(PDXSMSReportApiRequestV1 request, string reportCode)
        {
            var hazard = new Hazard(new HazardID("HZ-0000"))
            {
                Code = "HZ-0000", // Database will generate actual code
                Name = $"{request.HazardCategory}/{request.HazardType}",
                Description = request.HazardDescription,
                HazardCategory = request.HazardCategory ?? HazardCategory.Default.Value,
                HazardType = request.HazardType ?? HazardType.Default.Value,
                ReportCode = reportCode,
                IsInitialHazard = true,
                Status = HazardStatus.InitialRiskAssessment,
                CreatedBy = "EXTERNAL_SYSTEM",
                CreatedDate = DateTime.UtcNow
            };

            return await _mediator.SendAsync(new CreateHazardCommand(hazard), CancellationToken.None);
        }

        private async Task<Result<HazardLocation>> CreateHazardLocationAsync(PDXSMSReportApiRequestV1 request, string hazardCode)
        {
            // TODO: Add check to prevent duplicate location creation
            // Check if hazard location already exists for this hazard code

            var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"))
            {
                HazardCode = hazardCode,
                Latitude = request.LocationLatitude,
                Longitude = request.LocationLongitude,
                Description = request.LocationDescription
            };

            _logger.LogInformation("??? Creating hazard location for {HazardCode}: Lat={Latitude}, Lon={Longitude}, Desc='{Description}'", 
                hazardCode, request.LocationLatitude, request.LocationLongitude, request.LocationDescription);

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