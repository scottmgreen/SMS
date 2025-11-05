using SMS_Domain.Common;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Risk approval entity for managing approval workflows based on risk levels
/// Integrates with SMSRole authority levels and committee decision processes
/// </summary>
public sealed class RiskApproval : BaseAuditableEntity
{
    public RiskApproval(RiskApprovalID id, string hazardId, RiskLevel riskLevel, string requiredApproverId, string createdBy)
        : base(id, createdBy, DateTime.UtcNow)
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        RiskLevel = riskLevel ?? throw new ArgumentNullException(nameof(riskLevel));
        RequiredApproverId = requiredApproverId ?? throw new ArgumentNullException(nameof(requiredApproverId));
        ApprovalStatus = SMS_Domain.Enums.ApprovalStatus.Pending;
        RequestDate = DateTime.UtcNow;
        Code = string.Empty; // Will be set via code generation
    }

    // Core Properties
    public string Code { get; set; }
    public string HazardId { get; private set; }
    public RiskLevel RiskLevel { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public DateTime RequestDate { get; private set; }
    
    // Approval Authority
    public string RequiredApproverId { get; private set; }
    public string? ActualApproverId { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string? ApprovalNotes { get; private set; }
    
    // Escalation Tracking
    public string? EscalatedToId { get; private set; }
    public DateTime? EscalatedDate { get; private set; }
    public string? EscalationReason { get; private set; }
    public string? EscalatedBy { get; private set; }
    
    // Committee Integration
    public string? CommitteeId { get; private set; }
    public string? MeetingId { get; private set; }
    
    // Timing Properties
    public DateTime? DueDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }

    // Business Methods
    public Result Approve(string approverId, string? notes = null)
    {
        try
        {
            if (ApprovalStatus != SMS_Domain.Enums.ApprovalStatus.Pending && 
                ApprovalStatus != SMS_Domain.Enums.ApprovalStatus.UnderReview)
            {
                return Result.Failure(DomainErrors.ApprovalError.CannotApproveNonPendingRequest);
            }

            if (string.IsNullOrWhiteSpace(approverId))
                return Result.Failure(DomainErrors.ApprovalError.InvalidApprover);

            ActualApproverId = approverId;
            ApprovedDate = DateTime.UtcNow;
            ApprovalNotes = notes;
            ApprovalStatus = SMS_Domain.Enums.ApprovalStatus.Approved;
            UpdatedBy = approverId;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(DomainErrors.ApprovalError.ApprovalProcessFailed);
        }
    }

    public Result Reject(string rejectedBy, string rejectionReason)
    {
        try
        {
            if (ApprovalStatus != SMS_Domain.Enums.ApprovalStatus.Pending && 
                ApprovalStatus != SMS_Domain.Enums.ApprovalStatus.UnderReview)
            {
                return Result.Failure(DomainErrors.ApprovalError.CannotRejectNonPendingRequest);
            }

            if (string.IsNullOrWhiteSpace(rejectedBy))
                return Result.Failure(DomainErrors.ApprovalError.InvalidRejecter);

            if (string.IsNullOrWhiteSpace(rejectionReason))
                return Result.Failure(DomainErrors.ApprovalError.RejectionReasonRequired);

            ActualApproverId = rejectedBy;
            ApprovedDate = DateTime.UtcNow;
            ApprovalNotes = rejectionReason;
            ApprovalStatus = SMS_Domain.Enums.ApprovalStatus.Rejected;
            UpdatedBy = rejectedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(DomainErrors.ApprovalError.RejectionProcessFailed);
        }
    }

    public Result Escalate(string escalatedToId, string escalationReason, string escalatedBy)
    {
        try
        {
            if (ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Approved || 
                ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Rejected)
            {
                return Result.Failure(DomainErrors.ApprovalError.CannotEscalateCompletedRequest);
            }

            if (string.IsNullOrWhiteSpace(escalatedToId))
                return Result.Failure(DomainErrors.ApprovalError.InvalidEscalationTarget);

            if (string.IsNullOrWhiteSpace(escalationReason))
                return Result.Failure(DomainErrors.ApprovalError.EscalationReasonRequired);

            EscalatedToId = escalatedToId;
            EscalatedDate = DateTime.UtcNow;
            EscalationReason = escalationReason;
            EscalatedBy = escalatedBy;
            RequiredApproverId = escalatedToId; // Update required approver
            ApprovalStatus = SMS_Domain.Enums.ApprovalStatus.Escalated;
            UpdatedBy = escalatedBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(DomainErrors.ApprovalError.EscalationProcessFailed);
        }
    }

    public Result StartReview(string reviewerId)
    {
        try
        {
            if (ApprovalStatus != SMS_Domain.Enums.ApprovalStatus.Pending)
                return Result.Failure(DomainErrors.ApprovalError.CannotStartReviewOnNonPendingRequest);

            ApprovalStatus = SMS_Domain.Enums.ApprovalStatus.UnderReview;
            UpdatedBy = reviewerId;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(DomainErrors.ApprovalError.ReviewStartFailed);
        }
    }

    public void SetDueDate(DateTime dueDate, string updatedBy)
    {
        if (dueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future", nameof(dueDate));

        DueDate = dueDate;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void SetExpirationDate(DateTime expirationDate, string updatedBy)
    {
        if (expirationDate <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(expirationDate));

        ExpirationDate = expirationDate;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void AssignToCommittee(string committeeId, string? meetingId, string assignedBy)
    {
        CommitteeId = committeeId;
        MeetingId = meetingId;
        UpdatedBy = assignedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public Result ExpireApproval(string expiredBy)
    {
        try
        {
            if (ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Approved || 
                ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Rejected)
            {
                return Result.Failure(DomainErrors.ApprovalError.CannotExpireCompletedRequest);
            }

            ApprovalStatus = SMS_Domain.Enums.ApprovalStatus.Expired;
            UpdatedBy = expiredBy;
            UpdatedDate = DateTime.UtcNow;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(DomainErrors.ApprovalError.ExpirationProcessFailed);
        }
    }

    // Query Methods
    public bool IsApproved()
    {
        return ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Approved;
    }

    public bool IsRejected()
    {
        return ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Rejected;
    }

    public bool IsPending()
    {
        return ApprovalStatus.IsInProgress;
    }

    public bool IsExpired()
    {
        if (ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Expired)
            return true;

        return ExpirationDate.HasValue && ExpirationDate <= DateTime.UtcNow && 
               ApprovalStatus.IsInProgress;
    }

    public bool IsOverdue()
    {
        return DueDate.HasValue && DueDate <= DateTime.UtcNow && ApprovalStatus.IsInProgress;
    }

    public bool IsEscalated()
    {
        return ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.Escalated;
    }

    public bool IsUnderReview()
    {
        return ApprovalStatus == SMS_Domain.Enums.ApprovalStatus.UnderReview;
    }

    public bool IsAssignedToCommittee()
    {
        return !string.IsNullOrWhiteSpace(CommitteeId);
    }

    public bool IsScheduledForMeeting()
    {
        return !string.IsNullOrWhiteSpace(MeetingId);
    }

    public bool CanApprover(SMSRole approverRole)
    {
        return RiskLevel.CanApprove(approverRole);
    }

    public TimeSpan? GetTimeToExpiration()
    {
        if (!ExpirationDate.HasValue) return null;
        var timeRemaining = ExpirationDate.Value - DateTime.UtcNow;
        return timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero;
    }

    public TimeSpan GetProcessingTime()
    {
        var endDate = ApprovedDate ?? DateTime.UtcNow;
        return endDate - RequestDate;
    }

    public int GetDaysUntilDue()
    {
        if (!DueDate.HasValue) return -1;
        return (int)(DueDate.Value - DateTime.UtcNow).TotalDays;
    }
}