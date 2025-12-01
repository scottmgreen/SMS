using SMS_Domain.Common;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Committee entity for governance and decision-making processes
/// Integrates with existing SMS User Management system
/// </summary>
public sealed class SMSCommittee : BaseAuditableEntity
{
    //private readonly List<CommitteeMembership> _memberships = new();
    private readonly List<CommitteeMeeting> _meetings = new();

    public SMSCommittee(CommitteeID id, CommitteeType type, string name, string purpose, string chairPersonId, string createdBy)
        : base(id, createdBy, DateTime.UtcNow)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Purpose = purpose ?? throw new ArgumentNullException(nameof(purpose));
        ChairPersonId = chairPersonId ?? throw new ArgumentNullException(nameof(chairPersonId));
        IsActive = true;
        Code = string.Empty; // Will be set via code generation
    }

    // Core Properties
    public string Code { get; set; }
    public CommitteeType Type { get; private set; }
    public string Name { get; private set; }
    public string Purpose { get; private set; }
    public string ChairPersonId { get; private set; }
    public bool IsActive { get; private set; }
    
    // Optional Properties
    public string? Description { get; private set; }
    public string? MeetingSchedule { get; private set; }
    public string? Location { get; private set; }
    public int? MaxMembers { get; private set; }
    public int? QuorumRequired { get; private set; }
    
    // Collections
    //public IReadOnlyList<CommitteeMembership> Memberships => _memberships.AsReadOnly();
    public IReadOnlyList<CommitteeMeeting> Meetings => _meetings.AsReadOnly();

    // Business Methods
    //public Result<CommitteeMembership> AddMember(string userId, MembershipType membershipType, bool isVotingMember, string assignedBy)
    //{
    //    try
    //    {
    //        // Validate inputs
    //        if (string.IsNullOrWhiteSpace(userId))
    //            return Result<CommitteeMembership>.Failure<CommitteeMembership>(DomainErrors.CommitteeError.InvalidUserId);

    //        if (membershipType == null)
    //            return Result<CommitteeMembership>.Failure<CommitteeMembership>(DomainErrors.CommitteeError.InvalidMembershipType);

    //        // Check if user is already a member
    //        if (_memberships.Any(m => m.UserId == userId && m.IsActive))
    //        {
    //            return Result<CommitteeMembership>.Failure<CommitteeMembership>(DomainErrors.CommitteeError.UserAlreadyMember);
    //        }

    //        // Check maximum members limit
    //        if (MaxMembers.HasValue && _memberships.Count(m => m.IsActive) >= MaxMembers.Value)
    //        {
    //            return Result<CommitteeMembership>.Failure<CommitteeMembership>(DomainErrors.CommitteeError.MaxMembersExceeded);
    //        }

    //        var membership = new CommitteeMembership(
    //            new MembershipID(Guid.NewGuid().ToString()),
    //            userId,
    //            Id.Value,
    //            membershipType,
    //            isVotingMember,
    //            assignedBy
    //        );

    //        _memberships.Add(membership);
    //        return Result<CommitteeMembership>.Success(membership);
    //    }
    //    catch (Exception)
    //    {
    //        return Result<CommitteeMembership>.Failure<CommitteeMembership>(DomainErrors.CommitteeError.MembershipCreationFailed);
    //    }
    //}

    //public Result RemoveMember(string userId, string deactivatedBy)
    //{
    //    try
    //    {
    //        var membership = _memberships.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    //        if (membership == null)
    //        {
    //            return Result.Failure(DomainErrors.CommitteeError.MemberNotFound);
    //        }

    //        membership.Deactivate(deactivatedBy, DateTime.UtcNow);
    //        return Result.Success();
    //    }
    //    catch (Exception)
    //    {
    //        return Result.Failure(DomainErrors.CommitteeError.MemberRemovalFailed);
    //    }
    //}

    public Result<CommitteeMeeting> ScheduleMeeting(DateTime meetingDate, MeetingType meetingType, string facilitatorId, string scheduledBy)
    {
        try
        {
            // Validate inputs
            if (meetingDate <= DateTime.UtcNow)
                return Result<CommitteeMeeting>.Failure<CommitteeMeeting>(DomainErrors.MeetingError.InvalidMeetingDate);

            if (string.IsNullOrWhiteSpace(facilitatorId))
                return Result<CommitteeMeeting>.Failure<CommitteeMeeting>(DomainErrors.MeetingError.InvalidFacilitator);

            var meeting = new CommitteeMeeting(
                new MeetingID(Guid.NewGuid().ToString()),
                Id.Value,
                meetingDate,
                meetingType,
                facilitatorId,
                scheduledBy
            );

            _meetings.Add(meeting);
            return Result<CommitteeMeeting>.Success(meeting);
        }
        catch (Exception)
        {
            return Result<CommitteeMeeting>.Failure<CommitteeMeeting>(DomainErrors.MeetingError.MeetingCreationFailed);
        }
    }

    public void UpdateChairPerson(string newChairPersonId, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(newChairPersonId))
            throw new ArgumentException("Chair person ID cannot be null or empty", nameof(newChairPersonId));

        ChairPersonId = newChairPersonId;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void UpdateStatus(bool isActive, string updatedBy)
    {
        IsActive = isActive;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    public void UpdateDetails(string? description, string? meetingSchedule, string? location, int? maxMembers, int? quorumRequired, string updatedBy)
    {
        Description = description;
        MeetingSchedule = meetingSchedule;
        Location = location;
        MaxMembers = maxMembers;
        QuorumRequired = quorumRequired;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    // Query Methods
    //public IEnumerable<CommitteeMembership> GetActiveMembers()
    //{
    //    return _memberships.Where(m => m.IsActive);
    //}

    //public IEnumerable<CommitteeMembership> GetVotingMembers()
    //{
    //    return _memberships.Where(m => m.IsActive && m.IsVotingMember);
    //}

    //public bool HasQuorum()
    //{
    //    if (!QuorumRequired.HasValue) return true;
    //    return GetVotingMembers().Count() >= QuorumRequired.Value;
    //}

    //public bool IsUserMember(string userId)
    //{
    //    return _memberships.Any(m => m.UserId == userId && m.IsActive);
    //}

    //public bool CanUserVote(string userId)
    //{
    //    return _memberships.Any(m => m.UserId == userId && m.IsActive && m.IsVotingMember);
    //}

    //public int GetActiveMemberCount()
    //{
    //    return _memberships.Count(m => m.IsActive);
    //}

    //public int GetVotingMemberCount()
    //{
    //    return _memberships.Count(m => m.IsActive && m.IsVotingMember);
    //}

    //public CommitteeMembership? GetMembershipForUser(string userId)
    //{
    //    return _memberships.FirstOrDefault(m => m.UserId == userId && m.IsActive);
    //}

    public IEnumerable<CommitteeMeeting> GetUpcomingMeetings()
    {
        return _meetings.Where(m => m.MeetingDate > DateTime.UtcNow && 
                                  (m.Status == MeetingStatus.Scheduled || m.Status == MeetingStatus.Postponed));
    }

    public IEnumerable<CommitteeMeeting> GetRecentMeetings(int count = 5)
    {
        return _meetings.Where(m => m.Status == MeetingStatus.Completed)
                       .OrderByDescending(m => m.MeetingDate)
                       .Take(count);
    }
}