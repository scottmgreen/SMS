using System.ComponentModel.DataAnnotations;
using PDXSMS_Domain.Entities;
using PDXSMS_Domain.Enums;
using PDXSMS_Domain.ValueObjects;
using PDXSMS.UseCases.Queries.HazardQueries;

namespace PDXSMS_Presentation.ViewModels;

/// <summary>
/// Airport Shared Dataset ViewModel - Critical SMS Compliance Data
/// 
/// This contains all the regulatory-required data elements for SMS Risk validation
/// and hazard processing. Every field is important for compliance and must be preserved.
/// 
/// Renamed from MinimumDatasetViewModel to better reflect its critical importance
/// as the Airport Shared Dataset required for SMS compliance and regulatory reporting.
/// 
/// This ViewModel:
/// - Maps cleanly to HazardReportDetail domain entity
/// - Provides UI-friendly validation attributes
/// - Handles type conversion between string inputs and domain enums
/// - Maintains separation between presentation and domain concerns
/// </summary>
public class AirportSharedDatasetViewModel
{
    #region Basic Information
    
    /// <summary>
    /// Internal narrative - populated from original hazard description
    /// </summary>
    public string? PrivateNarrative { get; set; }
    
    /// <summary>
    /// External narrative suitable for stakeholder sharing
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Shared narrative cannot exceed 2000 characters")]
    public string? SharedNarrative { get; set; }
    
    #endregion
    
    #region Location Classification
    
    /// <summary>
    /// Primary location area classification
    /// </summary>
    public string? LocationArea { get; set; }
    
    /// <summary>
    /// Specific sub-area within the primary location
    /// </summary>
    public string? LocationSubArea { get; set; }
    
    /// <summary>
    /// Custom location description when "Other" is selected
    /// </summary>
    public string? LocationOther { get; set; }
    
    #endregion
    
    #region Environmental Context
    
    /// <summary>
    /// Weather conditions during hazard occurrence
    /// </summary>
    public string? Weather { get; set; }
    
    #endregion
    
    #region Event Details
    
    /// <summary>
    /// Primary triggering event classification
    /// </summary>
    public string? TriggeringEvent { get; set; }
    
    #endregion
    
    #region Involved Components
    
    public bool AircraftInvolved { get; set; }
    public bool PoweredEquipmentInvolved { get; set; }
    public bool NonPoweredEquipmentInvolved { get; set; }
    public bool PedestrianInvolved { get; set; }
    public bool OtherInvolved { get; set; }
    
    /// <summary>
    /// Description when "Other" component is involved
    /// </summary>
    public string? OtherDescription { get; set; }
    
    #endregion
    
    #region Resulting Issues
    
    public bool PropertyDamage { get; set; }
    public string? PropertyDamageComments { get; set; }
    
    public bool PersonalInjury { get; set; }
    public string? PersonalInjuryComments { get; set; }
    
    public bool Fatality { get; set; }
    public string? FatalityComments { get; set; }
    
    public bool OtherIssues { get; set; }
    public string? OtherIssuesDescription { get; set; }
    
    #endregion
    
    #region Operational Impact
    
    /// <summary>
    /// Airlines, companies, or operators involved
    /// </summary>
    public string? AirlineCompanyOperator { get; set; }
    
    /// <summary>
    /// Whether operators were authorized (Yes/No/Unknown/N/A)
    /// </summary>
    public string? OperatorsAuthorized { get; set; }
    
    /// <summary>
    /// Whether the incident caused flight delays (Yes/No/N/A)
    /// </summary>
    public string? FlightDelay { get; set; }
    
    /// <summary>
    /// Details about flight delays if they occurred
    /// </summary>
    public string? FlightDelayDetails { get; set; }
    
    /// <summary>
    /// Whether equipment was removed from service (Yes/No/N/A)
    /// </summary>
    public string? EquipmentRemovedFromService { get; set; }
    
    /// <summary>
    /// Details about equipment removal if it occurred
    /// </summary>
    public string? EquipmentRemovalDetails { get; set; }
    
    #endregion
    
    #region Investigation Details
    
    /// <summary>
    /// Whether a police report was taken (Yes/No)
    /// </summary>
    public string? PoliceReport { get; set; }
    
    /// <summary>
    /// Details about the police report
    /// </summary>
    public string? PoliceReportDetails { get; set; }
    
    #endregion
    
    #region Contributing Factors
    
    /// <summary>
    /// List of contributing factor names
    /// </summary>
    public List<string> ContributingFactors { get; set; } = new();
    
    /// <summary>
    /// Description when "Other" contributing factor is selected
    /// </summary>
    public string? FactorsOtherDescription { get; set; }
    
    #endregion
    
    #region Initialization and Mapping Methods
    
    /// <summary>
    /// Initialize ViewModel from existing hazard information
    /// </summary>
    /// <param name="hazard">Hazard summary from query</param>
    public void InitializeFromHazard(GetHazardSummaryQueryResponse hazard)
    {
        ArgumentNullException.ThrowIfNull(hazard);
        
        // Set private narrative from original hazard description
        PrivateNarrative = hazard.Description;
        
        // Other fields start empty for user input
        // This maintains the workflow where detailed data is collected during SMS validation
    }
    
    /// <summary>
    /// Initialize ViewModel from existing domain entity (for editing scenarios)
    /// </summary>
    /// <param name="reportDetail">Existing hazard report detail</param>
    public void InitializeFromDomainEntity(HazardReportDetail reportDetail)
    {
        ArgumentNullException.ThrowIfNull(reportDetail);
        
        // Basic Information
        PrivateNarrative = reportDetail.PrivateNarrative;
        SharedNarrative = reportDetail.SharedNarrative;
        
        // Location Classification
        LocationArea = reportDetail.LocationArea.Name;
        LocationSubArea = reportDetail.LocationSubArea;
        LocationOther = reportDetail.LocationOther;
        
        // Environmental Context
        Weather = reportDetail.Weather?.Name;
        
        // Event Details
        TriggeringEvent = reportDetail.TriggeringEvent.Name;
        
        // Involved Components
        AircraftInvolved = reportDetail.InvolvedComponents.AircraftInvolved;
        PoweredEquipmentInvolved = reportDetail.InvolvedComponents.PoweredEquipmentInvolved;
        NonPoweredEquipmentInvolved = reportDetail.InvolvedComponents.NonPoweredEquipmentInvolved;
        PedestrianInvolved = reportDetail.InvolvedComponents.PedestrianInvolved;
        OtherInvolved = reportDetail.InvolvedComponents.OtherInvolved;
        OtherDescription = reportDetail.InvolvedComponents.OtherDescription;
        
        // Resulting Issues
        PropertyDamage = reportDetail.ResultingIssues.PropertyDamage;
        PropertyDamageComments = reportDetail.ResultingIssues.PropertyDamageComments;
        PersonalInjury = reportDetail.ResultingIssues.PersonalInjury;
        PersonalInjuryComments = reportDetail.ResultingIssues.PersonalInjuryComments;
        Fatality = reportDetail.ResultingIssues.Fatality;
        FatalityComments = reportDetail.ResultingIssues.FatalityComments;
        OtherIssues = reportDetail.ResultingIssues.OtherIssues;
        OtherIssuesDescription = reportDetail.ResultingIssues.OtherIssuesDescription;
        
        // Operational Impact
        AirlineCompanyOperator = reportDetail.OperationalImpact.AirlineCompanyOperator;
        OperatorsAuthorized = reportDetail.OperationalImpact.OperatorsAuthorized;
        FlightDelay = reportDetail.OperationalImpact.FlightDelay;
        FlightDelayDetails = reportDetail.OperationalImpact.FlightDelayDetails;
        EquipmentRemovedFromService = reportDetail.OperationalImpact.EquipmentRemovedFromService;
        EquipmentRemovalDetails = reportDetail.OperationalImpact.EquipmentRemovalDetails;
        
        // Investigation Details
        PoliceReport = reportDetail.InvestigationDetails.PoliceReport;
        PoliceReportDetails = reportDetail.InvestigationDetails.PoliceReportDetails;
        
        // Contributing Factors
        ContributingFactors = reportDetail.ContributingFactors.Select(f => f.Name).ToList();
    }
    
    /// <summary>
    /// Convert ViewModel to domain entity using proper domain methods
    /// This is where the magic happens - clean mapping to domain entities
    /// </summary>
    /// <param name="hazardId">Associated hazard ID</param>
    /// <param name="collectedById">User collecting the data</param>
    /// <returns>Properly constructed domain entity</returns>
    public HazardReportDetail ToDomainEntity(HazardID hazardId, SMSUserID collectedById)
    {
        // Create the domain entity using factory method
        var reportDetail = HazardReportDetail.Create(
            hazardId, 
            collectedById, 
            PrivateNarrative ?? string.Empty);
        
        // Apply updates using domain methods (not direct property setting)
        reportDetail.UpdateSharedNarrative(SharedNarrative ?? string.Empty);
        
        // Map location classification using domain enums
        if (!string.IsNullOrWhiteSpace(LocationArea))
        {
            var locationArea = MapToLocationAreaEnum(LocationArea);
            reportDetail.SetLocationClassification(locationArea, LocationSubArea, LocationOther);
        }

        // Map weather condition using domain enum
        if (!string.IsNullOrWhiteSpace(Weather))
        {
            var weather = MapToWeatherConditionEnum(Weather);
            reportDetail.SetWeatherCondition(weather);
        }

        // Map triggering event using domain enum
        if (!string.IsNullOrWhiteSpace(TriggeringEvent))
        {
            var triggeringEvent = MapToTriggeringEventEnum(TriggeringEvent);
            reportDetail.SetTriggeringEvent(triggeringEvent);
        }

        // Create and set complex value objects
        var involvedComponents = new InvolvedComponents(
            aircraftInvolved: AircraftInvolved,
            poweredEquipmentInvolved: PoweredEquipmentInvolved,
            nonPoweredEquipmentInvolved: NonPoweredEquipmentInvolved,
            pedestrianInvolved: PedestrianInvolved,
            otherInvolved: OtherInvolved,
            otherDescription: OtherDescription);

        reportDetail.UpdateInvolvedComponents(involvedComponents);

        var resultingIssues = new ResultingIssues(
            propertyDamage: PropertyDamage,
            propertyDamageComments: PropertyDamageComments,
            personalInjury: PersonalInjury,
            personalInjuryComments: PersonalInjuryComments,
            fatality: Fatality,
            fatalityComments: FatalityComments,
            otherIssues: OtherIssues,
            otherIssuesDescription: OtherIssuesDescription);

        reportDetail.UpdateResultingIssues(resultingIssues);

        var operationalImpact = new OperationalImpact(
            airlineCompanyOperator: AirlineCompanyOperator,
            operatorsAuthorized: OperatorsAuthorized,
            flightDelay: FlightDelay,
            flightDelayDetails: FlightDelayDetails,
            equipmentRemovedFromService: EquipmentRemovedFromService,
            equipmentRemovalDetails: EquipmentRemovalDetails);

        reportDetail.UpdateOperationalImpact(operationalImpact);

        var investigationDetails = new InvestigationDetails(
            policeReport: PoliceReport,
            policeReportDetails: PoliceReportDetails);

        reportDetail.UpdateInvestigationDetails(investigationDetails);

        // Map contributing factors using domain enums
        var contributingFactors = ContributingFactors?
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .Select(MapToContributingFactorEnum)
            .ToList() ?? new List<ContributingFactor>();

        reportDetail.SetContributingFactors(contributingFactors);

        return reportDetail;
    }
    
    /// <summary>
    /// Convert ViewModel to Application DTO for command handling
    /// </summary>
    public PDXSMS.UseCases.Commands.HazardProcessing.AirportSharedDatasetDto ToApplicationDto()
    {
        return new PDXSMS.UseCases.Commands.HazardProcessing.AirportSharedDatasetDto
        {
            PrivateNarrative = PrivateNarrative,
            SharedNarrative = SharedNarrative,
            LocationArea = LocationArea,
            LocationSubArea = LocationSubArea,
            LocationOther = LocationOther,
            Weather = Weather,
            TriggeringEvent = TriggeringEvent,
            AircraftInvolved = AircraftInvolved,
            PoweredEquipmentInvolved = PoweredEquipmentInvolved,
            NonPoweredEquipmentInvolved = NonPoweredEquipmentInvolved,
            PedestrianInvolved = PedestrianInvolved,
            OtherInvolved = OtherInvolved,
            OtherDescription = OtherDescription,
            PropertyDamage = PropertyDamage,
            PropertyDamageComments = PropertyDamageComments,
            PersonalInjury = PersonalInjury,
            PersonalInjuryComments = PersonalInjuryComments,
            Fatality = Fatality,
            FatalityComments = FatalityComments,
            OtherIssues = OtherIssues,
            OtherIssuesDescription = OtherIssuesDescription,
            AirlineCompanyOperator = AirlineCompanyOperator,
            OperatorsAuthorized = OperatorsAuthorized,
            FlightDelay = FlightDelay,
            FlightDelayDetails = FlightDelayDetails,
            EquipmentRemovedFromService = EquipmentRemovedFromService,
            EquipmentRemovalDetails = EquipmentRemovalDetails,
            PoliceReport = PoliceReport,
            PoliceReportDetails = PoliceReportDetails,
            ContributingFactors = ContributingFactors ?? new List<string>(),
            FactorsOtherDescription = FactorsOtherDescription
        };
    }
    
    #endregion
    
    #region Private Mapping Methods
    
    /// <summary>
    /// Maps string input to LocationArea domain enum
    /// </summary>
    private static PDXSMS_Domain.Enums.LocationArea MapToLocationAreaEnum(string locationArea)
    {
        return locationArea switch
        {
            "Movement Area" => PDXSMS_Domain.Enums.LocationArea.MovementArea,
            "Non-movement Area" => PDXSMS_Domain.Enums.LocationArea.NonMovementArea,
            _ => PDXSMS_Domain.Enums.LocationArea.Other
        };
    }
    
    /// <summary>
    /// Maps string input to WeatherCondition domain enum
    /// </summary>
    private static PDXSMS_Domain.Enums.WeatherCondition MapToWeatherConditionEnum(string weather)
    {
        return weather switch
        {
            "Clear" => PDXSMS_Domain.Enums.WeatherCondition.Clear,
            "Cloudy" => PDXSMS_Domain.Enums.WeatherCondition.Cloudy,
            "Rain" => PDXSMS_Domain.Enums.WeatherCondition.Rain,
            "Snow" => PDXSMS_Domain.Enums.WeatherCondition.Snow,
            "Fog" => PDXSMS_Domain.Enums.WeatherCondition.Fog,
            "High Winds" => PDXSMS_Domain.Enums.WeatherCondition.HighWinds,
            _ => PDXSMS_Domain.Enums.WeatherCondition.Other
        };
    }
    
    /// <summary>
    /// Maps string input to TriggeringEvent domain enum
    /// </summary>
    private static PDXSMS_Domain.Enums.TriggeringEvent MapToTriggeringEventEnum(string triggeringEvent)
    {
        return triggeringEvent switch
        {
            "Aircraft Accident" => PDXSMS_Domain.Enums.TriggeringEvent.AircraftAccident,
            "Aircraft Incident" => PDXSMS_Domain.Enums.TriggeringEvent.AircraftIncident,
            "Surface Incident (Movement)" => PDXSMS_Domain.Enums.TriggeringEvent.SurfaceIncidentMovement,
            "Surface Incident (Non-movement)" => PDXSMS_Domain.Enums.TriggeringEvent.SurfaceIncidentNonMovement,
            "Runway Incursion" => PDXSMS_Domain.Enums.TriggeringEvent.RunwayIncursion,
            "Near Midair Collision (NMAC)" => PDXSMS_Domain.Enums.TriggeringEvent.NearMidairCollision,
            "Biological" => PDXSMS_Domain.Enums.TriggeringEvent.BiologicalHazard,
            "Hazardous Substances" => PDXSMS_Domain.Enums.TriggeringEvent.HazardousSubstances,
            "Physical" => PDXSMS_Domain.Enums.TriggeringEvent.PhysicalHazard,
            "Ergonomic" => PDXSMS_Domain.Enums.TriggeringEvent.ErgonomicHazard,
            "Physiological" => PDXSMS_Domain.Enums.TriggeringEvent.PhysiologicalHazard,
            "FOD" => PDXSMS_Domain.Enums.TriggeringEvent.FOD,
            "Wildlife" => PDXSMS_Domain.Enums.TriggeringEvent.Wildlife,
            _ => PDXSMS_Domain.Enums.TriggeringEvent.Other
        };
    }
    
    /// <summary>
    /// Maps string input to ContributingFactor domain enum
    /// </summary>
    private static PDXSMS_Domain.Enums.ContributingFactor MapToContributingFactorEnum(string factor)
    {
        return factor switch
        {
            "Fatigue" => PDXSMS_Domain.Enums.ContributingFactor.Fatigue,
            "Speed" => PDXSMS_Domain.Enums.ContributingFactor.Speed,
            "Weather" => PDXSMS_Domain.Enums.ContributingFactor.Weather,
            "Lighting" => PDXSMS_Domain.Enums.ContributingFactor.Lighting,
            "Signage/Markings" => PDXSMS_Domain.Enums.ContributingFactor.SignageMarkings,
            "Training" => PDXSMS_Domain.Enums.ContributingFactor.Training,
            "Construction" => PDXSMS_Domain.Enums.ContributingFactor.Construction,
            "Administrative Controls" => PDXSMS_Domain.Enums.ContributingFactor.AdministrativeControls,
            "Human Factors" => PDXSMS_Domain.Enums.ContributingFactor.HumanFactors,
            "Engineering" => PDXSMS_Domain.Enums.ContributingFactor.Engineering,
            "Mechanical" => PDXSMS_Domain.Enums.ContributingFactor.Mechanical,
            _ => PDXSMS_Domain.Enums.ContributingFactor.Other
        };
    }
    
    #endregion
    
    #region Validation Helper Methods
    
    /// <summary>
    /// Validates the ViewModel state before domain entity creation
    /// </summary>
    /// <returns>True if valid for domain entity creation</returns>
    public bool IsValidForDomainMapping()
    {
        // All fields are now completely optional - no validation required
        return true;
    }
    
    /// <summary>
    /// Gets validation error messages for the current state
    /// </summary>
    /// <returns>List of validation error messages</returns>
    public List<string> GetValidationErrors()
    {
        // All fields are now completely optional - no validation errors
        return new List<string>();
    }
    
    #endregion
    
    #region UI Helper Properties
    
    /// <summary>
    /// Checks if any components are involved (for UI display logic)
    /// </summary>
    public bool HasInvolvedComponents => 
        AircraftInvolved || PoweredEquipmentInvolved || NonPoweredEquipmentInvolved || 
        PedestrianInvolved || OtherInvolved;
    
    /// <summary>
    /// Checks if any issues resulted (for UI display logic)
    /// </summary>
    public bool HasResultingIssues => 
        PropertyDamage || PersonalInjury || Fatality || OtherIssues;
    
    /// <summary>
    /// Checks if operational impact occurred (for UI display logic)
    /// </summary>
    public bool HasOperationalImpact => 
        !string.IsNullOrWhiteSpace(AirlineCompanyOperator) ||
        FlightDelay == "Yes" ||
        EquipmentRemovedFromService == "Yes";
    
    /// <summary>
    /// Checks if investigation details are present (for UI display logic)
    /// </summary>
    public bool HasInvestigationDetails => 
        PoliceReport == "Yes" || !string.IsNullOrWhiteSpace(PoliceReportDetails);
    
    /// <summary>
    /// Gets a summary of the dataset for display purposes
    /// </summary>
    public string GetDatasetSummary()
    {
        var summary = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(SharedNarrative))
            summary.Add($"Narrative: {SharedNarrative.Substring(0, Math.Min(50, SharedNarrative.Length))}...");
        
        if (!string.IsNullOrWhiteSpace(LocationArea))
            summary.Add($"Location: {LocationArea}");
            
        if (!string.IsNullOrWhiteSpace(Weather))
            summary.Add($"Weather: {Weather}");
        
        if (!string.IsNullOrWhiteSpace(TriggeringEvent))
            summary.Add($"Event: {TriggeringEvent}");
        
        if (ContributingFactors.Any())
            summary.Add($"Factors: {ContributingFactors.Count} selected");
        
        return string.Join("; ", summary);
    }
    
    #endregion
}