using SMS_Domain.Common;
using SMS_Domain.Enums;
using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// Committee meeting entity for tracking meetings and documentation
/// </summary>
public sealed class CommitteeMeeting : BaseAuditableEntity
{
    private readonly List<MeetingAttendee> _attendees = new();
    private readonly List<MeetingAgendaItem> _agendaItems = new();

    public CommitteeMeeting(MeetingID id, string committeeId, DateTime meetingDate, MeetingType meetingType, string facilitatorId, string scheduledBy)
        : base(id, scheduledBy, DateTime.UtcNow)
    {
        CommitteeId = committeeId ?? throw new ArgumentNullException(nameof(committeeId));
        MeetingDate = meetingDate;
        MeetingType = meetingType ?? throw new ArgumentNullException(nameof(meetingType));
        FacilitatorId = facilitatorId ?? throw new ArgumentNullException(nameof(facilitatorId));
        Status = MeetingStatus.Scheduled;
        Code = string.Empty; // Will be set via code generation
    }

    // Core Properties
    public string Code { get; set; }
    public string CommitteeId { get; private set; }
    public DateTime MeetingDate { get; private set; }
    public MeetingType MeetingType { get; private set; }
    public string FacilitatorId { get; private set; }
    public MeetingStatus Status { get; private set; }
    
    // Meeting Documentation
    public string? Agenda { get; private set; }
    public string? Minutes { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    
    // Optional Properties
    public string? Location { get; private set; }
    public string? VirtualMeetingLink { get; private set; }
    public TimeSpan? Duration { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ActualStartTime { get; private set; }
    public DateTime? ActualEndTime { get; private set; }
    
    // Collections
    public IReadOnlyList<MeetingAttendee> Attendees => _attendees.AsReadOnly();
    public IReadOnlyList<MeetingAgendaItem> AgendaItems => _agendaItems.AsReadOnly();

    // Business Methods
    public Result UpdateMeetingDetails(DateTime meetingDate, string? location, string? virtualLink, string updatedBy)
    {
        try
        {
            // Validate meeting date is in the future (unless meeting is in progress or completed)
            if (meetingDate <= DateTime.UtcNow && Status == MeetingStatus.Scheduled)
                return Result.Failure(DomainErrors.MeetingError.InvalidMeetingDate);

            MeetingDate = meetingDate;
            Location = location;
            VirtualMeetingLink = virtualLink;
            UpdatedBy = updatedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MeetingUpdateFailed);
        }
    }

    public Result SetAgenda(string agenda, string updatedBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(agenda))
                return Result.Failure(DomainErrors.MeetingError.InvalidAgenda);

            if (Status == MeetingStatus.Completed)
                return Result.Failure(DomainErrors.MeetingError.CannotModifyCompletedMeeting);

            Agenda = agenda;
            UpdatedBy = updatedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.AgendaUpdateFailed);
        }
    }

    public Result SetMinutes(string minutes, string updatedBy)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(minutes))
                return Result.Failure(DomainErrors.MeetingError.InvalidMinutes);

            if (Status != MeetingStatus.InProgress && Status != MeetingStatus.Completed)
                return Result.Failure(DomainErrors.MeetingError.CannotSetMinutesForNonActiveMeeting);

            Minutes = minutes;
            Status = MeetingStatus.PendingApproval;
            UpdatedBy = updatedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MinutesUpdateFailed);
        }
    }

    public Result ApproveMinutes(string approvedBy)
    {
        try
        {
            if (Status != MeetingStatus.PendingApproval)
                return Result.Failure(DomainErrors.MeetingError.MinutesNotPendingApproval);

            if (string.IsNullOrWhiteSpace(Minutes))
                return Result.Failure(DomainErrors.MeetingError.NoMinutesToApprove);

            ApprovedBy = approvedBy;
            ApprovedDate = DateTime.UtcNow;
            Status = MeetingStatus.Completed;
            UpdatedBy = approvedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MinutesApprovalFailed);
        }
    }

    public Result StartMeeting(string startedBy)
    {
        try
        {
            if (Status != MeetingStatus.Scheduled)
                return Result.Failure(DomainErrors.MeetingError.CannotStartNonScheduledMeeting);

            Status = MeetingStatus.InProgress;
            ActualStartTime = DateTime.UtcNow;
            UpdatedBy = startedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MeetingStartFailed);
        }
    }

    public Result EndMeeting(string endedBy)
    {
        try
        {
            if (Status != MeetingStatus.InProgress)
                return Result.Failure(DomainErrors.MeetingError.CannotEndNonActiveMeeting);

            ActualEndTime = DateTime.UtcNow;
            if (ActualStartTime.HasValue)
            {
                Duration = ActualEndTime.Value - ActualStartTime.Value;
            }
            
            // If minutes are already set, go to pending approval, otherwise stay in progress
            if (!string.IsNullOrWhiteSpace(Minutes))
            {
                Status = MeetingStatus.PendingApproval;
            }
            
            UpdatedBy = endedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MeetingEndFailed);
        }
    }

    public Result CancelMeeting(string cancelledBy, string? reason = null)
    {
        try
        {
            if (Status == MeetingStatus.Completed || Status == MeetingStatus.Cancelled)
                return Result.Failure(DomainErrors.MeetingError.CannotCancelCompletedMeeting);

            Status = MeetingStatus.Cancelled;
            Notes = reason;
            UpdatedBy = cancelledBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MeetingCancellationFailed);
        }
    }

    public Result PostponeMeeting(DateTime newDate, string postponedBy, string? reason = null)
    {
        try
        {
            if (Status == MeetingStatus.Completed || Status == MeetingStatus.Cancelled)
                return Result.Failure(DomainErrors.MeetingError.CannotPostponeCompletedMeeting);

            if (newDate <= DateTime.UtcNow)
                return Result.Failure(DomainErrors.MeetingError.InvalidMeetingDate);

            MeetingDate = newDate;
            Status = MeetingStatus.Postponed;
            Notes = reason;
            UpdatedBy = postponedBy;
            UpdatedDate = DateTime.UtcNow;
            return Result.Success();
        }
        catch (Exception )
        {
            return Result.Failure(DomainErrors.MeetingError.MeetingPostponeFailed);
        }
    }

    public Result<MeetingAttendee> AddAttendee(string userId, AttendanceType attendanceType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Result<MeetingAttendee>.Failure<MeetingAttendee>(DomainErrors.MeetingError.InvalidUserId);

            // Check if attendee already ists
            if (_attendees.Any(a => a.UserId == userId))
            {
                return Result<MeetingAttendee>.Failure<MeetingAttendee>(DomainErrors.MeetingError.AttendeeAlreadyExists);
            }

            var attendee = new MeetingAttendee(
                new AttendeeID(Guid.NewGuid().ToString()),
                Id.Value,
                userId,
                attendanceType
            );

            _attendees.Add(attendee);
            return Result<MeetingAttendee>.Success(attendee);
        }
        catch (Exception )
        {
            return Result<MeetingAttendee>.Failure<MeetingAttendee>(DomainErrors.MeetingError.AttendeeAddFailed);
        }
    }

    public Result<MeetingAgendaItem> AddAgendaItem(AgendaItemType itemType, string title, string? hazardId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(title))
                return Result<MeetingAgendaItem>.Failure<MeetingAgendaItem>(DomainErrors.MeetingError.InvalidAgendaItemTitle);

            var agendaItem = new MeetingAgendaItem(
                new AgendaItemID(Guid.NewGuid().ToString()),
                Id.Value,
                itemType,
                title,
                hazardId
            );

            _agendaItems.Add(agendaItem);
            return Result<MeetingAgendaItem>.Success(agendaItem);
        }
        catch (Exception )
        {
            return Result<MeetingAgendaItem>.Failure<MeetingAgendaItem>(DomainErrors.MeetingError.AgendaItemAddFailed);
        }
    }

    // Query Methods
    public bool IsCompleted()
    {
        return Status == MeetingStatus.Completed;
    }

    public bool IsCancelled()
    {
        return Status == MeetingStatus.Cancelled;
    }

    public bool IsInProgress()
    {
        return Status == MeetingStatus.InProgress;
    }

    public bool IsScheduled()
    {
        return Status == MeetingStatus.Scheduled;
    }

    public int GetAttendeeCount()
    {
        return _attendees.Count(a => a.AttendanceType.CountsAsPresent);
    }

    public IEnumerable<MeetingAttendee> GetPresentAttendees()
    {
        return _attendees.Where(a => a.AttendanceType.CountsAsPresent);
    }

    public IEnumerable<MeetingAgendaItem> GetHazardReviewItems()
    {
        return _agendaItems.Where(item => item.ItemType == AgendaItemType.HazardReview);
    }

    public IEnumerable<MeetingAgendaItem> GetRiskAssessmentItems()
    {
        return _agendaItems.Where(item => item.ItemType == AgendaItemType.RiskAssessment);
    }

    public bool HasQuorum(int requiredQuorum)
    {
        return GetAttendeeCount() >= requiredQuorum;
    }

    public TimeSpan? GetPlannedDuration()
    {
        return Duration;
    }

    public bool IsOverdue()
    {
        return MeetingDate < DateTime.UtcNow && Status == MeetingStatus.Scheduled;
    }

    public bool IsUpcoming(TimeSpan timeWindow)
    {
        return MeetingDate > DateTime.UtcNow && 
               MeetingDate <= DateTime.UtcNow.Add(timeWindow) && 
               Status == MeetingStatus.Scheduled;
    }
}