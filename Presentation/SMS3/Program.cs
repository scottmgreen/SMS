using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using SMS_Application.Configuration;
using SMS_Application.Messaging.Commands;

using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Security;

using SMS_Shared.Configuration;

using SMS3.Components;
using SMS3.Configuration;
using SMS3.Components.Shared.UIHelpers;

namespace SMS3;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();
        
        // Add HTTP context accessor for static notification access
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddScoped<ApiKeyAuthenticationFilter>();

        // ? CENTRALIZED AUTHENTICATION - All authentication services from Application layer
        // No more Presentation layer AuthenticationService!
        
        // Register SMS Services
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        // ? SMS Session Management - Centralized in Application layer
        builder.Services.ConfigureSMSSession();

        // Configure notification settings
        builder.Services.Configure<NotificationSettings>(
            builder.Configuration.GetSection(NotificationSettings.SectionName));

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "SMS Confidential Reporting API",
                Version = "v1",
                Description = "API for external systems to submit confidential safety reports"
            });

            // Add API Key authentication to Swagger
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key needed to access the endpoints. X-API-Key: {your-api-key}",
                In = ParameterLocation.Header,
                Name = "X-API-Key",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        },
                        Scheme = "ApiKeyScheme",
                        Name = "X-API-Key",
                        In = ParameterLocation.Header,
                    },
                    new List<string>()
                }
            });
        });



        var app = builder.Build();
        
        // Set up service locator for static access
        ServiceLocator.Current = app.Services;

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
            // ADD THESE LINES FOR SWAGGER
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SMS Confidential Reporting API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Add session middleware
        app.UseSession();
        
        // ? ADD SMS AUTHENTICATION MIDDLEWARE
        app.UseSMSAuthentication();

        app.UseAntiforgery();



        // ============================================================================
        // PIPELINE TEST ENDPOINT - FOR DEVELOPMENT/TESTING
        // ============================================================================
        
        // Test endpoint to verify pipelines are working
        app.MapPost("/api/test-pipeline", async (
            [FromBody] string testMessage,
            IMediator mediator,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("Testing pipeline execution with message: {Message}", testMessage);

                var command = new TestPipelineCommand(testMessage);
                var result = await mediator.SendAsync(command, CancellationToken.None);

                if (result.IsSuccess)
                {
                    return Results.Ok(new
                    {
                        success = true,
                        message = "Pipeline test completed successfully!",
                        result = result.Value,
                        timestamp = DateTime.UtcNow
                    });
                }
                else
                {
                    return Results.BadRequest(new
                    {
                        success = false,
                        message = "Pipeline test failed",
                        error = result.Error?.Message,
                        timestamp = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing pipeline");
                return Results.Problem(
                    detail: ex.Message,
                    title: "Pipeline Test Failed",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .WithName("TestPipeline")
        .WithTags("Development")
        .WithSummary("Test pipeline execution")
        .WithDescription("Tests that audit pipelines are working correctly")
        .Produces<object>(StatusCodes.Status200OK);

        // ============================================================================
        // CONFIDENTIAL REPORTING MINIMAL API - FOR EXTERNAL SYSTEMS
        // ============================================================================
        app.MapPost("/api/confidential-reports", async (
            [FromBody] ConfidentialReportApiRequest request,
            IMediator mediator,
            ILogger<Program> logger) =>
        {
            try
            {
                logger.LogInformation("External confidential report submission started from: {Source}",
                    request.SourceSystem ?? "Unknown");

                // ? Enhanced validation with proper error responses
                var validationErrors = new List<string>();
                
                if (string.IsNullOrEmpty(request.HazardType))
                    validationErrors.Add("hazardType is required");
                
                if (string.IsNullOrEmpty(request.Description))
                    validationErrors.Add("description is required");
                
                if (string.IsNullOrEmpty(request.Location))
                    validationErrors.Add("location is required");

                if (request.Description?.Length > 2000)
                    validationErrors.Add("description cannot exceed 2000 characters");

                if (validationErrors.Any())
                {
                    return Results.BadRequest(new
                    {
                        error = "Validation failed",
                        details = validationErrors,
                        timestamp = DateTime.UtcNow
                    });
                }

                // ===============================
                // STEP 1: CREATE PARENT REPORT (Aligned with UI)
                // ===============================
                var report = new Report(new ReportID("RP-0000"))
                {
                    Code = "RP-0000", // Database will generate actual code
                    Name = $"External Confidential - {request.HazardCategory} - {request.HazardType}",
                    Description = request.Description,
                    SubmittedBy = "EXTERNAL_SYSTEM",
                    SubmittedDate = DateTime.UtcNow,
                    SubmittingDepartment = request.SourceSystem ?? "EXTERNAL_API",
                    SubmittingDepartmentJobFunction = "API_SUBMISSION",
                    IncidentDateTime = DateTime.UtcNow, // Could be enhanced to accept from request
                    IsAnonymous = true,
                    ReportContactName = string.Empty, // Anonymous
                    ReportContactCell = string.Empty,
                    ReportContactEmail = string.Empty,
                    Stage = "INITIAL",
                    Status = ReportStatus.NeedsValidation,
                    CreatedBy = "EXTERNAL_SYSTEM",
                    CreatedDate = DateTime.UtcNow
                };

                var reportResult = await mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);

                if (reportResult.IsFailure)
                {
                    logger.LogError("Failed to create report: {Error}", reportResult.Error?.Message);
                    return Results.Problem(
                        detail: reportResult.Error?.Message,
                        title: "Report Creation Failed",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                var actualReportCode = reportResult.Value.Code;
                logger.LogInformation("? External report created: {ReportCode}", actualReportCode);

                // ===============================
                // STEP 2: CREATE CONFIDENTIAL HAZARD (Aligned with UI)
                // ===============================
                var hazard = new Hazard(new HazardID("HZ-0000"))
                {
                    Code = "HZ-0000", // Database will generate actual code
                    Name = $"External - {request.HazardCategory} - {request.HazardType}",
                    Description = request.Description,
                    HazardCategory = request.HazardCategory ?? "EXTERNAL",
                    HazardType = request.HazardType,
                    ReportCode = actualReportCode,
                    IsInitialHazard = true,
                    LocationArea = request.Location,
                    Status = HazardStatus.InitialRiskAssessment,
                    CreatedBy = "EXTERNAL_SYSTEM",
                    CreatedDate = DateTime.UtcNow
                };

                // Handle geographic coordinates if provided (aligned with UI)
                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    hazard.LocationSubArea = $"Lat: {request.Latitude:F6}, Lng: {request.Longitude:F6}";
                }

                var createHazardCommand = new CreateHazardCommand(hazard);
                var createdHazardResult = await mediator.SendAsync(createHazardCommand, CancellationToken.None);

                if (createdHazardResult.IsFailure)
                {
                    logger.LogError("Failed to create hazard: {Error}", createdHazardResult.Error?.Message);
                    return Results.Problem(
                        detail: createdHazardResult.Error?.Message,
                        title: "Hazard Creation Failed",
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                var createdHazard = createdHazardResult.Value;
                logger.LogInformation("? External hazard created: {HazardCode} for Report: {ReportCode}", 
                    createdHazard.Code, actualReportCode);

                // ===============================
                // STEP 3: CREATE PRODUCTION TRACKING (Aligned with UI)
                // ===============================
                var hazardReportTracking = new HazardReportTracking(new HazardReportTrackingID("HT-TEMP"))
                {
                    HazardCode = createdHazard.Code,
                    ReportCode = createdHazard.ReportCode,
                    TrackingCode = "HT-0000", // Database will generate actual tracking code
                    CreatedBy = "EXTERNAL_SYSTEM",
                    CreatedDate = DateTime.UtcNow
                };

                var trackingCommand = new CreateHazardReportTrackingCommand(hazardReportTracking);
                var trackingResult = await mediator.SendAsync(trackingCommand, CancellationToken.None);

                if (trackingResult.IsFailure)
                {
                    logger.LogError("Failed to create tracking: {Error}", trackingResult.Error?.Message);
                    return Results.Problem(
                        detail: trackingResult.Error?.Message,
                        title: "Tracking Creation Failed",  
                        statusCode: StatusCodes.Status500InternalServerError
                    );
                }

                var actualTrackingCode = trackingResult.Value.TrackingCode;
                logger.LogInformation("? External tracking created: {TrackingCode}", actualTrackingCode);

                // ===============================
                // STEP 4: PROCESS ATTACHMENTS (Enhanced error handling)
                // ===============================
                var processedFiles = 0;
                var failedFiles = 0;

                if (request.Attachments?.Any() == true)
                {
                    logger.LogInformation("Processing {Count} attachments for external hazard {HazardCode}", 
                        request.Attachments.Count, createdHazard.Code);

                    foreach (var attachment in request.Attachments)
                    {
                        try
                        {
                            // Validate base64 and file size
                            if (string.IsNullOrEmpty(attachment.Base64Content))
                            {
                                logger.LogWarning("Skipping empty attachment: {FileName}", attachment.FileName);
                                failedFiles++;
                                continue;
                            }

                            byte[] fileData;
                            try
                            {
                                fileData = Convert.FromBase64String(attachment.Base64Content);
                            }
                            catch (FormatException)
                            {
                                logger.LogWarning("Invalid base64 content for attachment: {FileName}", attachment.FileName);
                                failedFiles++;
                                continue;
                            }

                            // Check file size (10MB limit)
                            if (fileData.Length > 10 * 1024 * 1024)
                            {
                                logger.LogWarning("File too large (>10MB): {FileName}", attachment.FileName);
                                failedFiles++;
                                continue;
                            }

                            var hazardFile = new HazardFile(new HazardFileID("HF-0000"))
                            {
                                Code = "HF-0000", // Database will generate actual code
                                HazardCode = createdHazard.Code,
                                ReportCode = actualReportCode,
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
                            var fileResult = await mediator.SendAsync(createFileCommand, CancellationToken.None);

                            if (fileResult.IsSuccess)
                            {
                                processedFiles++;
                                logger.LogInformation("? Processed attachment: {FileName} ({Size} bytes)", 
                                    attachment.FileName, fileData.Length);
                            }
                            else
                            {
                                failedFiles++;
                                logger.LogWarning("Failed to save attachment: {FileName}. Error: {Error}", 
                                    attachment.FileName, fileResult.Error?.Message);
                            }
                        }
                        catch (Exception fileEx)
                        {
                            failedFiles++;
                            logger.LogWarning(fileEx, "Exception processing attachment: {FileName}", attachment.FileName);
                        }
                    }

                    logger.LogInformation("File processing completed: {Processed} successful, {Failed} failed", 
                        processedFiles, failedFiles);
                }

                // ===============================
                // SUCCESS RESPONSE (Production-ready)
                // ===============================
                var response = new ConfidentialReportApiResponse
                {
                    TrackingId = actualTrackingCode,
                    HazardId = createdHazard.Code,
                    ReportId = actualReportCode,
                    SubmissionDateTime = DateTime.UtcNow,
                    Status = "Submitted",
                    Message = $"Confidential report submitted successfully. Tracking ID: {actualTrackingCode}",
                    ProcessedFiles = processedFiles,
                    FailedFiles = failedFiles
                };

                logger.LogInformation("? External confidential report submitted successfully. " +
                    "TrackingId: {TrackingId}, ReportId: {ReportId}, HazardId: {HazardId}, Files: {ProcessedFiles}/{TotalFiles}", 
                    actualTrackingCode, actualReportCode, createdHazard.Code, processedFiles, 
                    request.Attachments?.Count ?? 0);

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Unexpected error processing external confidential report submission");
                return Results.Problem(
                    detail: "An unexpected error occurred while processing your confidential report.",
                    title: "Internal Server Error",
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .AddEndpointFilter<ApiKeyAuthenticationFilter>()
        .WithName("SubmitConfidentialReport")
        .WithTags("ConfidentialReporting")
        .WithSummary("Submit a confidential safety report from external systems")
        .WithDescription("Allows external systems to submit confidential safety reports")
        .Produces<ConfidentialReportApiResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
        // <-- Add this line

        // ============================================================================
        // REFERENCE DATA ENDPOINTS FOR API CONSUMERS
        // ============================================================================

        // Get all available hazard categories
        app.MapGet("/api/confidential-reports/hazard-categories", () =>
        {
            var categories = HazardCategory.GetAllValues()
                .Select(hc => new
                {
                    value = hc.Value,
                    name = hc.Name,
                    description = hc.Description,
                    sortOrder = hc.SortOrder
                })
                .OrderBy(x => x.sortOrder)
                .ToList();

            return Results.Ok(new
            {
                message = "Available hazard categories for confidential reporting",
                categories = categories,
                totalCount = categories.Count
            });
        })
        .AddEndpointFilter<ApiKeyAuthenticationFilter>()
        .WithName("GetHazardCategories")
        .WithTags("ConfidentialReporting")
        .WithSummary("Get all available hazard categories")
        .WithDescription("Returns all valid hazard category values for API submissions")
        .Produces<object>(StatusCodes.Status200OK);

        // Get hazard types (optionally filtered by category)
        app.MapGet("/api/confidential-reports/hazard-types", (string? category) =>
        {
            IEnumerable<HazardType> hazardTypes;

            if (string.IsNullOrEmpty(category))
            {
                hazardTypes = HazardType.GetAllValues();
            }
            else
            {
                hazardTypes = HazardType.GetByCategory(category);
            }

            var types = hazardTypes
                .Select(ht => new
                {
                    value = ht.Value,
                    name = ht.Name,
                    description = ht.Description,
                    category = ht.Category,
                    guidanceText = ht.GuidanceText,
                    requiresRegulatoryReporting = ht.RequiresRegulatoryReporting
                })
                .ToList();

            return Results.Ok(new
            {
                message = category != null
                    ? $"Hazard types for category: {category}"
                    : "All available hazard types",
                category = category,
                hazardTypes = types,
                totalCount = types.Count
            });
        })
        .AddEndpointFilter<ApiKeyAuthenticationFilter>()
        .WithName("GetHazardTypes")
        .WithTags("ConfidentialReporting")
        .WithSummary("Get available hazard types")
        .WithDescription("Get hazard types. Use ?category=INCIDENT to filter by category")
        .Produces<object>(StatusCodes.Status200OK);

        // Get complete reference data in hierarchical format
        app.MapGet("/api/confidential-reports/reference-data", () =>
        {
            var categories = HazardCategory.GetAllValues()
                .Select(hc => new
                {
                    value = hc.Value,
                    name = hc.Name,
                    description = hc.Description,
                    sortOrder = hc.SortOrder,
                    hazardTypes = HazardType.GetByCategory(hc.Value)
                        .Select(ht => new
                        {
                            value = ht.Value,
                            name = ht.Name,
                            description = ht.Description,
                            guidanceText = ht.GuidanceText,
                            requiresRegulatoryReporting = ht.RequiresRegulatoryReporting
                        })
                        .ToList()
                })
                .OrderBy(x => x.sortOrder)
                .ToList();

            return Results.Ok(new
            {
                message = "Complete reference data for confidential reporting API",
                lastUpdated = DateTime.UtcNow,
                categories = categories,
                totalCategories = categories.Count,
                totalHazardTypes = categories.Sum(c => c.hazardTypes.Count),
                usage = new
                {
                    instruction = "Use the 'value' field for API submissions",
                    example = new
                    {
                        hazardCategory = "INCIDENT",
                        hazardType = "AIRCRAFT_INCIDENT"
                    }
                }
            });
        })
        .AddEndpointFilter<ApiKeyAuthenticationFilter>()
        .WithName("GetReferenceData")
        .WithTags("ConfidentialReporting")
        .WithSummary("Get complete reference data")
        .WithDescription("Returns all categories and hazard types in hierarchical structure")
        .Produces<object>(StatusCodes.Status200OK);

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
    
}

// ============================================================================
// API DATA MODELS
// ============================================================================

/// <summary>
/// Request model for external confidential report submissions
/// </summary>
/// <summary>
/// Request model for external confidential report submissions
/// </summary>
public record ConfidentialReportApiRequest
{
    /// <summary>
    /// The category of the hazard (required). Use GET /api/confidential-reports/hazard-categories to see all valid values.
    /// Examples: "INCIDENT", "FOD", "WILDLIFE", "OPERATIONAL_CHANGE"
    /// </summary>
    /// <example>INCIDENT</example>
    public string HazardCategory { get; init; } = string.Empty;

    /// <summary>
    /// Specific type of hazard being reported (required). Must match a valid value for the selected category.
    /// Use GET /api/confidential-reports/hazard-types?category={categoryValue} to see valid types for a category.
    /// Examples: "AIRCRAFT_INCIDENT", "BIRD_STRIKE", "METAL_FOD"
    /// </summary>
    /// <example>AIRCRAFT_INCIDENT</example>
    public string HazardType { get; init; } = string.Empty;

    /// <summary>
    /// Detailed description of the hazard or incident (required, max 2000 characters)
    /// </summary>
    /// <example>Aircraft experienced engine failure during takeoff roll, aborting takeoff safely</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Location where the hazard occurred (required)
    /// </summary>
    /// <example>Runway 28L, approximately 2000ft from threshold</example>
    public string Location { get; init; } = string.Empty;

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
    /// Name/identifier of the external system submitting the report
    /// </summary>
    /// <example>MAINTENANCE_SYSTEM_v2.1</example>
    public string? SourceSystem { get; init; }

    /// <summary>
    /// Optional incident date/time (if not provided, current time is used)
    /// </summary>
    /// <example>2024-01-15T14:30:00Z</example>
    public DateTime? IncidentDateTime { get; init; }

    /// <summary>
    /// Optional file attachments as base64 encoded strings (max 10MB per file)
    /// </summary>
    public List<FileAttachment>? Attachments { get; init; }

    /// <summary>
    /// Additional metadata or context information
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// Response model for confidential report submissions - Enhanced for production
/// </summary>
public record ConfidentialReportApiResponse
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
    /// Number of files successfully processed
    /// </summary>
    public int ProcessedFiles { get; init; } = 0;
    
    /// <summary>
    /// Number of files that failed to process
    /// </summary>
    public int FailedFiles { get; init; } = 0;
}

/// <summary>
/// File attachment model for API
/// </summary>
public record FileAttachment
{
    public string FileName { get; init; } = string.Empty;
    public string Base64Content { get; init; } = string.Empty;
    public string? ContentType { get; init; }
}