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
using SMS_Application.Commands;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Application.Queries;

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

        public async Task<Result<PDXSMSReportStatusApiResponseV2>> GetReportStatusByTrackingIdAsync(string trackingId, HttpContext httpContext)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(trackingId))
                {
                    return Result<PDXSMSReportStatusApiResponseV2>.Failure<PDXSMSReportStatusApiResponseV2>(
                        new Error("PDXSMS.ValidationFailed", "trackingId is required"));
                }

                var trackingQuery = new GetHazardReportTrackingByTrackingCodeQuery(trackingId.Trim());
                var trackingResult = await _mediator.SendAsync(trackingQuery, CancellationToken.None);
                if (trackingResult.IsFailure || trackingResult.Value is null)
                {
                    return Result<PDXSMSReportStatusApiResponseV2>.Failure<PDXSMSReportStatusApiResponseV2>(
                        new Error("PDXSMS.NotFound", $"No report found for trackingId: {trackingId}"));
                }

                var tracking = trackingResult.Value;

                var hazardResult = await _mediator.SendAsync(new GetHazardByCodeQuery(new HazardID(tracking.HazardCode)), CancellationToken.None);
                var reportResult = await _mediator.SendAsync(new GetReportByCodeQuery(new ReportID(tracking.ReportCode)), CancellationToken.None);

                var reportValidationResult = await _mediator.SendAsync(new GetReportValidationByReportIdQuery(new ReportID(tracking.ReportCode)), CancellationToken.None);
                var hazardLocationsResult = await _mediator.SendAsync(new GetHazardLocationsByHazardCodeQuery(tracking.HazardCode), CancellationToken.None);
                var filesResult = await _mediator.SendAsync(new GetHazardFilesByHazardCodeQuery(tracking.HazardCode), CancellationToken.None);
                var riskAssessmentsResult = await _mediator.SendAsync(new GetRiskAssessmentsByHazardCodeQuery(new HazardID(tracking.HazardCode)), CancellationToken.None);
                var mitigationsResult = await _mediator.SendAsync(new GetMitigationsByHazardCodeQuery(tracking.HazardCode), CancellationToken.None);

                var hazard = hazardResult.IsSuccess ? hazardResult.Value : null;
                var report = reportResult.IsSuccess ? reportResult.Value : null;
                var reportValidation = reportValidationResult.IsSuccess ? reportValidationResult.Value : null;

                var latestLocation = hazardLocationsResult.IsSuccess && hazardLocationsResult.Value?.Any() == true
                    ? hazardLocationsResult.Value
                        .OrderByDescending(l => l.UpdatedDate ?? DateTime.MinValue)
                        .ThenByDescending(l => l.DateSelected)
                        .ThenByDescending(l => l.CreatedDate ?? DateTime.MinValue)
                        .FirstOrDefault()
                    : null;

                var assessments = riskAssessmentsResult.IsSuccess && riskAssessmentsResult.Value is not null
                    ? riskAssessmentsResult.Value.ToList()
                    : new List<RiskAssessment>();

                var currentRiskAssessment = assessments.FirstOrDefault(ra => ra.RiskAssessmentCategory == RiskAssessmentCategory.Technical)
                                          ?? assessments.FirstOrDefault();

                var mitigations = mitigationsResult.IsSuccess && mitigationsResult.Value is not null
                    ? mitigationsResult.Value.ToList()
                    : new List<Mitigation>();

                var currentMitigation = mitigations.FirstOrDefault();

                var hazardCategoryValue = hazard?.HazardCategory ?? string.Empty;
                var hazardTypeValue = hazard?.HazardType ?? string.Empty;
                var isHazardCategoryValidated = !string.IsNullOrWhiteSpace(hazardCategoryValue) && !string.Equals(hazardCategoryValue, HazardCategory.Default.Value, StringComparison.OrdinalIgnoreCase);
                var isHazardTypeValidated = !string.IsNullOrWhiteSpace(hazardTypeValue) && !string.Equals(hazardTypeValue, HazardType.Default.Value, StringComparison.OrdinalIgnoreCase);
                var isHazardLocationValidated = latestLocation?.IsValidated == true;

                var validationDecisionDisplay = string.Empty;
                if (!string.IsNullOrWhiteSpace(reportValidation?.ValidationDecision) && ValidationDecision.TryFromValue(reportValidation.ValidationDecision, out var validationDecision))
                {
                    validationDecisionDisplay = validationDecision?.Name ?? string.Empty;
                }

                var response = new PDXSMSReportStatusApiResponseV2
                {
                    TrackingId = tracking.TrackingCode,
                    ReportId = tracking.ReportCode,
                    HazardId = tracking.HazardCode,
                    IsAnonymous = report?.IsAnonymous ?? true,
                    SubmittedBy = report?.SubmittedBy ?? string.Empty,
                    SubmittedDate = report?.SubmittedDate,
                    SubmittingDepartment = report?.SubmittingDepartment ?? string.Empty,
                    SubmittingDepartmentJobFunction = report?.SubmittingDepartmentJobFunction ?? string.Empty,
                    HazardCategory = hazardCategoryValue,
                    HazardType = hazardTypeValue,
                    HazardDescription = hazard?.Description ?? report?.Description ?? string.Empty,
                    ReportStatus = report?.Status ?? string.Empty,
                    HazardStatus = hazard?.Status ?? report?.Status ?? string.Empty,
                    ContactName = report?.ReportContactName ?? string.Empty,
                    ContactCell = report?.ReportContactCell ?? string.Empty,
                    ContactEmail = report?.ReportContactEmail ?? string.Empty,
                    ReportValidationCode = reportValidation?.Code ?? string.Empty,
                    ReportValidationDecision = reportValidation?.ValidationDecision ?? string.Empty,
                    ReportValidationDecisionDisplay = validationDecisionDisplay,
                    ReportValidationDate = reportValidation?.CreatedDate,
                    IsHazardCategoryValidated = isHazardCategoryValidated,
                    IsHazardTypeValidated = isHazardTypeValidated,
                    IsHazardLocationValidated = isHazardLocationValidated,
                    HazardLocationValidationText = latestLocation is null ? "No mapped location" : (latestLocation.IsValidated ? "Validated" : "Validation Required"),
                    RiskAssessmentCode = currentRiskAssessment?.Code ?? string.Empty,
                    RiskAssessmentAssessmentType = currentRiskAssessment?.AssessmentType ?? string.Empty,
                    RiskAssessmentStatus = currentRiskAssessment?.Status ?? string.Empty,
                    RiskAssessmentStage = currentRiskAssessment?.Stage ?? string.Empty,
                    RiskAssessmentCurrentStep = currentRiskAssessment?.CurrentStep,
                    RiskAssessmentUpdatedDate = currentRiskAssessment?.UpdatedDate,
                    MitigationCode = currentMitigation?.Code ?? string.Empty,
                    MitigationStatus = currentMitigation?.Status ?? string.Empty,
                    MitigationProgress = currentMitigation?.Progress,
                    MitigationUpdatedDate = currentMitigation?.UpdatedDate,
                    LocationCode = latestLocation?.Code ?? string.Empty,
                    LocationLatitude = latestLocation?.Latitude,
                    LocationLongitude = latestLocation?.Longitude,
                    LocationDescription = latestLocation?.Description ?? hazard?.LocationSubArea ?? hazard?.LocationArea ?? string.Empty,
                    LocationDateSelected = latestLocation?.DateSelected,
                    SubmittedFileCount = filesResult.IsSuccess && filesResult.Value is not null
                        ? filesResult.Value.Count()
                        : 0
                };

                return Result<PDXSMSReportStatusApiResponseV2>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving report status by tracking id: {TrackingId}", trackingId);
                return Result<PDXSMSReportStatusApiResponseV2>.Failure<PDXSMSReportStatusApiResponseV2>(
                    new Error("PDXSMS.ProcessingFailed", "An unexpected error occurred while retrieving report status."));
            }
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
                _logger.LogInformation("External report created: {ReportCode}", actualReportCode);

                // Step 3: Create associated hazard
                var hazardResult = await CreateHazardAsync(request, actualReportCode);
                if (hazardResult.IsFailure)
                {
                    return Result<PDXSMSReportApiResponse>.Failure<PDXSMSReportApiResponse>(hazardResult.Error);
                }

                var createdHazard = hazardResult.Value;
                _logger.LogInformation("External hazard created: {HazardCode} for Report: {ReportCode}", 
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
                _logger.LogInformation("External tracking created: {TrackingCode}", actualTrackingCode);

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
                    ProcessedFiles = processedFiles,
                    FailedFiles = failedFiles
                };

                _logger.LogInformation("PDX SMS API report submitted successfully. " +
                    "TrackingId: {TrackingId}, ReportId: {ReportId}, HazardId: {HazardId}, Files: {ProcessedFiles}/{TotalFiles}", 
                    actualTrackingCode, actualReportCode, createdHazard.Code, processedFiles, 
                    request.ReportAttachments?.Count ?? 0);

                return Result<PDXSMSReportApiResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing external confidential report submission");
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
                    Name = $"EXTERNAL_API_SOURCE",
                    Description = request.HazardDescription,
                    SubmittedBy = "EXTERNAL_API_SOURCE",
                    SubmittedDate = request.ReportSubmittedDate ?? DateTime.UtcNow,
                    SubmittingDepartment = request.ReportSubmittingDepartment ?? string.Empty,
                    SubmittingDepartmentJobFunction = request.ReportSubmittingDepartmentJobFunction ?? string.Empty,
                    IncidentDateTime = request.HazardIncidentDateTime ?? DateTime.UtcNow,
                    IsAnonymous = request.ReportIsAnonymous.GetValueOrDefault(true),
                    ReportContactName = request.ReportContactName ?? string.Empty,
                    ReportContactCell = request.ReportContactCell ?? string.Empty,
                    ReportContactEmail = request.ReportContactEmail ?? string.Empty,
                    Stage = "INITIAL",
                    Status = ReportStatus.NeedsValidation,
                    CreatedBy = "EXTERNAL_API_SOURCE",
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
                    Name = $"EXTERNAL_API_SOURCE",
                    HazardTitle = string.Empty,
                    Description = request.HazardDescription ?? string.Empty,
                    HazardCategory = HazardCategory.Default.Value,
                    HazardType = HazardType.Default.Value,
                    ReportCode = actualReportCode,
                    IsInitialHazard = true,
                    Status = HazardStatus.InitialRiskAssessment,
                    CreatedBy = "EXTERNAL_API_SOURCE",
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
                        Latitude = request.LocationLatitude ?? 0,
                        Longitude = request.LocationLongitude ?? 0,
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

            _logger.LogInformation("Processing {Count} attachments for external hazard {HazardCode}", attachments.Count, hazardCode);

            foreach (var attachment in attachments)
            {
                try
                {
                    var hasBase64Content = !string.IsNullOrWhiteSpace(attachment.Base64Content);
                    var hasFileUri = !string.IsNullOrWhiteSpace(attachment.FileUri);

                    // Must provide either uploaded bytes (base64) or a cloud/file URI
                    if (!hasBase64Content && !hasFileUri)
                    {
                        _logger.LogWarning("Skipping attachment with no Base64Content or FileUri: {FileName}", attachment.FileName);
                        failedFiles++;
                        continue;
                    }

                    byte[]? fileData = null;
                    string storageType;
                    string? filePath = null;
                    long fileSizeBytes = 0;

                    // If bytes are present, treat as direct upload fallback and persist in Database storage.
                    if (hasBase64Content)
                    {
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

                        storageType = "Database";
                        fileSizeBytes = fileData.Length;
                    }
                    else
                    {
                        // URI-only mode (e.g., Azure blob upload succeeded prior to API submit)
                        if (!Uri.TryCreate(attachment.FileUri, UriKind.Absolute, out _))
                        {
                            _logger.LogWarning("Invalid FileUri for attachment: {FileName} - {FileUri}", attachment.FileName, attachment.FileUri);
                            failedFiles++;
                            continue;
                        }

                        storageType = "Cloud";
                        filePath = attachment.FileUri;
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
                        FileSizeBytes = fileSizeBytes,
                        StorageType = storageType,
                        FilePath = filePath,
                        FileData = fileData,
                        UploadedBy = "EXTERNAL_API_SOURCE",
                        UploadedDate = DateTime.UtcNow,
                        IsActive = true,
                        IsConfidential = true,
                        CreatedBy = "EXTERNAL_API_SOURCE",
                        CreatedDate = DateTime.UtcNow
                    };

                    var createFileCommand = new CreateHazardFileCommand(hazardFile);
                    var fileResult = await _mediator.SendAsync(createFileCommand, CancellationToken.None);

                    if (fileResult.IsSuccess)
                    {
                        processedFiles++;
                        _logger.LogInformation("Processed attachment: {FileName} (StorageType={StorageType}, Size={Size} bytes)",
                            attachment.FileName, storageType, fileSizeBytes);
                    }
                    else
                    {
                        failedFiles++;
                        _logger.LogWarning("Failed to save attachment: {FileName}. Error: {Error}", 
                            attachment.FileName, fileResult.Error?.Message);
                    }
                }
                catch (Exception fileEx)
                {
                    failedFiles++;
                    _logger.LogWarning(fileEx, "Exception processing attachment: {FileName}", attachment.FileName);
                }
            }

            _logger.LogInformation("File processing completed: {Processed} successful, {Failed} failed", 
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
                SubmittedBy = "EXTERNAL_API_SOURCE",
                SubmittedDate = request.ReportSubmittedDate.Value,
                SubmittingDepartment = request.ReportSubmittingDepartment ?? string.Empty,
                SubmittingDepartmentJobFunction = request.ReportSubmittingDepartmentJobFunction ?? string.Empty,
                IncidentDateTime = request.HazardIncidentDateTime ?? DateTime.UtcNow,
                IsAnonymous = request.ReportIsAnonymous.GetValueOrDefault(true),
                ReportContactName = request.ReportContactName ?? string.Empty,
                ReportContactCell = request.ReportContactCell ?? string.Empty,
                ReportContactEmail = request.ReportContactEmail ?? string.Empty,
                Stage = "INITIAL",
                Status = ReportStatus.NeedsValidation,
                CreatedBy = "EXTERNAL_API_SOURCE",
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
                CreatedBy = "EXTERNAL_API_SOURCE",
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
                Latitude = request.LocationLatitude ?? 0,
                Longitude = request.LocationLongitude ?? 0,
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
