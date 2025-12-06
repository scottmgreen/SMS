using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Mitigation Strategy Domain Entity - Mission Critical
/// NOT a Value Object - this is a proper Domain Entity with identity and lifecycle
/// </summary>
public sealed class MitigationStrategy : BaseAuditableEntity
{
    // Constructor for Entity Framework
    private MitigationStrategy() : base(new MitigationStrategyId(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation
    private MitigationStrategy(MitigationStrategyId id, string description, string hazardId) 
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Description = description;
        HazardId = hazardId;
        ControlType = "Administrative"; // Default
        Priority = "Medium"; // Default
        Status = MitigationStatus.Planned;
        EstimatedCost = 0;
        CreatedDate = DateTime.UtcNow;
         UpdatedDate = DateTime.UtcNow;
    }

    // Public Properties (now with public setters for UI binding)
    public string Description { get; set; } = string.Empty;
    public string HazardId { get; set; } = string.Empty; // Link to hazard
    public string ControlType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? TargetDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public MitigationStatus Status { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? CompletedBy { get; set; }
    public string? ImplementationNotes { get; set; }
    public int ProgressPercentage { get; set; } = 0;
    public string? ResponsibleDepartment { get; set; }
    public string? ResponsiblePerson { get; set; }

    /// <summary>
    /// Factory method to create new Mitigation Strategy
    /// </summary>
    public static Result<MitigationStrategy> Create(string description, string hazardId)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<MitigationStrategy>.Failure<MitigationStrategy>(DomainErrors.MitigationStrategyError.InvalidDescription);
        }

        if (string.IsNullOrWhiteSpace(hazardId))
        {
            return Result<MitigationStrategy>.Failure<MitigationStrategy>(DomainErrors.MitigationStrategyError.InvalidHazardId);
        }

        var id = new MitigationStrategyId($"MS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
        var strategy = new MitigationStrategy(id, description, hazardId);
        return Result<MitigationStrategy>.Success(strategy);
    }

    /// <summary>
    /// Update basic strategy details
    /// </summary>
    public Result<bool> UpdateDetails(string description, string controlType, string priority)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationStrategyError.InvalidDescription);
        }

        Description = description;
        ControlType = controlType ?? "Administrative";
        Priority = priority ?? "Medium";
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update implementation details
    /// </summary>
    public Result<bool> UpdateImplementation(DateTime? targetDate, decimal estimatedCost, string? responsibleDepartment, string? responsiblePerson)
    {
        TargetDate = targetDate;
        EstimatedCost = estimatedCost;
        ResponsibleDepartment = responsibleDepartment;
        ResponsiblePerson = responsiblePerson;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update progress
    /// </summary>
    public Result<bool> UpdateProgress(int progressPercentage, string? notes = null)
    {
        if (progressPercentage < 0 || progressPercentage > 100)
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationStrategyError.InvalidProgress);
        }

        ProgressPercentage = progressPercentage;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            ImplementationNotes = notes;
        }

        // Update status based on progress
        if (progressPercentage == 100)
        {
            Status = MitigationStatus.Completed;
            CompletedDate = DateTime.UtcNow;
        }
        else if (progressPercentage > 0)
        {
            Status = MitigationStatus.InProgress;
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Mark as completed
    /// </summary>
    public Result<bool> MarkAsCompleted(string completedBy)
    {
        if (string.IsNullOrWhiteSpace(completedBy))
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationStrategyError.InvalidCompletedBy);
        }

        Status = MitigationStatus.Completed;
        ProgressPercentage = 100;
        CompletedDate = DateTime.UtcNow;
        CompletedBy = completedBy;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Mark as cancelled
    /// </summary>
    public Result<bool> MarkAsCancelled(string reason)
    {
        Status = MitigationStatus.Cancelled;
        ImplementationNotes = $"Cancelled: {reason}";
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Check if strategy is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return TargetDate.HasValue && 
               TargetDate < DateTime.UtcNow && 
               Status != MitigationStatus.Completed;
    }

    /// <summary>
    /// Get days remaining until target date
    /// </summary>
    public int? GetDaysRemaining()
    {
        if (!TargetDate.HasValue || Status == MitigationStatus.Completed)
            return null;

        return (int)(TargetDate.Value - DateTime.UtcNow).TotalDays;
    }
}

/// <summary>
/// Mitigation Strategy ID Value Object
/// </summary>
public sealed class MitigationStrategyId : BaseID<string>
{
    public MitigationStrategyId(string id) : base(id) { }
}

/// <summary>
/// Mitigation Status Enumeration
/// </summary>
public enum MitigationStatus
{
    Planned,
    InProgress,
    OnHold,
    Completed,
    Cancelled,
    Overdue
}