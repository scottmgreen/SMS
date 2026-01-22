//using SMS_Domain.Common;
//using SMS_Domain.Enums;
//using SMS_Shared.Common;

//namespace SMS_Domain.Entities;

///// <summary>
///// Committee membership entity linking users to committees
///// Supports temporal membership and role-based access
///// </summary>
//public sealed class CommitteeMembership : BaseAuditableEntity
//{
//    public CommitteeMembership(MembershipID id, string userId, string committeeId, MembershipType membershipType, bool isVotingMember, string assignedBy)
//        : base(id, assignedBy, DateTime.UtcNow)
//    {
//        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
//        CommitteeId = committeeId ?? throw new ArgumentNullException(nameof(committeeId));
//        MembershipType = membershipType ?? throw new ArgumentNullException(nameof(membershipType));
//        IsVotingMember = isVotingMember;
//        IsActive = true;
//        EffectiveDate = DateTime.UtcNow;
//        AssignedBy = assignedBy;
//        AssignedDate = DateTime.UtcNow;
//        Code = string.Empty; // Will be set via code generation
//    }

//    // Core Properties
//    public string Code { get; set; }
//    public string UserId { get; private set; }
//    public string CommitteeId { get; private set; }
//    public MembershipType MembershipType { get; private set; }
//    public bool IsVotingMember { get; private set; }
//    public bool IsActive { get; private set; }

//    // Temporal Properties
//    public DateTime EffectiveDate { get; private set; }
//    public DateTime? ExpirationDate { get; private set; }

//    // Assignment Tracking
//    public string AssignedBy { get; private set; }
//    public DateTime AssignedDate { get; private set; }
//    public string? DeactivatedBy { get; private set; }
//    public DateTime? DeactivatedDate { get; private set; }

//    // Optional Properties
//    public string? Notes { get; private set; }
//    public string? Expertise { get; private set; }
//    public string? Department { get; private set; }

//    // Business Methods
//    public Result UpdateMembershipType(MembershipType newType, string updatedBy)
//    {
//        try
//        {
//            if (newType == null)
//                return Result.Failure(DomainErrors.CommitteeError.InvalidMembershipType);

//            MembershipType = newType;
//            UpdatedBy = updatedBy;
//            UpdatedDate = DateTime.UtcNow;
//            return Result.Success();
//        }
//        catch (Exception )
//        {
//            return Result.Failure(DomainErrors.CommitteeError.MembershipUpdateFailed);
//        }
//    }

//    public Result UpdateVotingRights(bool canVote, string updatedBy)
//    {
//        try
//        {
//            // Check if membership type supports voting rights
//            if (canVote && !MembershipType.HasVotingRights)
//            {
//                return Result.Failure(DomainErrors.CommitteeError.MembershipTypeDoesNotSupportVoting);
//            }

//            IsVotingMember = canVote;
//            UpdatedBy = updatedBy;
//            UpdatedDate = DateTime.UtcNow;
//            return Result.Success();
//        }
//        catch (Exception )
//        {
//            return Result.Failure(DomainErrors.CommitteeError.VotingRightsUpdateFailed);
//        }
//    }

//    public Result SetExpiration(DateTime pirationDate, string updatedBy)
//    {
//        try
//        {
//            if (pirationDate <= DateTime.UtcNow)
//                return Result.Failure(DomainErrors.CommitteeError.InvalidExpirationDate);

//            ExpirationDate = pirationDate;
//            UpdatedBy = updatedBy;
//            UpdatedDate = DateTime.UtcNow;
//            return Result.Success();
//        }
//        catch (Exception )
//        {
//            return Result.Failure(DomainErrors.CommitteeError.ExpirationUpdateFailed);
//        }
//    }

//    public Result ExtendMembership(DateTime? newExpirationDate, string updatedBy)
//    {
//        try
//        {
//            if (newExpirationDate.HasValue && newExpirationDate <= DateTime.UtcNow)
//                return Result.Failure(DomainErrors.CommitteeError.InvalidExpirationDate);

//            ExpirationDate = newExpirationDate;
//            UpdatedBy = updatedBy;
//            UpdatedDate = DateTime.UtcNow;
//            return Result.Success();
//        }
//        catch (Exception )
//        {
//            return Result.Failure(DomainErrors.CommitteeError.MembershipExtensionFailed);
//        }
//    }

//    public void Deactivate(string deactivatedBy, DateTime deactivatedDate)
//    {
//        IsActive = false;
//        DeactivatedBy = deactivatedBy;
//        DeactivatedDate = deactivatedDate;
//        UpdatedBy = deactivatedBy;
//        UpdatedDate = deactivatedDate;
//    }

//    public Result Reactivate(string reactivatedBy)
//    {
//        try
//        {
//            // Check if membership is pired
//            if (IsExpired())
//                return Result.Failure(DomainErrors.CommitteeError.CannotReactivateExpiredMembership);

//            IsActive = true;
//            DeactivatedBy = null;
//            DeactivatedDate = null;
//            UpdatedBy = reactivatedBy;
//            UpdatedDate = DateTime.UtcNow;
//            return Result.Success();
//        }
//        catch (Exception )
//        {
//            return Result.Failure(DomainErrors.CommitteeError.MembershipReactivationFailed);
//        }
//    }

//    public void UpdateDetails(string? notes, string? pertise, string? department, string updatedBy)
//    {
//        Notes = notes;
//        Expertise = pertise;
//        Department = department;
//        UpdatedBy = updatedBy;
//        UpdatedDate = DateTime.UtcNow;
//    }

//    // Query Methods
//    public bool IsCurrentlyActive()
//    {
//        return IsActive && 
//               EffectiveDate <= DateTime.UtcNow && 
//               (ExpirationDate == null || ExpirationDate > DateTime.UtcNow);
//    }

//    public bool IsExpired()
//    {
//        return ExpirationDate.HasValue && ExpirationDate <= DateTime.UtcNow;
//    }

//    public bool IsTemporary()
//    {
//        return MembershipType == SMS_Domain.Enums.MembershipType.Temporary;
//    }

//    public bool IsCore()
//    {
//        return MembershipType == SMS_Domain.Enums.MembershipType.Core;
//    }

//    public bool IsExternal()
//    {
//        return MembershipType == SMS_Domain.Enums.MembershipType.External;
//    }

//    public bool IsSME()
//    {
//        return MembershipType == SMS_Domain.Enums.MembershipType.SME;
//    }

//    public bool HasVotingRights()
//    {
//        return IsVotingMember && MembershipType.HasVotingRights && IsCurrentlyActive();
//    }

//    public TimeSpan GetMembershipDuration()
//    {
//        var endDate = DeactivatedDate ?? ExpirationDate ?? DateTime.UtcNow;
//        return endDate - EffectiveDate;
//    }

//    public int GetDaysUntilExpiration()
//    {
//        if (!ExpirationDate.HasValue) return -1;
//        return (int)(ExpirationDate.Value - DateTime.UtcNow).TotalDays;
//    }

//    public bool IsExpiringSoon(int daysThreshold = 30)
//    {
//        return ExpirationDate.HasValue && GetDaysUntilExpiration() <= daysThreshold && GetDaysUntilExpiration() > 0;
//    }
//}