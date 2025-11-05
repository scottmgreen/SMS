using SMS_Domain.Common;
using SMS_Domain.Enums;

namespace SMS_Domain.Entities;

/// <summary>
/// Meeting attendee entity for tracking attendance at committee meetings
/// </summary>
public sealed class MeetingAttendee : BaseEntity
{
    public MeetingAttendee(AttendeeID id, string meetingId, string userId, AttendanceType attendanceType)
        : base(id)
    {
        MeetingId = meetingId ?? throw new ArgumentNullException(nameof(meetingId));
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        AttendanceType = attendanceType ?? throw new ArgumentNullException(nameof(attendanceType));
        RecordedDate = DateTime.UtcNow;
    }

    // Core Properties
    public string MeetingId { get; private set; }
    public string UserId { get; private set; }
    public AttendanceType AttendanceType { get; private set; }
    public DateTime RecordedDate { get; private set; }
    
    // Optional Properties
    public string? Notes { get; private set; }
    public DateTime? ArrivalTime { get; private set; }
    public DateTime? DepartureTime { get; private set; }
    public string? RecordedBy { get; private set; }

    // Business Methods
    public void UpdateAttendance(AttendanceType newAttendanceType, string? notes = null, string? recordedBy = null)
    {
        AttendanceType = newAttendanceType;
        Notes = notes;
        RecordedBy = recordedBy;
        RecordedDate = DateTime.UtcNow;
    }

    public void RecordArrival(DateTime arrivalTime, string? recordedBy = null)
    {
        ArrivalTime = arrivalTime;
        RecordedBy = recordedBy;
        RecordedDate = DateTime.UtcNow;
        
        // Update attendance type if currently absent
        if (AttendanceType == SMS_Domain.Enums.AttendanceType.Absent)
        {
            AttendanceType = SMS_Domain.Enums.AttendanceType.Present;
        }
    }

    public void RecordDeparture(DateTime departureTime, string? recordedBy = null)
    {
        DepartureTime = departureTime;
        RecordedBy = recordedBy;
        RecordedDate = DateTime.UtcNow;
        
        // If departure is before meeting end, mark as partial
        if (AttendanceType == SMS_Domain.Enums.AttendanceType.Present)
        {
            AttendanceType = SMS_Domain.Enums.AttendanceType.Partial;
        }
    }

    public void AddNotes(string notes, string? recordedBy = null)
    {
        Notes = string.IsNullOrWhiteSpace(Notes) ? notes : $"{Notes}; {notes}";
        RecordedBy = recordedBy;
        RecordedDate = DateTime.UtcNow;
    }

    // Query Methods
    public bool IsPresent()
    {
        return AttendanceType.CountsAsPresent;
    }

    public bool IsAbsent()
    {
        return AttendanceType == SMS_Domain.Enums.AttendanceType.Absent;
    }

    public bool IsExcused()
    {
        return AttendanceType == SMS_Domain.Enums.AttendanceType.Excused;
    }

    public bool IsVirtual()
    {
        return AttendanceType == SMS_Domain.Enums.AttendanceType.Virtual;
    }

    public bool IsPartial()
    {
        return AttendanceType == SMS_Domain.Enums.AttendanceType.Partial;
    }

    public TimeSpan? GetAttendanceDuration()
    {
        if (ArrivalTime.HasValue && DepartureTime.HasValue)
        {
            return DepartureTime.Value - ArrivalTime.Value;
        }
        return null;
    }

    public bool WasLate(DateTime meetingStartTime)
    {
        return ArrivalTime.HasValue && ArrivalTime > meetingStartTime;
    }

    public bool LeftEarly(DateTime meetingEndTime)
    {
        return DepartureTime.HasValue && DepartureTime < meetingEndTime;
    }
}