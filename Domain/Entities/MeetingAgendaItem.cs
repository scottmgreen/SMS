using SMS_Domain.Common;
using SMS_Domain.Enums;

namespace SMS_Domain.Entities;

/// <summary>
/// Meeting agenda item entity for organizing and tracking meeting topics
/// Links to hazards and risks for safety-focused discussions
/// </summary>
public sealed class MeetingAgendaItem : BaseEntity
{
    public MeetingAgendaItem(AgendaItemID id, string meetingId, AgendaItemType itemType, string title, string? hazardId = null)
        : base(id)
    {
        MeetingId = meetingId ?? throw new ArgumentNullException(nameof(meetingId));
        ItemType = itemType ?? throw new ArgumentNullException(nameof(itemType));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        HazardId = hazardId;
        CreatedDate = DateTime.UtcNow;
        IsCompleted = false;
    }

    // Core Properties
    public string MeetingId { get; private set; }
    public AgendaItemType ItemType { get; private set; }
    public string Title { get; private set; }
    public string? HazardId { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedDate { get; private set; }
    
    // Content Properties
    public string? Description { get; private set; }
    public string? Discussion { get; private set; }
    public string? Decision { get; private set; }
    public string? ActionItems { get; private set; }
    
    // Optional Properties
    public int? OrderSequence { get; private set; }
    public string? Presenter { get; private set; }
    public TimeSpan? EstimatedDuration { get; private set; }
    public DateTime? CompletedDate { get; private set; }
    public string? CompletedBy { get; private set; }

    // Business Methods
    public void UpdateContent(string? description, string? presenter, TimeSpan? estimatedDuration)
    {
        Description = description;
        Presenter = presenter;
        EstimatedDuration = estimatedDuration;
    }

    public void UpdateSequence(int orderSequence)
    {
        OrderSequence = orderSequence;
    }

    public void RecordDiscussion(string discussion, string recordedBy)
    {
        if (string.IsNullOrWhiteSpace(discussion))
            throw new ArgumentException("Discussion cannot be null or empty", nameof(discussion));

        Discussion = string.IsNullOrWhiteSpace(Discussion) 
            ? discussion 
            : $"{Discussion}\n\n{discussion}";
    }

    public void RecordDecision(string decision, string recordedBy)
    {
        if (string.IsNullOrWhiteSpace(decision))
            throw new ArgumentException("Decision cannot be null or empty", nameof(decision));

        Decision = decision;
    }

    public void RecordActionItems(string actionItems, string recordedBy)
    {
        if (string.IsNullOrWhiteSpace(actionItems))
            throw new ArgumentException("Action items cannot be null or empty", nameof(actionItems));

        ActionItems = string.IsNullOrWhiteSpace(ActionItems) 
            ? actionItems 
            : $"{ActionItems}\n\n{actionItems}";
    }

    public void MarkCompleted(string completedBy)
    {
        IsCompleted = true;
        CompletedDate = DateTime.UtcNow;
        CompletedBy = completedBy;
    }

    public void MarkIncomplete()
    {
        IsCompleted = false;
        CompletedDate = null;
        CompletedBy = null;
    }

    public void LinkToHazard(string hazardId)
    {
        if (string.IsNullOrWhiteSpace(hazardId))
            throw new ArgumentException("Hazard ID cannot be null or empty", nameof(hazardId));

        HazardId = hazardId;
    }

    public void UnlinkFromHazard()
    {
        HazardId = null;
    }

    // Query Methods
    public bool IsHazardRelated()
    {
        return !string.IsNullOrWhiteSpace(HazardId) || 
               ItemType == AgendaItemType.HazardReview || 
               ItemType == AgendaItemType.RiskAssessment;
    }

    public bool IsSafetyCritical()
    {
        return ItemType.IsSafetyCritical;
    }

    public bool RequiresVoting()
    {
        return ItemType.RequiresVoting;
    }

    public bool IsHighPriority()
    {
        return ItemType.IsHighPriority;
    }

    public bool HasDiscussion()
    {
        return !string.IsNullOrWhiteSpace(Discussion);
    }

    public bool HasDecision()
    {
        return !string.IsNullOrWhiteSpace(Decision);
    }

    public bool HasActionItems()
    {
        return !string.IsNullOrWhiteSpace(ActionItems);
    }

    public bool IsDocumented()
    {
        return HasDiscussion() || HasDecision() || HasActionItems();
    }

    public TimeSpan? GetEstimatedDuration()
    {
        return EstimatedDuration;
    }

    public string GetStatusSummary()
    {
        if (!IsCompleted)
            return "Pending";

        var summary = "Completed";
        if (HasDecision())
            summary += " with Decision";
        if (HasActionItems())
            summary += " with Action Items";
        
        return summary;
    }
}