using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using SMS_Application.Configuration;

using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Security;

using SMS_Shared.Configuration;

using SMS3.Components;
using SMS3.Configuration;

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

        builder.Services.AddScoped<ApiKeyAuthenticationFilter>();
        // Register authentication service as singleton
        builder.Services.AddSingleton<AuthenticationService>();

        // Register SMS Services
        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        // SMS Session Management
        builder.Services.ConfigureSMSSession();

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

        app.UseAntiforgery();



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

                // Validate required fields
                if (string.IsNullOrEmpty(request.HazardType) ||
                    string.IsNullOrEmpty(request.Description) ||
                    string.IsNullOrEmpty(request.Location))
                {
                    return Results.BadRequest(new
                    {
                        error = "Missing required fields",
                        required = new[] { "hazardType", "description", "location" }
                    });
                }

                // Generate anonymous tracking ID
                var trackingId = GenerateAnonymousTrackingId();

                // ===============================
                // STEP 1: CREATE PARENT REPORT
                // ===============================
                var report = new Report(new ReportID("RP-0000"))
                {
                    Code = "RP-0000",
                    Name = $"External Confidential Report - {request.HazardType}",
                    Description = request.Description,
                    Stage = "Initial",
                    Status = "Initial",
                    SubmittedBy = "EXTERNAL_SYSTEM",
                    SubmittingDepartment = request.SourceSystem ?? "EXTERNAL"
                };

                var reportResult = await mediator.SendAsync(new CreateReportCommand(report), CancellationToken.None);

                if (reportResult.IsFailure)
                {
                    logger.LogError("Failed to create report: {Error}", reportResult.Error?.Message);
                    return Results.Problem("Failed to create report");
                }

                var actualReportCode = reportResult.Value.Code;

                // ===============================
                // STEP 2: CREATE CONFIDENTIAL HAZARD
                // ===============================
                var hazard = new Hazard(new HazardID("HZ-0000"))
                {
                    Code = "HZ-0000",
                    Name = $"External Confidential - {request.HazardType}",
                    Description = request.Description,
                    HazardCategory = request.HazardCategory ?? "EXTERNAL",
                    HazardType = request.HazardType,
                    //SubmittedBy = "EXTERNAL_SYSTEM",
                    ////SubmittedOn = DateTime.UtcNow,
                    ////IncidentDateTime //=>>>> from Widget
                    //ReportingDepartment = request.SourceSystem ?? "External System",
                    //IsAnonymous = true,
                    ReportCode = actualReportCode,
                    IsInitialHazard = true,
                    LocationArea = request.Location,
                    //CreatedOn = DateTime.UtcNow,
                    CreatedBy = "EXTERNAL_SYSTEM"
                };

                // Handle geographic coordinates if provided
                if (request.Latitude.HasValue && request.Longitude.HasValue)
                {
                    hazard.LocationSubArea = $"Lat: {request.Latitude:F6}, Lng: {request.Longitude:F6}";
                }

                var createHazardCommand = new CreateHazardCommand(hazard);
                var createdHazardResult = await mediator.SendAsync(createHazardCommand, CancellationToken.None);

                if (createdHazardResult.IsFailure)
                {
                    logger.LogError("Failed to create hazard: {Error}", createdHazardResult.Error?.Message);
                    return Results.Problem("Failed to create hazard");
                }

                var createdHazard = createdHazardResult.Value;

                // ===============================
                // STEP 3: PROCESS ATTACHMENTS (if any)
                // ===============================
                if (request.Attachments?.Any() == true)
                {
                    foreach (var attachment in request.Attachments)
                    {
                        try
                        {
                            var fileData = Convert.FromBase64String(attachment.Base64Content);

                            var hazardFile = new HazardFile(new HazardFileID("HF-0000"))
                            {
                                Code = "HF-0000",
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
                                IsConfidential = true
                            };

                            var createFileCommand = new CreateHazardFileCommand(hazardFile);
                            await mediator.SendAsync(createFileCommand, CancellationToken.None);
                        }
                        catch (Exception fileEx)
                        {
                            logger.LogWarning(fileEx, "Failed to process attachment: {FileName}", attachment.FileName);
                        }
                    }
                }

                // ===============================
                // SUCCESS RESPONSE
                // ===============================
                var response = new ConfidentialReportApiResponse
                {
                    TrackingId = trackingId,
                    HazardId = createdHazard.Code,
                    ReportId = actualReportCode,
                    SubmissionDateTime = DateTime.UtcNow,
                    Status = "Submitted",
                    Message = "Confidential report submitted successfully"
                };

                logger.LogInformation("External confidential report submitted successfully. TrackingId: {TrackingId}", trackingId);

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing external confidential report submission");
                return Results.Problem("An error occurred while processing the confidential report");
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
    private static string GenerateAnonymousTrackingId()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomComponent = Random.Shared.Next(1000, 9999);
        var checksum = (timestamp.GetHashCode() + randomComponent).ToString()[..2];
        return $"CONF-{timestamp}-{randomComponent}-{checksum}";
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
    /// The category of the hazard. Use GET /api/confidential-reports/hazard-categories to see all valid values.
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
    /// Detailed description of the hazard or incident (required)
    /// </summary>
    /// <example>Aircraft experienced engine failure during takeoff roll, aborting takeoff safely</example>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Location where the hazard occurred (required)
    /// </summary>
    /// <example>Runway 28L, approximately 2000ft from threshold</example>
    public string Location { get; init; } = string.Empty;

    /// <summary>
    /// Optional latitude coordinate for precise location
    /// </summary>
    /// <example>45.5898</example>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Optional longitude coordinate for precise location  
    /// </summary>
    /// <example>-122.5951</example>
    public decimal? Longitude { get; init; }

    /// <summary>
    /// Name/identifier of the external system submitting the report
    /// </summary>
    /// <example>MAINTENANCE_SYSTEM_v2.1</example>
    public string? SourceSystem { get; init; }

    /// <summary>
    /// Optional file attachments as base64 encoded strings
    /// </summary>
    public List<FileAttachment>? Attachments { get; init; }
}

/// <summary>
/// Response model for confidential report submissions
/// </summary>
public record ConfidentialReportApiResponse
{
    public string TrackingId { get; init; } = string.Empty;
    public string HazardId { get; init; } = string.Empty;
    public string ReportId { get; init; } = string.Empty;
    public DateTime SubmissionDateTime { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
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