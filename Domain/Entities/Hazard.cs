using SMS_Shared.Common;
using SMS_Domain.Enums;

namespace SMS_Domain.Entities;

/// <summary>
/// Hazard Domain Entity - Mission Critical
/// Represents identified hazards within the SMS system with comprehensive properties
/// to support the Risk Assessment Wizard and hazard management workflows
/// </summary>
public sealed class Hazard : BaseAuditableEntity
{
    // Constructor for Entity Framework
    private Hazard() : base(new HazardID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor for domain usage
    public Hazard(HazardID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation with validation
    private Hazard(HazardID id, string code, string description, string category)
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        Description = description;
        Category = category;
        Status = HazardStatus.Active;
        Priority = HazardPriority.Medium;
        CreatedDate = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Aircraft Operations, Ground Operations, etc.
    public FiveMComponent? FiveMComponent { get; set; } // Man, Machine, Method, Material, Milieu - Smart Enum
    public HazardStatus Status { get; set; } = HazardStatus.Active;
    public HazardPriority Priority { get; set; } = HazardPriority.Medium;

    #endregion

    #region Hazard Classification Properties

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

    public string? Location { get; set; }
    public string? LocationArea { get; set; }
    public string? LocationSubArea { get; set; }

    // Hazard Location Entity Reference
    public HazardLocation? HazardLocation { get; private set; }

    // Hazard Files Collection
    private readonly List<HazardFile> _hazardFiles = new();
    public IReadOnlyList<HazardFile> HazardFiles => _hazardFiles.AsReadOnly();

    #endregion

    #region Assessment and Risk Properties

    public string ReportCode { get; set; } = string.Empty;
    public string? ScoringPanelCode { get; set; }
    public string? AverageScore { get; set; }
    public string? RiskLevel { get; set; } // Very Low, Low, Medium, High, Very High
    public string? WorstCredibleOutcome { get; set; }
    public string? RootCause { get; set; }

    #endregion

    #region Mitigation Properties

    public string? CurrentMitigations { get; set; }
    public string? ProposedMitigations { get; set; }
    public DateTime? MitigationTargetDate { get; set; }
    public string? MitigationOwner { get; set; }

    #endregion

    #region Additional Assessment Context

    public string? AdditionalComments { get; set; }
    public bool RequiresInvestigation { get; set; } = false;
    public DateTime? InvestigationCompletedDate { get; set; }
    public string? InvestigationNotes { get; set; }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new hazard with basic information
    /// </summary>
    public static Result<Hazard> Create(string code, string description, string category, FiveMComponent fiveMComponent = null)
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

        var hazardId = new HazardID(code);
        var hazard = new Hazard(hazardId, code, description, category)
        {
            FiveMComponent = fiveMComponent
        };

        return Result<Hazard>.Success(hazard);
    }

    /// <summary>
    /// Create hazard from Risk Assessment Step 2 data
    /// </summary>
    public static Result<Hazard> CreateFromStep2(string hazardId, string description, string category, string? fiveMComponent)
    {
        // Parse the fiveMComponent string to Smart Enum if provided
        FiveMComponent? component = null;
        if (!string.IsNullOrWhiteSpace(fiveMComponent))
        {
            component = Enums.FiveMComponent.FromValue(fiveMComponent.ToUpperInvariant()) 
                     ?? Enums.FiveMComponent.FromName(fiveMComponent);
        }

        return CreateComprehensive(hazardId, description, category, "SYSTEM", null, null, component);
    }

    /// <summary>
    /// Create a comprehensive hazard with all required SMS information
    /// </summary>
    public static Result<Hazard> CreateComprehensive(string code, string description, string category, 
        string reportedBy, string? reportingDepartment = null, string? hazardType = null, 
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
        var hazard = new Hazard(hazardId, code, description, category)
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
        string? reportingDepartment, string? hazardType = null, string? location = null, 
        bool isConfidential = false, bool isAnonymous = false)
    {
        // Auto-generate code
        var code = $"HZ-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        
        var result = CreateComprehensive(code, description, category, reportedBy, reportingDepartment, 
            hazardType, null, isConfidential, isAnonymous); // Let auto-analysis determine Five M component
            
        if (result.IsFailure)
        {
            return result;
        }

        var hazard = result.Value;
        hazard.Location = location;
        
        return Result<Hazard>.Success(hazard);
    }

    #endregion

    #region Domain Behavior Methods

    /// <summary>
    /// Update hazard details
    /// </summary>
    public Result<bool> UpdateDetails(string description, string category, FiveMComponent? fiveMComponent = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.NullOrEmpty);
        }

        if (description.Length < 10)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardError.InvalidCode);
        }

        Description = description;
        Category = category ?? Category;
        FiveMComponent = fiveMComponent;
        UpdatedDate = DateTime.UtcNow;

        // Auto-analyze Five M component if not provided but description changed
        if (fiveMComponent == null)
        {
            var analysisResult = AnalyzeFiveMComponent();
            if (analysisResult.IsSuccess && analysisResult.Value != null)
            {
                FiveMComponent = analysisResult.Value;
            }
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update risk assessment information
    /// </summary>
    public Result<bool> UpdateRiskAssessment(string? worstCredibleOutcome, string? rootCause, string? riskLevel)
    {
        WorstCredibleOutcome = worstCredibleOutcome;
        RootCause = rootCause;
        RiskLevel = riskLevel;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update mitigation information
    /// </summary>
    public Result<bool> UpdateMitigations(string? currentMitigations, string? proposedMitigations,
        DateTime? targetDate, string? mitigationOwner)
    {
        CurrentMitigations = currentMitigations;
        ProposedMitigations = proposedMitigations;
        MitigationTargetDate = targetDate;
        MitigationOwner = mitigationOwner;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Mark hazard as requiring investigation
    /// </summary>
    public Result<bool> RequireInvestigation(string investigationNotes)
    {
        RequiresInvestigation = true;
        InvestigationNotes = investigationNotes;
        Status = HazardStatus.UnderInvestigation;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Complete investigation
    /// </summary>
    public Result<bool> CompleteInvestigation(string investigationNotes)
    {
        RequiresInvestigation = false;
        InvestigationCompletedDate = DateTime.UtcNow;
        InvestigationNotes = investigationNotes;
        Status = HazardStatus.Active; // Return to active for continued processing
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Set hazard location
    /// </summary>
    public Result<bool> SetLocation(HazardLocation hazardLocation)
    {
        if (hazardLocation == null)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.NullOrEmpty);
        }

        // Validate that the location belongs to this hazard
        if (hazardLocation.HazardCode != Code)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.InvalidCode);
        }

        HazardLocation = hazardLocation;
        
        // Update the legacy location fields for backward compatibility
        Location = hazardLocation.GetDisplayName();
        LocationArea = hazardLocation.LocationArea;
        LocationSubArea = hazardLocation.LocationSubArea;
        
        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Create and set a new hazard location
    /// </summary>
    public Result<bool> CreateLocation(decimal? latitude = null, decimal? longitude = null, 
        string? locationArea = null, string? locationSubArea = null, string? description = null)
    {
        var locationCode = $"HL-{Code}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        
        var locationResult = HazardLocation.Create(locationCode, Code, latitude, longitude);
        if (locationResult.IsFailure)
        {
            return Result<bool>.Failure<bool>(locationResult.Error);
        }

        var location = locationResult.Value;
        location.UpdateLocationInfo(locationArea, locationSubArea, null, description);

        return SetLocation(location);
    }

    /// <summary>
    /// Remove hazard location
    /// </summary>
    public Result<bool> RemoveLocation()
    {
        HazardLocation = null;
        Location = null;
        LocationArea = null;
        LocationSubArea = null;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update location coordinates
    /// </summary>
    public Result<bool> UpdateLocationCoordinates(decimal latitude, decimal longitude, 
        decimal? accuracyMeters = null, string source = "Manual")
    {
        if (HazardLocation == null)
        {
            // Create a new location if none exists
            var createResult = CreateLocation(latitude, longitude);
            if (createResult.IsFailure)
            {
                return createResult;
            }
        }

        var updateResult = HazardLocation!.UpdateCoordinates(latitude, longitude, accuracyMeters, source);
        if (updateResult.IsSuccess)
        {
            UpdatedDate = DateTime.UtcNow;
        }

        return updateResult;
    }

    /// <summary>
    /// Validate hazard location
    /// </summary>
    public Result<bool> ValidateLocation(string validatedBy, string? notes = null)
    {
        if (HazardLocation == null)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.NullOrEmpty);
        }

        var validateResult = HazardLocation.ValidateLocation(validatedBy, notes);
        if (validateResult.IsSuccess)
        {
            UpdatedDate = DateTime.UtcNow;
        }

        return validateResult;
    }

    /// <summary>
    /// Set hazard as confidential
    /// </summary>
    public Result<bool> SetConfidential(bool isConfidential, string reason = "")
    {
        IsConfidential = isConfidential;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            AdditionalComments = string.IsNullOrWhiteSpace(AdditionalComments) 
                ? $"Confidentiality set: {reason}" 
                : $"{AdditionalComments}; Confidentiality set: {reason}";
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Set hazard reporting as anonymous
    /// </summary>
    public Result<bool> SetAnonymous(bool isAnonymous, string reason = "")
    {
        IsAnonymous = isAnonymous;
        
        if (isAnonymous)
        {
            // When setting to anonymous, clear reporter identity for privacy
            ReportedBy = "ANONYMOUS";
        }
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            AdditionalComments = string.IsNullOrWhiteSpace(AdditionalComments) 
                ? $"Anonymity set: {reason}" 
                : $"{AdditionalComments}; Anonymity set: {reason}";
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update reporting information
    /// </summary>
    public Result<bool> UpdateReportingInfo(string? reportingDepartment, string? hazardType)
    {
        ReportingDepartment = reportingDepartment;
        HazardType = hazardType;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Check if hazard requires special privacy handling
    /// </summary>
    public bool RequiresPrivacyProtection()
    {
        return IsConfidential || IsAnonymous;
    }

    /// <summary>
    /// Get display name for reporter (handles anonymity)
    /// </summary>
    public string GetReporterDisplayName()
    {
        if (IsAnonymous)
            return "Anonymous Reporter";
            
        return string.IsNullOrWhiteSpace(ReportedBy) ? "Unknown Reporter" : ReportedBy;
    }

    /// <summary>
    /// Check if hazard is recently reported (within specified days)
    /// </summary>
    public bool IsRecentlyReported(int withinDays = 7)
    {
        return (DateTime.UtcNow - ReportedOn).TotalDays <= withinDays;
    }

    /// <summary>
    /// Get hazard age in days (updated to use ReportedOn)
    /// </summary>
    public int GetAgeInDays()
    {
        return (DateTime.UtcNow - ReportedOn).Days;
    }

    /// <summary>
    /// Calculate distance to a specific coordinate in meters
    /// </summary>
    public double? CalculateDistanceTo(decimal latitude, decimal longitude)
    {
        return HazardLocation?.CalculateDistanceTo(latitude, longitude);
    }

    /// <summary>
    /// Add a file to this hazard
    /// </summary>
    public Result<bool> AddFile(HazardFile hazardFile)
    {
        if (hazardFile == null)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
        }

        // Validate that the file belongs to this hazard
        if (hazardFile.HazardCode != Code)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.HazardCodeRequired);
        }

        // Check if file with same code already exists
        if (_hazardFiles.Any(f => f.Code == hazardFile.Code))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.CreateFailed);
        }

        _hazardFiles.Add(hazardFile);
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Remove a file from this hazard
    /// </summary>
    public Result<bool> RemoveFile(string fileCode)
    {
        if (string.IsNullOrWhiteSpace(fileCode))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NullOrEmpty);
        }

        var file = _hazardFiles.FirstOrDefault(f => f.Code == fileCode);
        if (file == null)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardFileError.NotFound);
        }

        _hazardFiles.Remove(file);
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Get files by category
    /// </summary>
    public IReadOnlyList<HazardFile> GetFilesByCategory(HazardFileCategory category)
    {
        return _hazardFiles.Where(f => f.Category == category && f.IsActive).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get files by type
    /// </summary>
    public IReadOnlyList<HazardFile> GetFilesByType(string fileType)
    {
        return _hazardFiles.Where(f => f.FileType.Equals(fileType, StringComparison.OrdinalIgnoreCase) && f.IsActive)
                          .ToList().AsReadOnly();
    }

    /// <summary>
    /// Get active files only
    /// </summary>
    public IReadOnlyList<HazardFile> GetActiveFiles()
    {
        return _hazardFiles.Where(f => f.IsActive).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get photo files
    /// </summary>
    public IReadOnlyList<HazardFile> GetPhotos()
    {
        return _hazardFiles.Where(f => f.IsImage() && f.IsActive).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get document files
    /// </summary>
    public IReadOnlyList<HazardFile> GetDocuments()
    {
        return _hazardFiles.Where(f => f.IsDocument() && f.IsActive).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get video files
    /// </summary>
    public IReadOnlyList<HazardFile> GetVideos()
    {
        return _hazardFiles.Where(f => f.IsVideo() && f.IsActive).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get total file count
    /// </summary>
    public int GetFileCount()
    {
        return _hazardFiles.Count(f => f.IsActive);
    }

    /// <summary>
    /// Get total file size in bytes
    /// </summary>
    public long GetTotalFileSizeBytes()
    {
        return _hazardFiles.Where(f => f.IsActive).Sum(f => f.FileSizeBytes);
    }

    /// <summary>
    /// Check if hazard has files
    /// </summary>
    public bool HasFiles()
    {
        return _hazardFiles.Any(f => f.IsActive);
    }

    /// <summary>
    /// Check if hazard has photos
    /// </summary>
    public bool HasDocuments()
    {
        return _hazardFiles.Any(f => f.IsDocument() && f.IsActive);
    }

    /// <summary>
    /// Check if hazard has confidential files
    /// </summary>
    public bool HasConfidentialFiles()
    {
        return _hazardFiles.Any(f => f.IsConfidential && f.IsActive);
    }

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

    /// <summary>
    /// Set Five M component with validation
    /// </summary>
    public Result<bool> SetFiveMComponent(FiveMComponent? component)
    {
        FiveMComponent = component;
        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Get Five M analysis questions for this hazard
    /// </summary>
    public List<string> GetFiveMAnalysisQuestions()
    {
        if (FiveMComponent == null)
        {
            return new List<string> { "Please first identify the primary Five M component (Man, Machine, Method, Material, Milieu) for this hazard." };
        }

        return FiveMComponent.GetAnalysisQuestions();
    }

    /// <summary>
    /// Get recommended mitigation approaches based on Five M component
    /// </summary>
    public List<string> GetRecommendedMitigationApproaches()
    {
        if (FiveMComponent == null)
        {
            return new List<string>();
        }

        return FiveMComponent.GetMitigationApproaches();
    }

    /// <summary>
    /// Check if this hazard involves high-impact Five M components
    /// </summary>
    public bool IsHighImpactFiveMComponent()
    {
        return FiveMComponent?.IsHighImpact ?? false;
    }

    /// <summary>
    /// Get Five M component display information
    /// </summary>
    public (string Icon, string Color, string Description) GetFiveMComponentDisplay()
    {
        if (FiveMComponent == null)
        {
            return ("fas fa-question", "text-muted", "Five M component not identified");
        }

        return (FiveMComponent.GetIconClass(), FiveMComponent.GetColorClass(), FiveMComponent.Description);
    }

    /// <summary>
    /// Check if hazard needs Five M analysis
    /// </summary>
    public bool NeedsFiveMAnalysis()
    {
        return FiveMComponent == null && !string.IsNullOrWhiteSpace(Description);
    }

    #endregion
}
