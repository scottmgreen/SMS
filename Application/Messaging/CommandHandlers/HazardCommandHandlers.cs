using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// HAZARD COMMAND HANDLERS - SMS Backend Integration
// =============================================

/// <summary>
/// Command handler for creating hazards using individual properties
/// Used by HazardReporting page for direct SMS Backend integration
/// </summary>
public class CreateHazardCommandHandler : BaseCommandBundle, IRequestHandler<CreateHazardCommand, Result<Hazard>>
{
    private readonly HazardDataService _hazardDataService;
    private readonly ScoringPanelDataService _scoringPanelDataService;
    private readonly HazardLocationDataService _locationDataService;
    private readonly RiskAssessmentDataService _riskAssessmentDataService;
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    private readonly ILogger<CreateHazardCommandHandler> _logger;
    private readonly ILogSupport _logsupport;
    private readonly string _logheader = string.Empty;

    public CreateHazardCommandHandler(
        HazardDataService hazardDataService, ScoringPanelDataService scoringPanelDataService,
        HazardLocationDataService locationDataService,
        RiskAssessmentDataService riskAssessmentDataService, RiskAnalysisDataService riskAnalysisDataService, ILogSupport logsupport,
    ILogger<CreateHazardCommandHandler> logger)
    {
        _hazardDataService = hazardDataService ?? throw new ArgumentNullException(nameof(hazardDataService));
        _scoringPanelDataService = scoringPanelDataService ?? throw new ArgumentNullException(nameof(scoringPanelDataService));
        _locationDataService = locationDataService ?? throw new ArgumentNullException(nameof(locationDataService));
        _riskAssessmentDataService = riskAssessmentDataService ?? throw new ArgumentNullException(nameof(riskAssessmentDataService));
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
        _logsupport = logsupport;
        _logheader = _logsupport.GenerateLogHeader();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(CreateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("? SMS Backend: Creating new hazard - {Name} for Report: {ReportCode}", 
                request.Hazard.Name, request.Hazard.ReportCode);

            Hazard hazard = request.Hazard;
            hazard.ReportCode = request.Hazard.ReportCode;

            var hazardResult = await _hazardDataService.CreateHazardAsync(hazard, ct);
            if (hazardResult.IsFailure)
            {
                _logger.LogApplicationError($"{_logheader} SMS Backend: Failed to save hazard via data service", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
            }
            else
            {
                hazard = hazardResult.Value;
                
                // Create scoring panel and location as before
                var scoringPanel = new ScoringPanel(new ScoringPanelID("SP-0000"))
                {
                    HazardCode = hazard.Code
                };

                var hazardLocation = new HazardLocation(new HazardLocationID("HL-0000"))
                {
                    HazardCode = hazard.Code,
                    Latitude = 0,
                    Longitude = 0,
                    Description = "Map selected location"
                };

                var createdLocationResult = await _locationDataService.CreateHazardLocationAsync(hazardLocation, ct);
                hazard.HazardLocation = createdLocationResult.Value;

                // ? CRITICAL FIX: Check for existing RiskAssessments for this report first
                var existingAssessments = await FindExistingRiskAssessmentsForReport(hazard.ReportCode, ct);
                
                string initialRiskAssessmentCode;
                string residualRiskAssessmentCode;
                
                if (existingAssessments.InitialAssessment != null && existingAssessments.ResidualAssessment != null)
                {
                    // ? Use existing RiskAssessments - don't create new ones!
                    initialRiskAssessmentCode = existingAssessments.InitialAssessment.Code;
                    residualRiskAssessmentCode = existingAssessments.ResidualAssessment.Code;
                    
                    _logger.LogInformation("? REUSING existing RiskAssessments for Report {ReportCode}: Initial={InitialCode}, Residual={ResidualCode}",
                        hazard.ReportCode, initialRiskAssessmentCode, residualRiskAssessmentCode);
                }
                else
                {
                    // ? Create NEW RiskAssessments only if none exist (first hazard in the report)
                    var (initialCode, residualCode) = await CreateRiskAssessmentsForHazard(hazard, ct);
                    initialRiskAssessmentCode = initialCode;
                    residualRiskAssessmentCode = residualCode;
                    
                    _logger.LogInformation("? Created NEW RiskAssessments for Report {ReportCode}: Initial={InitialCode}, Residual={ResidualCode}",
                        hazard.ReportCode, initialRiskAssessmentCode, residualRiskAssessmentCode);
                }

                // ? ALWAYS create RiskAnalysis records linking this hazard to the assessments
                await CreateRiskAnalysisForHazard(hazard.Code, initialRiskAssessmentCode, residualRiskAssessmentCode, ct);

                _logger.LogInformation("? Created RiskAnalyses for Hazard: {HazardCode} linked to assessments", hazard.Code);
                _logger.LogInformation("    ?? Initial Assessment: {InitialCode}", initialRiskAssessmentCode);
                _logger.LogInformation("    ?? Residual Assessment: {ResidualCode}", residualRiskAssessmentCode);
            }

            _logger.LogApplicationInformation(ApplicationEventIds.Information, "? SMS Backend: Hazard saved via data service - {Code}", hazard.Code);
            return Result<Hazard>.Success(hazardResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("? SMS Backend: Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed);
        }
    }

    /// <summary>
    /// Create RiskAssessments for this specific hazard
    /// Note: This creates per-hazard assessments (current design)
    /// TODO: Future enhancement - create per-report assessments
    /// </summary>
    private async Task<(string InitialCode, string ResidualCode)> CreateRiskAssessmentsForHazard(Hazard hazard, CancellationToken ct)
    {
        // Create Initial RiskAssessment for this hazard
        RiskAssessment initialRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"));
        initialRiskAssessment.HazardCode = hazard.Code;
        initialRiskAssessment.AssessmentType = RiskAssessmentType.Initial;
        initialRiskAssessment.CurrentStep = 1;
        initialRiskAssessment.PrimaryHazardId = hazard.Code;
        initialRiskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
        initialRiskAssessment.Description = $"Initial Risk Assessment for Hazard {hazard.Code} in Report {hazard.ReportCode}";
        initialRiskAssessment.Name = $"Initial Risk Assessment - {hazard.Code}";
        
        var initialResult = await _riskAssessmentDataService.CreateRiskAssessmentAsync(initialRiskAssessment, ct);
        var initialCode = initialResult.Value.Code;

        // Create Residual RiskAssessment for this hazard
        RiskAssessment residualRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"));
        residualRiskAssessment.HazardCode = hazard.Code;
        residualRiskAssessment.AssessmentType = RiskAssessmentType.Residual;
        residualRiskAssessment.PrimaryHazardId = hazard.Code;
        residualRiskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
        residualRiskAssessment.Description = $"Residual Risk Assessment for Hazard {hazard.Code} in Report {hazard.ReportCode}";
        residualRiskAssessment.Name = $"Residual Risk Assessment - {hazard.Code}";
        
        var residualResult = await _riskAssessmentDataService.CreateRiskAssessmentAsync(residualRiskAssessment, ct);
        var residualCode = residualResult.Value.Code;

        return (initialCode, residualCode);
    }

    /// <summary>
    /// Create RiskAnalysis records linking this hazard to its RiskAssessments
    /// </summary>
    private async Task CreateRiskAnalysisForHazard(string hazardCode, string initialAssessmentCode, string residualAssessmentCode, CancellationToken ct)
    {
        // Create Initial RiskAnalysis (used in Step 3)
        RiskAnalysis initialRiskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"));
        initialRiskAnalysis.HazardCode = hazardCode;
        initialRiskAnalysis.RiskAssessmentCode = initialAssessmentCode;
        await _riskAnalysisDataService.CreateRiskAnalysisAsync(initialRiskAnalysis, ct);

        // Create Residual RiskAnalysis (used in Step 5)
        RiskAnalysis residualRiskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"));
        residualRiskAnalysis.HazardCode = hazardCode;
        residualRiskAnalysis.RiskAssessmentCode = residualAssessmentCode;
        await _riskAnalysisDataService.CreateRiskAnalysisAsync(residualRiskAnalysis, ct);
        
        _logger.LogInformation("? Created RiskAnalyses for Hazard {HazardCode}:", hazardCode);
        _logger.LogInformation("    ?? Initial RiskAnalysis for Assessment: {InitialCode}", initialAssessmentCode);
        _logger.LogInformation("    ?? Residual RiskAnalysis for Assessment: {ResidualCode}", residualAssessmentCode);
    }

    /// <summary>
    /// ? CRITICAL METHOD: Find existing RiskAssessments for this report using proper code-based matching
    /// This prevents creating duplicate RiskAssessments when adding hazards to existing reports
    /// </summary>
    private async Task<(RiskAssessment? InitialAssessment, RiskAssessment? ResidualAssessment)> FindExistingRiskAssessmentsForReport(
        string reportCode, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("?? Searching for existing RiskAssessments for Report: {ReportCode} using CODE-based matching", reportCode);
            
            // STEP 1: Get hazards for this specific report using proper method
            var reportHazardsResult = await _hazardDataService.GetHazardsByReportIdAsync(new ReportID(reportCode), ct);
            
            if (reportHazardsResult.IsSuccess && reportHazardsResult.Value?.Any() == true)
            {
                var reportHazards = reportHazardsResult.Value;
                _logger.LogInformation("?? Found {Count} existing hazards for Report {ReportCode}", reportHazards.Count, reportCode);
                
                // Get hazard codes for this report
                var reportHazardCodes = reportHazards.Select(h => h.Code).ToList();
                _logger.LogInformation("?? Hazard codes for Report {ReportCode}: {HazardCodes}", reportCode, string.Join(", ", reportHazardCodes));
                
                // STEP 2: Get all RiskAssessments and find any linked to these hazards
                var allAssessmentsResult = await _riskAssessmentDataService.GetAllRiskAssessmentsAsync(ct);
                
                if (allAssessmentsResult.IsSuccess && allAssessmentsResult.Value?.Any() == true)
                {
                    _logger.LogInformation("?? Found {Count} total RiskAssessments in database", allAssessmentsResult.Value.Count);

                    // Find RiskAssessments linked to these hazards
                    var reportAssessments = allAssessmentsResult.Value
                        .Where(assessment => 
                            !string.IsNullOrEmpty(assessment.HazardCode) && 
                            reportHazardCodes.Contains(assessment.HazardCode))
                        .ToList();

                    _logger.LogInformation("?? Found {Count} RiskAssessments linked to hazards from Report {ReportCode}", 
                        reportAssessments.Count, reportCode);

                    // Log details of found assessments
                    foreach (var assessment in reportAssessments)
                    {
                        _logger.LogInformation("  ?? Assessment {Code}: Type={Type}, HazardCode={HazardCode}", 
                            assessment.Code, assessment.AssessmentType, assessment.HazardCode);
                    }

                    if (reportAssessments.Any())
                    {
                        var initialAssessment = reportAssessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
                        var residualAssessment = reportAssessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Residual);

                        if (initialAssessment != null && residualAssessment != null)
                        {
                            _logger.LogInformation("? Found BOTH existing RiskAssessments for Report {ReportCode}: Initial={InitialCode}, Residual={ResidualCode}",
                                reportCode, initialAssessment.Code, residualAssessment.Code);
                            return (initialAssessment, residualAssessment);
                        }
                        else if (initialAssessment != null || residualAssessment != null)
                        {
                            _logger.LogWarning("?? Found PARTIAL RiskAssessments for Report {ReportCode}: Initial={InitialCode}, Residual={ResidualCode}",
                                reportCode, initialAssessment?.Code ?? "NULL", residualAssessment?.Code ?? "NULL");
                        }
                        else
                        {
                            _logger.LogWarning("?? Found {Count} assessments linked to report hazards but none are Initial or Residual type", 
                                reportAssessments.Count);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("? Failed to get RiskAssessments from database or no assessments found");
                }
            }
            else
            {
                _logger.LogInformation("?? No existing hazards found for Report {ReportCode} - this must be the first hazard", reportCode);
            }

            _logger.LogInformation("? No existing RiskAssessments found for Report {ReportCode} - will create new ones", reportCode);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error finding existing RiskAssessments for report {ReportCode}", reportCode);
            return (null, null);
        }
    }
}


public class UpdateHazardCommandHandler : BaseCommandBundle, IRequestHandler<UpdateHazardCommand, Result<Hazard>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<UpdateHazardCommandHandler> _logger;

    public UpdateHazardCommandHandler(HazardDataService dataService, ILogger<UpdateHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Hazard>> HandleAsync(UpdateHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.Hazard is null)
            {
                _logger.LogApplicationError("UpdateHazardCommand received with null Hazard", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateHazardCommand for ID: {Id}, Code: {Code}",
                request.Hazard.Id, request.Hazard.Code);

            var result = await _dataService.UpdateHazardAsync(request.Hazard, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully updated Hazard with ID: {Id}", request.Hazard.Id);
            }
            else
            {
                _logger.LogApplicationError("Failed to update Hazard with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating Hazard with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.UpdateFailed);
        }
    }
}

public class DeleteHazardCommandHandler : BaseCommandBundle, IRequestHandler<DeleteHazardCommand, Result<bool>>
{
    private readonly HazardDataService _dataService;
    private readonly ILogger<DeleteHazardCommandHandler> _logger;

    public DeleteHazardCommandHandler(HazardDataService dataService, ILogger<DeleteHazardCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteHazardCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.HazardId is null)
            {
                _logger.LogApplicationError("DeleteHazardCommand received with null HazardId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.HazardError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteHazardCommand for ID: {Id}", request.HazardId);

            var result = await _dataService.DeleteHazardAsync(request.HazardId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted Hazard with ID: {Id}", request.HazardId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete Hazard with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteHazardCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting Hazard with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.DeleteFailed);
        }
    }
}