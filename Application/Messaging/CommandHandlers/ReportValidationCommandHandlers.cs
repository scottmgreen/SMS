using Microsoft.Extensions.Logging;

using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Services;

namespace SMS_Application.Messaging.CommandHandlers;

// =============================================
// REPORT VALIDATION COMMAND HANDLERS
// =============================================

public class CreateReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<CreateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _dataService;
    private readonly ILogger<CreateReportValidationCommandHandler> _logger;
    private readonly HazardDataService _hazardDataService;
    private readonly RiskAssessmentDataService _riskAssessmentDataService;
    private readonly RiskAnalysisDataService _riskAnalysisDataService;
    public CreateReportValidationCommandHandler(ReportValidationDataService dataService, HazardDataService hazardDataService,
        RiskAssessmentDataService riskAssessmentDataService,
        RiskAnalysisDataService riskAnalysisDataService, ILogger<CreateReportValidationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hazardDataService = hazardDataService;
        _riskAssessmentDataService = riskAssessmentDataService ?? throw new ArgumentNullException(nameof(riskAssessmentDataService));
        _riskAnalysisDataService = riskAnalysisDataService ?? throw new ArgumentNullException(nameof(riskAnalysisDataService));
    }

    public async Task<Result<ReportValidation>> HandleAsync(CreateReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogApplicationError("CreateReportValidationCommand received with null ReportValidation", ApplicationEventIds.Error, null);
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("Processing CreateReportValidationCommand for Code: {Code}", request.ReportValidation.Code);

            var result = await _dataService.CreateReportValidationAsync(request.ReportValidation, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully created ReportValidation with ID: {Id}, Code: {Code}",result.Value?.Id, result.Value?.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to create ReportValidation with Code: {Code}. Error: {Error}",ApplicationEventIds.Error, null);
            }

            //IF SMS_RISK Create the RiskAssessments and RiskAnalysis
            //This was moved from AddHazard Command Handler.

            if (request.ReportValidation.ValidationDecision == ValidationDecision.SmsRisk.Value)
            {
                var hazards = _hazardDataService.GetHazardsByReportIdAsync(new ReportID(request.ReportValidation.ReportCode));
                var hazard = hazards.Result.Value.FirstOrDefault();
                if (hazards == null)
                {
                    _logger.LogApplicationError("Failed to Find Hazard Code: {Code}. Error: {Error}", ApplicationEventIds.Error, null);
                }
                else
                {
                    // ? CORRECT DESIGN: Check for existing RiskAssessments for this report first
                    var existingAssessments = await FindExistingRiskAssessmentsForReport(result.Value.ReportCode, ct);

                    string initialRiskAssessmentCode;
                    string residualRiskAssessmentCode;

                    if (existingAssessments.InitialAssessment != null && existingAssessments.ResidualAssessment != null)
                    {
                        // ? Use existing shared RiskAssessments - perfect for multiple hazards per report!
                        initialRiskAssessmentCode = existingAssessments.InitialAssessment.Code;
                        residualRiskAssessmentCode = existingAssessments.ResidualAssessment.Code;

                    }
                    else
                    {
                        // ? Create NEW shared RiskAssessments only if none exist (first hazard in the report)
                        var initialCode = await CreateRiskAssessmentsForReport(hazard, ct);
                        initialRiskAssessmentCode = initialCode;
                        //residualRiskAssessmentCode = residualCode;

                    }

                    // ? ALWAYS create RiskAnalysis records linking this hazard to the shared assessments
                    await CreateRiskAnalysisForHazard(hazard.Code, initialRiskAssessmentCode, ct);





                }
            }


            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("CreateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while creating ReportValidation", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.CreateFailed);
        }
    }

    /// <summary>
    /// Create RiskAnalysis records linking this hazard to its RiskAssessments
    /// </summary>
    private async Task CreateRiskAnalysisForHazard(string hazardCode, string initialAssessmentCode, CancellationToken ct)
    {
        // Create Initial RiskAnalysis (used in Step 3)
        RiskAnalysis initialRiskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"));
        initialRiskAnalysis.AssessmentType = RiskAnalysisType.Initial;
        initialRiskAnalysis.HazardCode = hazardCode;
        initialRiskAnalysis.RiskAssessmentCode = initialAssessmentCode;
        await _riskAnalysisDataService.CreateRiskAnalysisAsync(initialRiskAnalysis, ct);

        // Create Residual RiskAnalysis (used in Step 5)
        //RiskAnalysis residualRiskAnalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"));
        //residualRiskAnalysis.AssessmentType = RiskAnalysisType.Residual;
        //residualRiskAnalysis.HazardCode = hazardCode;
        //residualRiskAnalysis.RiskAssessmentCode = residualAssessmentCode;
        //await _riskAnalysisDataService.CreateRiskAnalysisAsync(residualRiskAnalysis, ct);

    }

    /// <summary>
    /// Find existing RiskAssessments for this report using proper code-based matching
    /// This prevents creating duplicate RiskAssessments when adding hazards to existing reports
    /// </summary>
    private async Task<(RiskAssessment? InitialAssessment, RiskAssessment? ResidualAssessment)> FindExistingRiskAssessmentsForReport(string reportCode, CancellationToken ct)
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
                        _logger.LogInformation("  ?? Assessment {Code}: Type={Type}, HazardCode={HazardCode}",assessment.Code, assessment.AssessmentType, assessment.HazardCode);
                    }

                    if (reportAssessments.Any())
                    {
                        var initialAssessment = reportAssessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Initial);
                        //var residualAssessment = reportAssessments.FirstOrDefault(x => x.AssessmentType == RiskAssessmentType.Residual);

                        //if (initialAssessment != null && residualAssessment != null)
                        //{
                        //    _logger.LogInformation("? Found BOTH existing RiskAssessments for Report {ReportCode}: Initial={InitialCode}, Residual={ResidualCode}", reportCode, initialAssessment.Code, residualAssessment.Code);
                        //    return (initialAssessment, residualAssessment);
                        //}
                        //else if (initialAssessment != null || residualAssessment != null)
                        //{
                        //    _logger.LogWarning("?? Found PARTIAL RiskAssessments for Report {ReportCode}: Initial={InitialCode}, Residual={ResidualCode}", reportCode, initialAssessment?.Code ?? "NULL", residualAssessment?.Code ?? "NULL");
                        //}
                        //else
                        //{
                        //    _logger.LogWarning("?? Found {Count} assessments linked to report hazards but none are Initial or Residual type", reportAssessments.Count);
                        //}
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

    /// <summary>
    /// Create shared RiskAssessments for this report (first hazard creates them, subsequent hazards reuse them)
    /// </summary>
    private async Task<string> CreateRiskAssessmentsForReport(Hazard hazard, CancellationToken ct)
    {
        // Create Initial RiskAssessment for this report (shared by all hazards)
        RiskAssessment initialRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"));
        initialRiskAssessment.HazardCode = hazard.Code;
        initialRiskAssessment.AssessmentType = RiskAssessmentType.Initial;
        initialRiskAssessment.CurrentStep = 1;
        initialRiskAssessment.PrimaryHazardId = hazard.Code;
        initialRiskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
        initialRiskAssessment.Description = $"Initial Risk Assessment for Report {hazard.ReportCode}";
        initialRiskAssessment.Name = $"Initial Risk Assessment - {hazard.ReportCode}";

        var initialResult = await _riskAssessmentDataService.CreateRiskAssessmentAsync(initialRiskAssessment, ct);
        var initialCode = initialResult.Value.Code;

        //// Create Residual RiskAssessment for this report (shared by all hazards)
        //RiskAssessment residualRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"));
        //residualRiskAssessment.HazardCode = hazard.Code;
        //residualRiskAssessment.AssessmentType = RiskAssessmentType.Residual;
        //residualRiskAssessment.PrimaryHazardId = hazard.Code;
        //residualRiskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
        //residualRiskAssessment.Description = $"Residual Risk Assessment for Report {hazard.ReportCode}";
        //residualRiskAssessment.Name = $"Residual Risk Assessment - {hazard.ReportCode}";

        //var residualResult = await _riskAssessmentDataService.CreateRiskAssessmentAsync(residualRiskAssessment, ct);
        //var residualCode = residualResult.Value.Code;

        return (initialCode);
    }
}

public class UpdateReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<UpdateReportValidationCommand, Result<ReportValidation>>
{
    private readonly ReportValidationDataService _dataService;
    
    private readonly ILogger<UpdateReportValidationCommandHandler> _logger;

    public UpdateReportValidationCommandHandler(ReportValidationDataService dataService,ILogger<UpdateReportValidationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
    }

    public async Task<Result<ReportValidation>> HandleAsync(UpdateReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidation is null)
            {
                _logger.LogApplicationError("UpdateReportValidationCommand received with null ReportValidation", ApplicationEventIds.Error, null);
                return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("Processing UpdateReportValidationCommand for Code: {Code}",  request.ReportValidation.Code);

            var updateresult = await _dataService.UpdateReportValidationAsync(request.ReportValidation, ct).ConfigureAwait(false);

            if (updateresult.IsSuccess)
            {
                _logger.LogInformation("Successfully updated ReportValidation with Code: {Code}", request.ReportValidation.Code);
            }
            else
            {
                _logger.LogApplicationError("Failed to update ReportValidation with Code: {Code}. Error: {Error}", ApplicationEventIds.Error, null);
            }


            








            return updateresult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("UpdateReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while updating ReportValidation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<ReportValidation>.Failure<ReportValidation>(DomainErrors.ReportError.UpdateFailed);
        }
    }


    



}

public class DeleteReportValidationCommandHandler : BaseCommandBundle, IRequestHandler<DeleteReportValidationCommand, Result<bool>>
{
    private readonly ReportValidationDataService _dataService;
    private readonly ILogger<DeleteReportValidationCommandHandler> _logger;

    public DeleteReportValidationCommandHandler(ReportValidationDataService dataService, ILogger<DeleteReportValidationCommandHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<bool>> HandleAsync(DeleteReportValidationCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request?.ReportValidationId is null)
            {
                _logger.LogApplicationError("DeleteReportValidationCommand received with null ReportValidationId", ApplicationEventIds.Error, null);
                return Result<bool>.Failure<bool>(DomainErrors.ReportError.NullOrEmpty);
            }

            _logger.LogInformation("Processing DeleteReportValidationCommand for ID: {Id}", request.ReportValidationId);

            var result = await _dataService.DeleteReportValidationAsync(request.ReportValidationId, ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully deleted ReportValidation with ID: {Id}", request.ReportValidationId);
            }
            else
            {
                _logger.LogApplicationError("Failed to delete ReportValidation with ID: {Id}. Error: {Error}",
                    ApplicationEventIds.Error, null);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("DeleteReportValidationCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error occurred while deleting ReportValidation with ID: {Id}", ApplicationEventIds.Error, ex);
            return Result<bool>.Failure<bool>(DomainErrors.ReportError.DeleteFailed);
        }
    }
}