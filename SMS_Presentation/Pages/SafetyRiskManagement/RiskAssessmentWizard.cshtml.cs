using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Shared.Common;

namespace SMS.Presentation.Pages.SafetyRiskManagement;

/// <summary>
/// MISSION CRITICAL: Risk Assessment Wizard - Enhanced CQRS Integration
/// Creates RiskAssessment and AirportSharedDataset when transitioning from ReportValidation
/// Loads composite entity data from Database using CQRS mediator
/// PRESERVES EXISTING UI - Only changes backend integration!
/// </summary>
public class RiskAssessmentWizardModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly ILogger<RiskAssessmentWizardModel> _logger;

    public RiskAssessmentWizardModel(IMediator mediator, ILogger<RiskAssessmentWizardModel> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Properties - Composite Entity Data from Database

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public int StepNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? HazardId { get; set; }

    // Core Domain Entities loaded from database
    public RiskAssessment? Assessment { get; set; }
    public Hazard? PrimaryHazard { get; set; }
    public Report? SourceReport { get; set; }
    public AirportSharedDataset? SharedDataset { get; set; }
    public List<SMSApplicationUser> AvailableAssessors { get; set; } = new();
    public List<Hazard> RelatedHazards { get; set; } = new();

    // Form binding properties for step data
    [BindProperty]
    public string LeadAssessor { get; set; } = string.Empty;

    [BindProperty]
    public string SystemDescription { get; set; } = string.Empty;

    [BindProperty]
    public string SystemBoundaries { get; set; } = string.Empty;

    [BindProperty]
    public string SystemPurpose { get; set; } = string.Empty;

    [BindProperty]
    public string PersonnelFactors { get; set; } = string.Empty;

    [BindProperty]
    public string EquipmentFactors { get; set; } = string.Empty;

    [BindProperty]
    public string ProcedureFactors { get; set; } = string.Empty;

    [BindProperty]
    public string ResourceFactors { get; set; } = string.Empty;

    [BindProperty]
    public string EnvironmentFactors { get; set; } = string.Empty;

    [BindProperty]
    public List<string> StakeholderIds { get; set; } = new();

    // Step 2 - Hazard Management
    [BindProperty]
    public List<string> HazardIds { get; set; } = new();

    [BindProperty]
    public List<string> HazardDescriptions { get; set; } = new();

    [BindProperty]
    public List<string> HazardCategories { get; set; } = new();

    [BindProperty]
    public List<string> HazardFiveMComponents { get; set; } = new();

    // Step 3 - Risk Analysis
    [BindProperty]
    public string RiskAnalysisMethod { get; set; } = "SMS Risk Matrix";

    [BindProperty]
    public string RiskCriteria { get; set; } = string.Empty;

    [BindProperty]
    public Dictionary<string, string> HazardWorstOutcomes { get; set; } = new();

    [BindProperty]
    public Dictionary<string, string> HazardRootCauses { get; set; } = new();

    // Step 4 - Risk Assessment Properties
    [BindProperty]
    public string TolerabilityFramework { get; set; } = "PDX-SMS Default";

    [BindProperty]
    public string RiskAcceptanceCriteria { get; set; } = string.Empty;

    // Step 5 - Implementation Properties
    [BindProperty]
    public string ImplementationStrategy { get; set; } = string.Empty;

    [BindProperty]
    public DateTime? OverallTargetDate { get; set; }

    [BindProperty]
    public string ImplementationNotes { get; set; } = string.Empty;

    #endregion

    #region Additional Properties Required by UI Templates

    // Step 1 - 5M Framework properties
    [BindProperty]
    public string FiveMPersonnel { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMEquipment { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMProcedures { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMResources { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMPhysicalEnvironment { get; set; } = string.Empty;

    [BindProperty]
    public string FiveMOperationalEnvironment { get; set; } = string.Empty;

    // Step 1 - Stakeholder properties using SMSStakeholderUser
    public List<StakeholderGroup> StakeholderGroups_Data { get; set; } = new();
    public List<SMSStakeholderUser> AvailableStakeholders { get; set; } = new();
    
    [BindProperty]
    public string StakeholderGroups { get; set; } = string.Empty;
    
    [BindProperty]
    public string SelectedIndividualStakeholders { get; set; } = string.Empty;
    
    public List<string> SelectedStakeholderIds { get; set; } = new();

    // Step 2 - Hazard properties (using RelatedHazards as AvailableHazards)
    public List<Hazard> AvailableHazards => RelatedHazards;

    // Step 3 - Additional analysis properties
    [BindProperty]
    public List<string> AdditionalComments { get; set; } = new();

    // Step 4 - Panel and scoring properties
    public List<SMSApplicationUser> AvailableSMSUsers => AvailableAssessors;
    public List<string> SelectedPanelMembers { get; set; } = new();
    public Dictionary<string, List<PanelMemberScoreData>> PanelScores { get; set; } = new();
    public Dictionary<string, double> HazardAverageScores { get; set; } = new();
    public Dictionary<string, List<string>> HazardPanelMembers { get; set; } = new();

    // Step 5 - Implementation properties
    public string SavedImplementationStrategy => ImplementationStrategy;
    public string SavedOverallTargetDate => OverallTargetDate?.ToString("yyyy-MM-dd") ?? string.Empty;
    public string SavedImplementationNotes => ImplementationNotes;
    
    // Step 5 - Mitigation data structures
    public Dictionary<string, List<string>> SavedMitigationStrategies { get; set; } = new();

    // Assessment wrapper property for backward compatibility
    public AssessmentDataWrapper? AssessmentData { get; set; }

    // Helper properties for UI
    public int Count => RelatedHazards.Count;

    #endregion

    #region Helper Classes for UI Compatibility

    public class AssessmentDataWrapper
    {
        public List<IdentifiedHazardDto> IdentifiedHazards { get; set; } = new();
        public string ImplementationPlan { get; set; } = string.Empty;
    }

    public class IdentifiedHazardDto
    {
        public string Id { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string WorstCredibleOutcome { get; set; } = string.Empty;
        public string RiskLevel { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Likelihood { get; set; } = string.Empty;
        public string Tolerability { get; set; } = string.Empty;
        public List<string> ProposedMitigations { get; set; } = new();
        public List<string> CurrentMitigations { get; set; } = new();
    }

    public class StakeholderGroup
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Members { get; set; } = new();
        public int MemberCount => Members.Count;
    }

    public class PanelMemberScoreData
    {
        public string PanelMemberId { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty; // Alias for compatibility
        public string MemberName { get; set; } = string.Empty;
        public int SeverityScore { get; set; }
        public int LikelihoodScore { get; set; }
        public double CalculatedScore => SeverityScore * LikelihoodScore;
        public string RiskLevel { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;
        public bool IsComplete => SeverityScore > 0 && LikelihoodScore > 0;

        // Constructor to ensure MemberId is set
        public PanelMemberScoreData()
        {
            MemberId = PanelMemberId;
        }
    }

    #endregion

    #region UI Helper Methods

    public string GetRiskLevelClass(string riskLevel)
    {
        return riskLevel?.ToLowerInvariant() switch
        {
            "high" or "5" or "4" => "bg-danger text-white",
            "medium" or "3" => "bg-warning text-dark",
            "low" or "2" or "1" => "bg-success text-white",
            _ => "bg-secondary text-white"
        };
    }

    #endregion

    #region Page Handlers

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("Loading Risk Assessment Wizard: AssessmentId={AssessmentId}, HazardId={HazardId}, Step={Step}",
            Id, HazardId, StepNumber);

        try
        {
            // PHASE 1: Handle creation from ReportValidation (when Assessment doesn't exist yet)
            // Support both empty Id and "new" as indicators for creation
            if ((string.IsNullOrWhiteSpace(Id) || Id.Equals("new", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(HazardId))
            {
                _logger.LogInformation("Creating new RiskAssessment for Hazard: {HazardId}", HazardId);
                var createResult = await CreateRiskAssessmentFromHazardAsync(HazardId);
                
                if (createResult.IsFailure)
                {
                    _logger.LogError("Failed to create RiskAssessment for Hazard {HazardId}: {Error}",
                        HazardId, createResult.Error?.Message);
                    TempData["ErrorMessage"] = $"Error creating risk assessment: {createResult.Error?.Message}";
                    return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
                }

                Id = createResult.Value;
                _logger.LogInformation("Created RiskAssessment {AssessmentId} for Hazard {HazardId}", Id, HazardId);
                
                // Redirect to proper URL with the actual assessment ID
                return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
                    new { id = Id, stepNumber = StepNumber });
            }

            // PHASE 2: Load existing assessment and related entities
            if (string.IsNullOrWhiteSpace(Id))
            {
                _logger.LogWarning("No Assessment ID or Hazard ID provided");
                TempData["ErrorMessage"] = "Assessment ID or Hazard ID is required.";
                return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
            }

            // Load all related entities using CQRS
            await LoadCompositeEntityDataAsync();

            if (Assessment == null)
            {
                _logger.LogError("Risk Assessment not found: {AssessmentId}", Id);
                TempData["ErrorMessage"] = $"Risk Assessment {Id} not found.";
                return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
            }

            // PHASE 3: Populate form fields from loaded entities
            PopulateFormFieldsFromEntities();

            // PHASE 4: Load reference data for current step
            await LoadStepReferenceDataAsync();

            _logger.LogInformation("Successfully loaded Risk Assessment Wizard - Assessment: {Name}, Status: {Status}, Step: {Step}",
                Assessment.Name, Assessment.Status, StepNumber);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Risk Assessment Wizard");
            TempData["ErrorMessage"] = $"Error loading assessment: {ex.Message}";
            return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
        }
    }

    #endregion

    #region Creation Logic - From ReportValidation Transition

    /// <summary>
    /// Creates RiskAssessment and AirportSharedDataset when transitioning from ReportValidation
    /// This is the critical workflow transition when Decision = SMS_RISK
    /// </summary>
    private async Task<Result<string>> CreateRiskAssessmentFromHazardAsync(string hazardId)
    {
        try
        {
            _logger.LogInformation("Creating RiskAssessment workflow for Hazard: {HazardId}", hazardId);

            // STEP 1: Load the primary hazard to get related data
            var hazardQuery = new GetHazardByIdQuery(new HazardID(hazardId));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);

            if (hazardResult.IsFailure)
            {
                return Result<string>.Failure<string>(hazardResult.Error);
            }

            var hazard = hazardResult.Value;
            
            // STEP 2: Create RiskAssessment entity
            var assessmentName = $"Risk Assessment - {hazard.Code}";
            var leadAssessorId = HttpContext.Session.GetString("SMS_UserId") ?? "SYSTEM";

            RiskAssessment ra = new RiskAssessment(new RiskAssessmentID("RA-0000"));
            ra.AssessmentType = RiskAssessmentType.Initial;
            ra.Name = assessmentName;
            ra.LeadAssessorId = leadAssessorId;
            ra.RiskAssessmentCategory = RiskAssessmentCategory.FiveStep;
            ra.HazardCode = hazardId;
            ra.PrimaryHazardId = hazardId;
            
            
            // STEP 3: Save RiskAssessments (Initial AND Residual) using CQRS
            var createInitialAssessmentCommand = new CreateRiskAssessmentCommand(ra);
            var createdInitialAssessmentResult = await _mediator.SendAsync(createInitialAssessmentCommand, CancellationToken.None);

            if (createdInitialAssessmentResult.IsFailure)
            {
                return Result<string>.Failure<string>(createdInitialAssessmentResult.Error);
            }

            var createdInitialAssessment = createdInitialAssessmentResult.Value;

            //Now the Residual
            
            ra.AssessmentType = RiskAssessmentType.Residual;
            var createResidualAssessmentCommand = new CreateRiskAssessmentCommand(ra);

            var createdResidualAssessmentResult = await _mediator.SendAsync(createResidualAssessmentCommand, CancellationToken.None);

            if (createdResidualAssessmentResult.IsFailure)
            {
                return Result<string>.Failure<string>(createdResidualAssessmentResult.Error);
            }


            // STEP 4: Create AirportSharedDataset using hazard and report data
            await CreateAirportSharedDatasetAsync(hazard, createdInitialAssessment.Code);

            _logger.LogInformation("Successfully created RiskAssessment {AssessmentId} and AirportSharedDataset for Hazard {HazardId}",
                createdInitialAssessment.Code, hazardId);

            return Result<string>.Success(createdInitialAssessment.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating RiskAssessment from Hazard {HazardId}", hazardId);
            return Result<string>.Failure<string>(
                new SMS_Domain.Common.Error("CREATE_FAILED", $"Error creating risk assessment: {ex.Message}"));
        }
    }

    /// <summary>
    /// Creates AirportSharedDataset with data from Hazard and Report
    /// </summary>
    private async Task CreateAirportSharedDatasetAsync(Hazard hazard, string riskAssessmentId)
    {
        try
        {
            _logger.LogInformation("Creating AirportSharedDataset for RiskAssessment {AssessmentId}", riskAssessmentId);

            // Create AirportSharedDataset entity populated from hazard data
            var dataset = new AirportSharedDataset(new AirportSharedDatasetID($"ASD-0000"))
            {
                ReportID = hazard.ReportCode ?? "Unknown",
                HazardCode = hazard.Code,
                PrivateNarrative = hazard.Description,
                SharedNarrative = hazard.Description,
                LocationArea = hazard.Location,
                TriggeringEvent = hazard.HazardType,
                // Set basic flags based on hazard type
                AircraftInvolved = hazard.HazardType?.Contains("Aircraft", StringComparison.OrdinalIgnoreCase) ?? false,
                PoweredEquipmentInvolved = hazard.HazardType?.Contains("Equipment", StringComparison.OrdinalIgnoreCase) ?? false
            };

            // Save using CQRS
            var createDatasetCommand = new CreateAirportSharedDatasetCommand(dataset);
            var datasetResult = await _mediator.SendAsync(createDatasetCommand, CancellationToken.None);

            if (datasetResult.IsSuccess)
            {
                _logger.LogInformation("Successfully created AirportSharedDataset {DatasetId}", datasetResult.Value.Code);
            }
            else
            {
                _logger.LogError("Failed to create AirportSharedDataset: {Error}", datasetResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating AirportSharedDataset");
        }
    }

    #endregion

    #region Entity Loading - CQRS Integration

    /// <summary>
    /// Loads all related entities using CQRS queries
    /// </summary>
    private async Task LoadCompositeEntityDataAsync()
    {
        try
        {
            _logger.LogInformation("Loading composite entity data for Assessment: {AssessmentId}", Id);

            // Load RiskAssessment
            var assessmentQuery = new GetRiskAssessmentByIdQuery(new RiskAssessmentID(Id));
            var assessmentResult = await _mediator.SendAsync(assessmentQuery, CancellationToken.None);
            
            if (assessmentResult.IsSuccess)
            {
                Assessment = assessmentResult.Value;
                _logger.LogInformation("Loaded RiskAssessment: {Name} (Status: {Status})", 
                    Assessment.Name, Assessment.Status);

                // Load Primary Hazard if we have a hazard code
                if (!string.IsNullOrEmpty(Assessment.HazardCode))
                {
                    await LoadPrimaryHazardAsync(Assessment.HazardCode);
                }

                // Load related hazards (all hazards associated with this assessment)
                await LoadRelatedHazardsAsync();

                // Load Source Report if we have a report connection through hazard
                if (PrimaryHazard?.ReportCode != null)
                {
                    await LoadSourceReportAsync(PrimaryHazard.ReportCode);
                }

                // Load AirportSharedDataset
                await LoadAirportSharedDatasetAsync();
            }
            else
            {
                _logger.LogError("Failed to load RiskAssessment {AssessmentId}: {Error}", Id, assessmentResult.Error?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading composite entity data for Assessment: {AssessmentId}", Id);
        }
    }

    private async Task LoadPrimaryHazardAsync(string hazardCode)
    {
        try
        {
            var hazardQuery = new GetHazardByIdQuery(new HazardID(hazardCode));
            var hazardResult = await _mediator.SendAsync(hazardQuery, CancellationToken.None);
            
            if (hazardResult.IsSuccess)
            {
                PrimaryHazard = hazardResult.Value;
                HazardId = hazardCode; // Set for route consistency
                _logger.LogInformation("Loaded Primary Hazard: {HazardId} - {Type}", hazardCode, PrimaryHazard.HazardType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load Primary Hazard: {HazardCode}", hazardCode);
        }
    }

    private async Task LoadRelatedHazardsAsync()
    {
        try
        {
            var allHazardsQuery = new GetAllHazardsQuery();
            var hazardsResult = await _mediator.SendAsync(allHazardsQuery, CancellationToken.None);
            
            if (hazardsResult.IsSuccess)
            {
                // Filter hazards related to this assessment (same report or related)
                RelatedHazards = hazardsResult.Value
                    .Where(h => Assessment?.IdentifiedHazardIds.Contains(h.Code) ?? false ||
                               h.Code == Assessment?.HazardCode ||
                               h.ReportCode == PrimaryHazard?.ReportCode)
                    .ToList();
                
                _logger.LogInformation("Loaded {Count} related hazards", RelatedHazards.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load related hazards");
            RelatedHazards = new List<Hazard>();
        }
    }

    private async Task LoadSourceReportAsync(string reportCode)
    {
        try
        {
            var reportQuery = new GetReportByIdQuery(new ReportID(reportCode));
            var reportResult = await _mediator.SendAsync(reportQuery, CancellationToken.None);
            
            if (reportResult.IsSuccess)
            {
                SourceReport = reportResult.Value;
                _logger.LogInformation("Loaded Source Report: {ReportId}", reportCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load Source Report: {ReportCode}", reportCode);
        }
    }

    private async Task LoadAirportSharedDatasetAsync()
    {
        try
        {
            var datasetQuery = new GetAllAirportSharedDatasetsQuery();
            var datasetsResult = await _mediator.SendAsync(datasetQuery, CancellationToken.None);
            
            if (datasetsResult.IsSuccess)
            {
                // Find dataset related to this assessment's hazard or report
                SharedDataset = datasetsResult.Value
                    .FirstOrDefault(d => d.HazardCode == Assessment?.HazardCode ||
                                        d.ReportID == PrimaryHazard?.ReportCode);
                
                if (SharedDataset != null)
                {
                    _logger.LogInformation("Loaded AirportSharedDataset: {DatasetId}", SharedDataset.Code);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load AirportSharedDataset");
        }
    }

    #endregion

    #region Form Population and Step Handlers

    /// <summary>
    /// Populates form fields from loaded entities
    /// </summary>
    private void PopulateFormFieldsFromEntities()
    {
        if (Assessment == null) return;

        // Step 1 Data from RiskAssessment
        LeadAssessor = Assessment.LeadAssessorId;
        SystemDescription = Assessment.SystemDescription;
        SystemBoundaries = Assessment.SystemBoundaries;
        SystemPurpose = Assessment.SystemPurpose;
        PersonnelFactors = Assessment.PersonnelFactors;
        EquipmentFactors = Assessment.EquipmentFactors;
        ProcedureFactors = Assessment.ProcedureFactors;
        ResourceFactors = Assessment.ResourceFactors;
        EnvironmentFactors = Assessment.EnvironmentFactors;
        
        // Map to 5M UI properties for compatibility
        FiveMPersonnel = Assessment.PersonnelFactors;
        FiveMEquipment = Assessment.EquipmentFactors;
        FiveMProcedures = Assessment.ProcedureFactors;
        FiveMResources = Assessment.ResourceFactors;
        FiveMPhysicalEnvironment = Assessment.EnvironmentFactors;
        FiveMOperationalEnvironment = Assessment.EnvironmentFactors;
        
        StakeholderIds = Assessment.StakeholderIds.ToList();
        SelectedStakeholderIds = Assessment.StakeholderIds.ToList();

        // Step 2 Data - Identified Hazards
        HazardIds = Assessment.IdentifiedHazardIds.ToList();
        
        // Add primary hazard if not already in list
        if (PrimaryHazard != null && !HazardIds.Contains(PrimaryHazard.Code))
        {
            HazardIds.Insert(0, PrimaryHazard.Code);
            HazardDescriptions.Insert(0, PrimaryHazard.Description);
            HazardCategories.Insert(0, PrimaryHazard.HazardType);
        }

        // Populate identified hazards for UI
        var identifiedHazards = RelatedHazards.Where(h => Assessment.IdentifiedHazardIds.Contains(h.Code))
            .Select(h => new IdentifiedHazardDto
            {
                Id = h.Code,
                Description = h.Description,
                Category = h.HazardType,
                WorstCredibleOutcome = "", // Will be populated from assessment data
                RiskLevel = h.Priority?.ToString() ?? "Medium",
                Severity = "3", // Default values for UI compatibility
                Likelihood = "3",
                Tolerability = "ALARP"
            }).ToList();

        AssessmentData = new AssessmentDataWrapper
        {
            IdentifiedHazards = identifiedHazards,
            ImplementationPlan = Assessment.ImplementationStrategy
        };

        // Step 3 Data from Assessment
        RiskAnalysisMethod = Assessment.RiskAnalysisMethod;
        RiskCriteria = Assessment.RiskCriteria;

        // Step 4 Data
        TolerabilityFramework = Assessment.TolerabilityFramework;
        RiskAcceptanceCriteria = Assessment.RiskAcceptanceCriteria;

        // Step 5 Data
        ImplementationStrategy = Assessment.ImplementationStrategy;
        OverallTargetDate = Assessment.OverallTargetDate;
        ImplementationNotes = Assessment.ImplementationNotes;

        // Initialize stakeholder groups for UI (predefined groups for airport stakeholders)
        StakeholderGroups_Data = new List<StakeholderGroup>
        {
            new() { Id = "ATC", Name = "Air Traffic Control", Description = "ATC Personnel", Members = new List<string> {"ATC_TOWER", "ATC_APPROACH", "ATC_GROUND"} },
            new() { Id = "OPS", Name = "Airport Operations", Description = "Operations Staff", Members = new List<string> {"OPS_MGR", "OPS_COORD", "OPS_TECH"} },
            new() { Id = "MAINT", Name = "Maintenance", Description = "Maintenance Personnel", Members = new List<string> {"MAINT_MGR", "MAINT_TECH", "MAINT_ELEC"} },
            new() { Id = "SMS", Name = "Safety Management", Description = "SMS Team", Members = new List<string> {"SMS_MGR", "SMS_ANALYST", "SMS_COORD"} },
            new() { Id = "FIRE", Name = "Emergency Services", Description = "Fire & Rescue", Members = new List<string> {"FIRE_CHIEF", "FIRE_CAPT", "ARFF_TECH"} },
            new() { Id = "AIRLINE", Name = "Airlines", Description = "Airline Stakeholders", Members = new List<string>() },
            new() { Id = "VENDOR", Name = "Vendors/Contractors", Description = "External Service Providers", Members = new List<string>() }
        };

        // Populate airline and vendor groups with actual stakeholder data
        PopulateStakeholderGroupsFromUsers();

        _logger.LogDebug("Populated form fields from Assessment and related entities");
    }

    /// <summary>
    /// Populates stakeholder groups with actual SMS Stakeholder User data
    /// </summary>
    private void PopulateStakeholderGroupsFromUsers()
    {
        if (!AvailableStakeholders.Any()) return;

        var airlineGroup = StakeholderGroups_Data.FirstOrDefault(g => g.Id == "AIRLINE");
        var vendorGroup = StakeholderGroups_Data.FirstOrDefault(g => g.Id == "VENDOR");

        if (airlineGroup != null)
        {
            airlineGroup.Members = AvailableStakeholders
                .Where(s => s.StakeholderType.Contains("Airline", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.UserId.Value)
                .ToList();
        }

        if (vendorGroup != null)
        {
            vendorGroup.Members = AvailableStakeholders
                .Where(s => s.StakeholderType.Contains("Contractor", StringComparison.OrdinalIgnoreCase) ||
                           s.StakeholderType.Contains("Vendor", StringComparison.OrdinalIgnoreCase) ||
                           s.StakeholderType.Contains("Service Provider", StringComparison.OrdinalIgnoreCase))
                .Select(s => s.UserId.Value)
                .ToList();
        }
    }

    private async Task LoadStepReferenceDataAsync()
    {
        try
        {
            // Load available assessors (SMS Application Users) for Step 1 and Step 4
            if (StepNumber == 1 || StepNumber == 4)
            {
                var assessorsQuery = new GetAllSMSApplicationUsersQuery();
                var assessorsResult = await _mediator.SendAsync(assessorsQuery, CancellationToken.None);
                if (assessorsResult.IsSuccess)
                {
                    AvailableAssessors = assessorsResult.Value.ToList();
                }

                // Load SMS Stakeholder Users for stakeholder selection
                var stakeholdersQuery = new GetAllSMSStakeholderUsersQuery();
                var stakeholdersResult = await _mediator.SendAsync(stakeholdersQuery, CancellationToken.None);
                if (stakeholdersResult.IsSuccess)
                {
                    AvailableStakeholders = stakeholdersResult.Value.ToList();
                    _logger.LogInformation("Loaded {Count} SMS Stakeholder Users for stakeholder selection", 
                        AvailableStakeholders.Count);
                }
            }

            _logger.LogDebug("Loaded reference data for Step {StepNumber}", StepNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reference data for Step {StepNumber}", StepNumber);
        }
    }

    #endregion

    #region Step Save Handlers - CQRS Integration

    public async Task<IActionResult> OnPostSaveStep1Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 1 for Assessment {AssessmentId}", Id);

            if (Assessment == null)
            {
                return new JsonResult(new { success = false, message = "Assessment not found" });
            }

            // Update assessment using Domain Entity methods
            var updateResult = Assessment.UpdateSystemDescription(
                SystemDescription,
                SystemBoundaries,
                SystemPurpose,
                PersonnelFactors,
                EquipmentFactors,
                ProcedureFactors,
                ResourceFactors,
                EnvironmentFactors
            );

            if (updateResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = updateResult.Error.Message });
            }

            // Update stakeholders
            foreach (var stakeholderId in StakeholderIds)
            {
                Assessment.AddStakeholder(stakeholderId);
            }

            // Mark Step 1 as completed and save
            Assessment.CompleteStep(1);

            var saveCommand = new UpdateRiskAssessmentCommand(Assessment);
            var saveResult = await _mediator.SendAsync(saveCommand, CancellationToken.None);

            if (saveResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = saveResult.Error?.Message });
            }

            _logger.LogInformation("Step 1 saved successfully for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = true, message = "Step 1 saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 1 for Assessment {AssessmentId}", Id);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep2Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 2 for Assessment {AssessmentId}", Id);

            if (Assessment == null)
            {
                return new JsonResult(new { success = false, message = "Assessment not found" });
            }

            // Clear existing identified hazards
            Assessment.ClearIdentifiedHazards();

            // Process identified hazards
            for (int i = 0; i < HazardIds.Count; i++)
            {
                if (i < HazardDescriptions.Count && !string.IsNullOrWhiteSpace(HazardDescriptions[i]))
                {
                    Assessment.AddIdentifiedHazard(HazardIds[i], HazardDescriptions[i]);
                }
            }

            // Mark Step 2 as completed and save
            Assessment.CompleteStep(2);

            var saveCommand = new UpdateRiskAssessmentCommand(Assessment);
            var saveResult = await _mediator.SendAsync(saveCommand, CancellationToken.None);

            if (saveResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = saveResult.Error?.Message });
            }

            _logger.LogInformation("Step 2 saved successfully with {HazardCount} hazards", HazardIds.Count);
            return new JsonResult(new { success = true, message = $"Step 2 saved with {HazardIds.Count} hazards" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 2: {Error}", ex.Message);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSaveStep3Async()
    {
        try
        {
            _logger.LogInformation("Saving Step 3 for Assessment {AssessmentId}", Id);

            if (Assessment == null)
            {
                return new JsonResult(new { success = false, message = "Assessment not found" });
            }

            // Update risk analysis method
            Assessment.UpdateRiskAnalysisMethod(RiskAnalysisMethod, RiskCriteria);

            // Update hazard worst outcomes and root causes
            foreach (var hazardId in Assessment.IdentifiedHazardIds)
            {
                if (HazardWorstOutcomes.ContainsKey(hazardId))
                {
                    Assessment.UpdateHazardWorstOutcome(hazardId, HazardWorstOutcomes[hazardId]);
                }
                if (HazardRootCauses.ContainsKey(hazardId))
                {
                    Assessment.UpdateHazardRootCause(hazardId, HazardRootCauses[hazardId]);
                }
            }

            // Mark Step 3 as completed and save
            Assessment.CompleteStep(3);

            var saveCommand = new UpdateRiskAssessmentCommand(Assessment);
            var saveResult = await _mediator.SendAsync(saveCommand, CancellationToken.None);

            if (saveResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = saveResult.Error?.Message });
            }

            _logger.LogInformation("Step 3 saved successfully for assessment {AssessmentId}", Id);
            return new JsonResult(new { success = true, message = "Step 3 saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving Step 3: {Error}", ex.Message);
            return new JsonResult(new { success = false, message = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostCompleteAssessmentAsync()
    {
        try
        {
            _logger.LogInformation("Completing Assessment {AssessmentId}", Id);

            if (Assessment == null)
            {
                TempData["ErrorMessage"] = "Assessment not found";
                return Page();
            }

            // Mark assessment as completed using Domain Entity
            var completeResult = Assessment.MarkAsCompleted();

            if (completeResult.IsFailure)
            {
                TempData["ErrorMessage"] = completeResult.Error.Message;
                return Page();
            }

            // Save completed assessment
            var saveCommand = new UpdateRiskAssessmentCommand(Assessment);
            var saveResult = await _mediator.SendAsync(saveCommand, CancellationToken.None);

            if (saveResult.IsFailure)
            {
                TempData["ErrorMessage"] = saveResult.Error?.Message;
                return Page();
            }

            _logger.LogInformation("Assessment {AssessmentId} completed successfully", Id);
            TempData["SuccessMessage"] = "Risk Assessment completed successfully! All steps have been saved.";
            
            return RedirectToPage("/SafetyRiskManagement/ReportProcessing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing Assessment {AssessmentId}", Id);
            TempData["ErrorMessage"] = $"Error completing assessment: {ex.Message}";
            return Page();
        }
    }

    #endregion

    #region Navigation Handlers

    public async Task<IActionResult> OnPostNextStepAsync()
    {
        var nextStep = Math.Min(StepNumber + 1, 5);
        return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
            new { id = Id, stepNumber = nextStep, hazardId = HazardId });
    }

    public async Task<IActionResult> OnPostPreviousStepAsync()
    {
        var previousStep = Math.Max(StepNumber - 1, 1);
        return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
            new { id = Id, stepNumber = previousStep, hazardId = HazardId });
    }

    #endregion

    #region UI Helper Methods - Preserve Existing Interface

    public string GetStepName(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "System Description",
            2 => "Hazard Identification", 
            3 => "Risk Analysis",
            4 => "Risk Assessment",
            5 => "Risk Mitigation",
            _ => "Unknown Step"
        };
    }

    public string GetStepIcon(int stepNumber)
    {
        return stepNumber switch
        {
            1 => "fas fa-cogs",
            2 => "fas fa-exclamation-triangle",
            3 => "fas fa-chart-line",
            4 => "fas fa-balance-scale",
            5 => "fas fa-shield-alt",
            _ => "fas fa-question"
        };
    }

    public bool CanSkipToStep(int targetStep)
    {
        return Assessment?.IsStepCompleted(targetStep - 1) ?? (targetStep == 1);
    }

    public int GetLastCompletedStep()
    {
        return Assessment?.CompletedSteps.LastOrDefault() ?? 0;
    }

    public int GetRecommendedStep()
    {
        return Assessment?.GetNextRecommendedStep() ?? 1;
    }

    public string GetAssessmentName()
    {
        return Assessment?.Name ?? "Risk Assessment";
    }

    public string AssessmentName => GetAssessmentName();
    public string AssessmentId => Id;
    public string LeadAssessorName => AvailableAssessors.FirstOrDefault(a => a.Id.Value == LeadAssessor)?.DisplayName ?? LeadAssessor;
    public int CurrentStep => StepNumber;
    public string CurrentStepName => GetStepName(StepNumber);
    public int IdentifiedHazardsCount => Assessment?.IdentifiedHazardIds.Count ?? RelatedHazards.Count;

    #endregion
}
