using Microsoft.Extensions.Logging;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Services;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.Errors;
using SMS_Domain.ValueObjects;

using SMS_Infrastructure.Common;
using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Services;

using SMS_Shared.Common;

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
            _logger.LogInformation("??? SMS Backend: Creating new hazard - {Name}", request.Hazard.Name);
           
            Hazard hazard = request.Hazard;
            hazard.ReportCode = request.Hazard.ReportCode;

            var hazardResult = await _hazardDataService.CreateHazardAsync(hazard, ct);
            if (hazardResult.IsFailure)
            {
                _logger.LogApplicationError($"{_logheader} SMS Backend: Failed to save hazard via both data service", ApplicationEventIds.Error, null);
                return Result<Hazard>.Failure<Hazard>(hazardResult.Error);
            }
            else
            {
                hazard = hazardResult.Value;
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

                var createdLocationResult = await _locationDataService.CreateHazardLocationAsync(hazardLocation,ct);
                hazard.HazardLocation = createdLocationResult.Value;

                RiskAssessment initialRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"));
                initialRiskAssessment.HazardCode = hazard.Code;
                initialRiskAssessment.AssessmentType = RiskAssessmentType.Initial;
                initialRiskAssessment.CurrentStep = 0;
                // BUSINESS RULE: Initial assessment MUST have PrimaryHazardId set to currentHazardId
                initialRiskAssessment.PrimaryHazardId = hazard.Code;
                // BUSINESS RULE: Default to Technical category unless specified otherwise
                initialRiskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
                initialRiskAssessment.Description = hazard.Description;
                initialRiskAssessment.Name = $"Risk Assessment - {hazard.Name}";
                var riskAssessmentResult = _riskAssessmentDataService.CreateRiskAssessmentAsync(initialRiskAssessment, ct);
                var initialRiskAccessmentCode = riskAssessmentResult.Result.Value.Code;

                RiskAssessment residualRiskAssessment = new RiskAssessment(new RiskAssessmentID("RS-0000"));
                residualRiskAssessment.HazardCode = hazard.Code;
                residualRiskAssessment.AssessmentType = RiskAssessmentType.Residual;
                residualRiskAssessment.Description = hazard.Description;
                // BUSINESS RULE: Residual assessments can have same PrimaryHazardId as initial
                residualRiskAssessment.Name = $"Risk Assessment - {hazard.Name}";
                residualRiskAssessment.PrimaryHazardId = hazard.Code;
                residualRiskAssessment.RiskAssessmentCategory = RiskAssessmentCategory.Technical;
                riskAssessmentResult = _riskAssessmentDataService.CreateRiskAssessmentAsync(residualRiskAssessment, ct);

                RiskAnalysis riskanalysis = new RiskAnalysis(new RiskAnalysisID("RA-0000"));
                riskanalysis.HazardCode= hazard.Code;
                riskanalysis.RiskAssessmentCode = initialRiskAccessmentCode;
                var riskanalysisresult  = _riskAnalysisDataService.CreateRiskAnalysisAsync(riskanalysis, ct); 

            }
                        
            
            _logger.LogApplicationInformation(ApplicationEventIds.Information,"? SMS Backend: Hazard saved via data service - {Code}", hazard.Code);
            return Result<Hazard>.Success(hazardResult.Value);
        
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("? SMS Backend: Exception creating hazard - {Name}", ApplicationEventIds.Error, ex);
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CreateFailed); // Use the correct error constant
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