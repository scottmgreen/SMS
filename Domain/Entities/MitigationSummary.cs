using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Mitigation Summary Domain Entity
/// Represents aggregated mitigation data for reporting and dashboard purposes
/// </summary>
public sealed class MitigationSummary : BaseAuditableEntity
{
    // Constructor for Entity Framework
    private MitigationSummary() : base(new MitigationSummaryId(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor following the pattern
    public MitigationSummary(MitigationSummaryId id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Basic properties following the pattern from other entities
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }
    
    // Mitigation-specific summary properties
    public string? HazardCode { get; set; }
    public string? MitigationType { get; set; }
    public string? Department { get; set; }
    public string? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public int ProgressPercentage { get; set; } = 0;
    public string? Priority { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? ImplementationNotes { get; set; }
    
    /// <summary>
    /// Factory method to create new Mitigation Summary
    /// </summary>
    public static Result<MitigationSummary> Create(string code, string name, string hazardCode)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<MitigationSummary>.Failure<MitigationSummary>(DomainErrors.MitigationError.InvalidCode);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<MitigationSummary>.Failure<MitigationSummary>(DomainErrors.MitigationError.NullOrEmpty);
        }

        var id = new MitigationSummaryId($"MSU-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}");
        var summary = new MitigationSummary(id)
        {
            Code = code,
            Name = name,
            HazardCode = hazardCode,
            Status = "Active",
            Stage = "Planning"
        };
        
        return Result<MitigationSummary>.Success(summary);
    }

    /// <summary>
    /// Update summary status
    /// </summary>
    public Result<bool> UpdateStatus(string status, int progressPercentage)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.InvalidStatus);
        }

        if (progressPercentage < 0 || progressPercentage > 100)
        {
            return Result<bool>.Failure<bool>(DomainErrors.MitigationError.InvalidProgress);
        }

        Status = status;
        ProgressPercentage = progressPercentage;
        UpdatedDate = DateTime.UtcNow;

        if (progressPercentage == 100)
        {
            CompletedDate = DateTime.UtcNow;
            Status = "Completed";
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Check if mitigation is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return DueDate.HasValue && 
               DueDate < DateTime.UtcNow && 
               Status != "Completed";
    }

    /// <summary>
    /// Get days remaining until due date
    /// </summary>
    public int? GetDaysRemaining()
    {
        if (!DueDate.HasValue || Status == "Completed")
            return null;

        return (int)(DueDate.Value - DateTime.UtcNow).TotalDays;
    }
}
