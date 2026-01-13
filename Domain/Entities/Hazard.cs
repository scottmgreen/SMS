using SMS_Shared.Common;
using SMS_Domain.Enums;
using Microsoft.Win32.SafeHandles;

namespace SMS_Domain.Entities;

/// <summary>
/// Hazard Domain Entity - Mission Critical
/// Represents identified hazards within the SMS system with comprehensive properties
/// to support the Risk Assessment Wizard and hazard management workflows
/// </summary>
public sealed class Hazard : BaseAuditableEntity
{
    // Public constructor for domain usage
    public Hazard(HazardID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation with validation
    //private Hazard(HazardID id, string code, string description, string category)
    //    : base(id, "SYSTEM", DateTime.UtcNow)
    //{
    //    Code = code;
    //    Name = HazardType;
    //    Description = description;
    //    HazardCategory = category;
    //    Status = HazardStatus.Active;
    //    Priority = HazardPriority.Medium;
    //    CreatedDate = DateTime.UtcNow;
    //    UpdatedDate = DateTime.UtcNow;
    //    HazardLocation = new HazardLocation(new HazardLocationID("HL-0000"));
    //}

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public FiveMComponent? FiveMComponent { get; set; } // Man, Machine, Method, Material, Milieu - Smart Enum
    public HazardStatus Status { get; set; } = HazardStatus.Active;
    public HazardPriority Priority { get; set; } = HazardPriority.Medium;

    public bool IsInitialHazard { get; set; }   
    #endregion

    #region Hazard Classification Properties
    public string HazardCategory { get; set; } = string.Empty; // Aircraft Operations, Ground Operations, etc.
    public string? HazardType { get; set; }              // Type/category of hazard (e.g., "Operational", "Equipment", etc.)

    #endregion

    #region Reporting Information Properties

    public string ReportedBy { get; set; } = string.Empty;           // Who reported the hazard
    public DateTime ReportedOn { get; set; } = DateTime.UtcNow;     // When hazard was reported
    public string? ReportingDepartment { get; set; }                // Department that reported the hazard

    #endregion

    #region Privacy and Confidentiality Properties

    public bool IsConfidential { get; set; } = false;               // Confidential hazard - restricted access
    public bool IsAnonymous { get; set; } = false;                  // Anonymous reporting - protect reporter identity

    #endregion

    #region Location and Context Properties

    public HazardLocation Location { get; set; }
    public string? LocationArea { get; set; }
    public string? LocationSubArea { get; set; }

    // Hazard Location Entity Reference
    public HazardLocation? HazardLocation { get; set; }

    #endregion

    #region Related Entity References (IDs Only)

    // Related HazardFiles (IDs only - load separately when needed)
    private readonly List<HazardFileID> _hazardFileIds = new();
    public IReadOnlyList<HazardFileID> HazardFileIds => _hazardFileIds.AsReadOnly();

    // Related Mitigations (IDs only - load separately when needed) 
    private readonly List<MitigationID> _mitigationIds = new();
    public IReadOnlyList<MitigationID> MitigationIds => _mitigationIds.AsReadOnly();

    #endregion

    #region Assessment and Risk Properties

    public string ReportCode { get; set; } = string.Empty;
    public string? RiskMatrixCode { get; set; }
    public decimal? AverageScore { get; set; }
    public string? RiskLevel { get; set; } // Very Low, Low, Medium, High, Very High
    public string? WorstCredibleOutcome { get; set; }
    public string? RootCause { get; set; }

    #endregion

    #region Additional Assessment Context

    public string? AdditionalComments { get; set; }
    public bool RequiresInvestigation { get; set; } = false;
    public DateTime? InvestigationCompletedDate { get; set; }
    public string? InvestigationNotes { get; set; }

    #endregion

    #region Factory Methods

    
    /// <summary>
    /// Create hazard from Risk Assessment Step 2 data
    /// </summary>
    //public static Result<Hazard> CreateFromStep2(string hazardId, string description, string category, string? fiveMComponent)
    //{
    //    // Parse the fiveMComponent string to Smart Enum if provided
    //    FiveMComponent? component = null;
    //    if (!string.IsNullOrWhiteSpace(fiveMComponent))
    //    {
    //        component = Enums.FiveMComponent.FromValue(fiveMComponent.ToUpperInvariant()) 
    //                 ?? Enums.FiveMComponent.FromName(fiveMComponent);
    //    }

    //    return CreateComprehensive(hazardId, description, category, "SYSTEM", null, null, component);
    //}

    /// <summary>
    /// Create a comprehensive hazard with all required SMS information
    /// </summary>
    public static Result<Hazard> CreateComprehensive(string code, string description, string category, string reportedBy, string? reportingDepartment = null, string? hazardType = null, 
        FiveMComponent? fiveMComponent = null, bool isConfidential = false, bool isAnonymous = false)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.NullOrEmpty);
        }

        if (description.Length < 10)
        {
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(reportedBy))
        {
            return Result<Hazard>.Failure<Hazard>(DomainErrors.HazardError.ReportedByRequired);
        }

        var hazardId = new HazardID(code);
        var hazard = new Hazard(hazardId)
        {
            FiveMComponent = fiveMComponent,
            HazardType = hazardType,
            ReportedBy = reportedBy,
            ReportingDepartment = reportingDepartment,
            IsConfidential = isConfidential,
            IsAnonymous = isAnonymous
        };

        // Auto-analyze Five M component if not provided
        if (fiveMComponent == null)
        {
            var analysisResult = hazard.AnalyzeFiveMComponent();
            if (analysisResult.IsSuccess && analysisResult.Value != null)
            {
                hazard.FiveMComponent = analysisResult.Value;
            }
        }

        return Result<Hazard>.Success(hazard);
    }

    /// <summary>
    /// Create hazard from hazard reporting form
    /// </summary>
    public static Result<Hazard> CreateFromHazardReport(string description, string category, string reportedBy, 
        string? reportingDepartment, string? hazardType = null, HazardLocation location = null, 
        bool isConfidential = false, bool isAnonymous = false)
    {
        // Auto-generate code
        var code = $"HZ-0000"; // These are generated in the Database via stored proc so the initial code is HZ-0000
        
        var result = CreateComprehensive(code, description, category, reportedBy, reportingDepartment, hazardType, null, isConfidential, isAnonymous); // Let auto-analysis determine Five M component
            
        if (result.IsFailure)
        {
            return result;
        }

        var hazard = result.Value;
        hazard.HazardLocation = location;
        
        // Set the Name property based on hazard type or use a default
        hazard.Name = GetHazardNameFromType(hazardType) ?? $"Hazard Report - {DateTime.UtcNow:yyyy-MM-dd}";
        
        return Result<Hazard>.Success(hazard);
    }
    
    /// <summary>
    /// Get a user-friendly name based on hazard type
    /// </summary>
    private static string? GetHazardNameFromType(string? hazardType)
    {
        return hazardType switch
        {
            "RWY_INCURSION" => "Runway Incursion",
            "ACFT_DAMAGE" => "Aircraft Damage",
            "GROUND_VEHICLE" => "Ground Vehicle Incident",
            "WILDLIFE_STRIKE" => "Wildlife Strike",
            "FOD" => "Foreign Object Debris",
            "EQUIPMENT_FAIL" => "Equipment Failure",
            "PERSONNEL_INJURY" => "Personnel Injury",
            "OTHER" => "Other Hazard",
            _ => hazardType // Return as-is if not a known type
        };
    }

    #endregion

    #region Domain Behavior Methods

     
    

    /// <summary>
    /// Set hazard location
    /// </summary>
    //public Result<bool> SetLocation(HazardLocation hazardLocation)
    //{
    //    if (hazardLocation == null)
    //    {
    //        return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.NullOrEmpty);
    //    }

    //    // Validate that the location belongs to this hazard
    //    if (hazardLocation.HazardCode != Code)
    //    {
    //        return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.InvalidCode);
    //    }

    //    HazardLocation = hazardLocation;
        
    //    // Update the legacy location fields for backward compatibility
    //    Location = hazardLocation.GetDisplayName();
    //    LocationArea = hazardLocation.LocationArea;
    //    LocationSubArea = hazardLocation.LocationSubArea;
        
    //    UpdatedDate = DateTime.UtcNow;
    //    return Result<bool>.Success(true);
    //}

    /// <summary>
    /// Create and set a new hazard location
    /// </summary>
    //public Result<bool> CreateLocation(decimal? latitude = null, decimal? longitude = null, 
    //    string? locationArea = null, string? locationSubArea = null, string? description = null)
    //{
    //    var locationCode = $"HL-{Code}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        
    //    var locationResult = HazardLocation.Create(locationCode, Code, latitude, longitude);
    //    if (locationResult.IsFailure)
    //    {
    //        return Result<bool>.Failure<bool>(locationResult.Error);
    //    }

    //    var location = locationResult.Value;
    //    location.UpdateLocationInfo(locationArea, locationSubArea, null, description);

    //    return SetLocation(location);
    //}

    /// <summary>
    /// Remove hazard location
    /// </summary>

    #region Related Entity Management

    /// <summary>
    /// Add a HazardFile reference to this hazard
    /// </summary>
    public void AddHazardFile(HazardFileID hazardFileId)
    {
        if (hazardFileId != null && !_hazardFileIds.Contains(hazardFileId))
        {
            _hazardFileIds.Add(hazardFileId);
            UpdatedDate = DateTime.UtcNow;
        }
    }

   
    
   
    #endregion

    
    /// <summary>
    /// Analyze and suggest Five M component based on hazard description
    /// </summary>
    public Result<FiveMComponent?> AnalyzeFiveMComponent()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            return Result<FiveMComponent?>.Success((FiveMComponent?)null);
        }

        var suggestedComponent = Enums.FiveMComponent.AnalyzeFromDescription(Description);
        return Result<FiveMComponent?>.Success(suggestedComponent);
    }



    #endregion

    
}
