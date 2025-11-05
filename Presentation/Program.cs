using PDXSMS_Infrastructure.Common;
using PDXSMS.Configuration;
using PDXSMS_Infrastructure.Configuration;
using PDXSMS_Shared.Configuration;
using PDXSMS_Presentation.Middleware;
using PDXSMS.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// ?? CENTRALIZED RISK MATRIX SERVICE - Single Source of Truth
builder.Services.AddSingleton<StandardizedRiskMatrixService>();

// Register JSON data service first
builder.Services.AddScoped<UniversalJsonDataService>();
builder.Services.AddScoped<UniversalUserRepository>();
builder.Services.AddScoped<SMSIdGenerationService>();
builder.Services.AddScoped<StandardizedRiskMatrixService>();
builder.Services.AddScoped<RiskAssessmentRepository>();
builder.Services.AddScoped<RiskAssessmentPersistenceService>();
builder.Services.AddScoped<RiskAssessmentDataNormalizationService>();
builder.Services.AddScoped<DashboardDataService>();
builder.Services.AddScoped<MitigationSummaryService>();
builder.Services.AddScoped<SMSValidationDataPersistenceService>();
builder.Services.AddScoped<SMSWorkflowDataService>();
builder.Services.AddScoped<MapThumbnailService>(); // Add the new map thumbnail service
builder.Services.AddScoped<ReportDocumentService>(); // Add the new report document service

// Register command and query handlers
builder.Services.AddScoped<ValidateSMSRiskWithDatasetCommandHandler>();

// Add session support for maintaining assessment state
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Session timeout for risk assessments
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "PDXSMS.Session";
});

// Add Shared services (includes Feature Management, Logging, etc.)
builder.Services.AddSharedServices(builder.Configuration);

// Add Application layer services - Critical for proper Clean Architecture
// Note: CQRS Command and Query Handlers are registered in AddApplicationServices()
// This also registers UniversalUserRepository, UniversalJsonDataService, DashboardDataService, etc.
PDXSMS.Configuration.DependencyInjection.AddApplicationServices(builder.Services);

// ? CORE SERVICES - Essential for basic functionality
builder.Services.AddSingleton<JsonUserRepository>();
builder.Services.AddSingleton<RiskAssessmentRepository>();
builder.Services.AddSingleton<SMSIdGenerationService>();

// ? TEMP COMMENTED - Will re-enable one by one to isolate issue
// builder.Services.AddSingleton<SMSWorkflowDataService>();
// builder.Services.AddSingleton<SMSValidationDataPersistenceService>();
// builder.Services.AddSingleton<DataMigrationService>();

// ? BUSINESS LOGIC SERVICES - Should be safe
builder.Services.AddScoped<CleanRiskAssessmentService>();
builder.Services.AddScoped<RiskAssessmentPersistenceService>();
builder.Services.AddScoped<RiskAssessmentDataNormalizationService>();
builder.Services.AddScoped<MitigationSummaryService>();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// *** DISABLED: EAGER INITIALIZATION to prevent auto-seeding ***
// This was causing repositories to create default data on startup
// REMOVED to achieve clean slate testing environment
/*
using (var scope = app.Services.CreateScope())
{
    // Initialize Universal User Repository (creates all user types and groups)
    var universalUserRepository = scope.ServiceProvider.GetRequiredService<UniversalUserRepository>();
    var debugInfo = await universalUserRepository.GetAllUsersForDebugAsync();
    Console.WriteLine($"*** STARTUP: UniversalUserRepository initialized");
    
    // Initialize Risk Assessment Repository (creates sample assessment data)
    var riskAssessmentRepository = scope.ServiceProvider.GetRequiredService<RiskAssessmentRepository>();
    var assessmentSummary = await riskAssessmentRepository.GetAssessmentSummaryAsync("sample");
    Console.WriteLine($"*** STARTUP: RiskAssessmentRepository initialized");
    
    // Initialize legacy repository for login compatibility
    var legacyUserRepository = scope.ServiceProvider.GetRequiredService<JsonUserRepository>();
    var legacyUserCount = (await legacyUserRepository.GetAllUsersAsync()).Count;
    Console.WriteLine($"*** STARTUP: JsonUserRepository initialized with {legacyUserCount} users");
}
*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware - must be before UseAuthorization
app.UseSession();

// Add custom authentication middleware
app.UseMiddleware<AuthenticationMiddleware>();

app.UseAuthorization();

app.MapRazorPages();

// Development endpoints to manage all user types
if (app.Environment.IsDevelopment())
{
    // Legacy user debug endpoint (ApplicationUser for login)
    app.MapGet("/api/users/debug", async (JsonUserRepository userRepo) =>
    {
        var users = await userRepo.GetAllUsersAsync();
        return Results.Json(users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.DisplayName,
            u.Department,
            u.JobTitle,
            u.ApplicationRoles,
            u.Permissions,
            u.ContactSummary,
            u.NotificationPreferences,
            u.LastLoginDate,
            u.IsActive,
            u.IsLockedOut,
            PasswordHint = u.Username switch
            {
                "admin@flypdx.com" => "admin123",
                "sarah.johnson@flypdx.com" => "safety123",
                "mike.chen@flypdx.com" => "ops123",
                "lisa.rodriguez@flypdx.com" => "audit123",
                "john.smith@flypdx.com" => "user123",
                _ => "unknown"
            }
        }));
    });
    
    // Universal user repository debug endpoint (ALL user types)
    app.MapGet("/api/users/all", async (UniversalUserRepository universalRepo) =>
    {
        var allData = await universalRepo.GetAllUsersForDebugAsync();
        return Results.Json(allData);
    });
    
    // Application Users management
    app.MapGet("/api/users/application", async (UniversalUserRepository universalRepo) =>
    {
        var users = await universalRepo.GetAllApplicationUsersAsync();
        return Results.Json(users);
    });
    
    // SMS Organizational Users management  
    app.MapGet("/api/users/sms", async (UniversalUserRepository universalRepo) =>
    {
        var users = await universalRepo.GetAllSMSUsersAsync();
        return Results.Json(users);
    });
    
    // Stakeholder Users management
    app.MapGet("/api/users/stakeholders", async (UniversalUserRepository universalRepo) =>
    {
        var users = await universalRepo.GetAllStakeholderUsersAsync();
        return Results.Json(users);
    });
    
    // Groups management
    app.MapGet("/api/groups", async (UniversalUserRepository universalRepo) =>
    {
        var groups = await universalRepo.GetAllGroupsAsync();
        return Results.Json(groups);
    });
    
    // Get SMS Team members
    app.MapGet("/api/users/sms/team/{teamName}", async (string teamName, UniversalUserRepository universalRepo) =>
    {
        var users = await universalRepo.GetSMSUsersByTeamAsync(teamName);
        return Results.Json(users);
    });
    
    // Get Stakeholder Group members
    app.MapGet("/api/users/stakeholders/group/{groupName}", async (string groupName, UniversalUserRepository universalRepo) =>
    {
        var users = await universalRepo.GetStakeholderUsersByGroupAsync(groupName);
        return Results.Json(users);
    });
    
    // Risk Assessment Repository debug endpoints
    app.MapGet("/api/assessments", async (RiskAssessmentRepository riskRepo) =>
    {
        try
        {
            return Results.Json(new List<object>());
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = ex.Message });
        }
    });
    
    app.MapGet("/api/assessments/{id}", async (string id, RiskAssessmentRepository riskRepo) =>
    {
        try
        {
            return Results.Json(new { Id = id, Name = "Assessment", Status = "Draft" });
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = ex.Message });
        }
    });
    
    app.MapGet("/api/assessments/{id}/summary", async (string id, RiskAssessmentRepository riskRepo) =>
    {
        try
        {
            return Results.Json(new { Id = id, Summary = "Assessment Summary" });
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = ex.Message });
        }
    });
    
    app.MapGet("/api/hazards/assessment/{assessmentId}", async (string assessmentId, RiskAssessmentRepository riskRepo) =>
    {
        try
        {
            return Results.Json(new List<object>());
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = ex.Message });
        }
    });
    
    app.MapGet("/api/mitigation/hazard/{hazardId}", async (string hazardId, RiskAssessmentRepository riskRepo) =>
    {
        try
        {
            return Results.Json(new List<object>());
        }
        catch (Exception ex)
        {
            return Results.Json(new { error = ex.Message });
        }
    });
    
    // SMS ID Generation debug endpoints
    app.MapGet("/api/debug/id-counters", (SMSIdGenerationService idService) =>
    {
        var counters = idService.GetCurrentCounters();
        return Results.Json(new
        {
            message = "Current ID counters",
            counters = new
            {
                reports = new { counter = counters.ReportCounter, lastYear = counters.LastReportYear, format = "SMS-YYYY-XXXX" },
                hazards = new { counter = counters.HazardCounter, format = "HZ-XXXX" },
                riskAssessments = new { counter = counters.RiskAssessmentCounter, format = "RA-XXXX" },
                mitigationStrategies = new { counter = counters.MitigationStrategyCounter, format = "MS-XXXX" },
                riskAnalysis = new { counter = counters.RiskAnalysisCounter, format = "RAN-XXXX" },
                scoringPanels = new { counter = counters.ScoringPanelCounter, format = "SP-XXXX" },
                panelScores = new { counter = counters.PanelScoreCounter, format = "PS-XXXX" },
                assignments = new { counter = counters.AssignmentCounter, format = "ASG-XXXX" },
                audits = new { counter = counters.AuditCounter, format = "AUD-XXXX" }
            },
            lastUpdated = DateTime.UtcNow
        });
    });

    // Generate sample IDs endpoint
    app.MapPost("/api/debug/generate-sample-ids", (SMSIdGenerationService idService) =>
    {
        return Results.Json(new
        {
            message = "Generated sample IDs",
            sampleIds = new
            {
                reportId = idService.GenerateReportId(),
                hazardId = idService.GenerateHazardId(),
                riskAssessmentId = idService.GenerateRiskAssessmentId(),
                mitigationStrategyId = idService.GenerateMitigationStrategyId(),
                riskAnalysisId = idService.GenerateRiskAnalysisId(),
                scoringPanelId = idService.GenerateScoringPanelId(),
                panelScoreId = idService.GeneratePanelScoreId(),
                assignmentId = idService.GenerateAssignmentId(),
                auditId = idService.GenerateAuditId()
            },
            generatedAt = DateTime.UtcNow
        });
    });

    // Workflow data debug endpoints
    app.MapGet("/api/debug/workflow-status", () =>
    {
        return Results.Json(new
        {
            message = "Workflow system status - service not available",
            workflowDataLoaded = false,
            availableMethods = new[]
            {
                "SMSWorkflowDataService not currently registered",
                "Enable service registration in Program.cs to access workflow methods"
            },
            statusAt = DateTime.UtcNow
        });
    });

    // Data Migration Endpoints
    app.MapGet("/api/debug/migration/check", async () =>
    {
        // Simple static response since DataMigrationService is not registered
        return Results.Json(new
        {
            message = "Migration status check - service not available",
            migrationNeeded = false,
            checkedAt = DateTime.UtcNow,
            action = "DataMigrationService not currently registered"
        });
    });

    app.MapPost("/api/debug/migration/run", async () =>
    {
        // Simple static response since DataMigrationService is not registered
        return Results.Json(new
        {
            message = "Migration service not available",
            success = false,
            migratedAt = DateTime.UtcNow,
            nextStep = "Enable DataMigrationService registration in Program.cs"
        });
    });

    // Existing mitigations.json debug endpoints
    app.MapGet("/api/mitigations", async () =>
    {
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
        var mitigationsFile = Path.Combine(appDataPath, "mitigations.json");
        
        if (!File.Exists(mitigationsFile))
        {
            return Results.Json(new List<object>());
        }
        
        var json = await File.ReadAllTextAsync(mitigationsFile);
        var mitigations = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Dictionary<string, object?>>();
        
        return Results.Json(mitigations);
    });

    app.MapGet("/api/mitigations/{id}", async (string id) =>
    {
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
        var mitigationsFile = Path.Combine(appDataPath, "mitigations.json");
        
        if (!File.Exists(mitigationsFile))
        {
            return Results.NotFound($"Mitigation {id} not found - no mitigations file exists");
        }
        
        var json = await File.ReadAllTextAsync(mitigationsFile);
        var mitigations = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Dictionary<string, object?>>();
        
        var mitigation = mitigations.FirstOrDefault(m => 
            m.ContainsKey("id") && m["id"]?.ToString() == id);
            
        return mitigation != null ? Results.Json(mitigation) : Results.NotFound($"Mitigation {id} not found");
    });

    app.MapGet("/api/mitigations/hazard/{hazardId}", async (string hazardId) =>
    {
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "AppData");
        var mitigationsFile = Path.Combine(appDataPath, "mitigations.json");
        
        if (!File.Exists(mitigationsFile))
        {
            return Results.Json(new List<object>());
        }
        
        var json = await File.ReadAllTextAsync(mitigationsFile);
        var mitigations = JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Dictionary<string, object?>>();
        
        var hazardMitigations = mitigations.Where(m => 
            m.ContainsKey("relatedHazard") && m["relatedHazard"]?.ToString() == hazardId).ToList();
            
        return Results.Json(hazardMitigations);
    });

    // Risk Assessment API endpoints
    app.MapGet("/api/risk-assessments", async (CleanRiskAssessmentService service) =>
    {
        try
        {
            return Results.Ok(new List<object>());
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving assessments: {ex.Message}");
        }
    });

    app.MapGet("/api/risk-assessments/{id}", async (string id, CleanRiskAssessmentService service) =>
    {
        try
        {
            return Results.Ok(new { Id = id, Name = "Assessment", Status = "Draft" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving assessment: {ex.Message}");
        }
    });

    app.MapGet("/api/assessments/{assessmentId}/hazards", async (string assessmentId, CleanRiskAssessmentService service) =>
    {
        try
        {
            return Results.Ok(new List<object>());
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error retrieving hazards: {ex.Message}");
        }
    });
}

app.Run();app.Run();