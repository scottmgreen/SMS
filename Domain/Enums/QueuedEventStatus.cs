namespace SMS_Domain.Enums;

public abstract class QueuedEventStatus : BaseEnum<QueuedEventStatus>
{
    protected QueuedEventStatus(int id, string value, string name, string description) : base(value, name)
    {
        Id = id;
        Description = description;
    }

    public int Id { get; }
    public string Description { get; }

    public static readonly QueuedEventStatus Pending = new PendingStatus();
    public static readonly QueuedEventStatus Processing = new ProcessingStatus();
    public static readonly QueuedEventStatus Processed = new ProcessedStatus();
    public static readonly QueuedEventStatus Failed = new FailedStatus();
    public static readonly QueuedEventStatus Cancelled = new CancelledStatus();

    public static QueuedEventStatus? FromId(int id)
    {
        return id switch
        {
            0 => Pending,
            1 => Processing,
            2 => Processed,
            3 => Failed,
            4 => Cancelled,
            _ => null
        };
    }

    public static explicit operator int(QueuedEventStatus status)
    {
        return status.Id;
    }

    public static explicit operator QueuedEventStatus(int id)
    {
        return FromId(id) ?? throw new InvalidCastException($"Unknown QueuedEventStatus id '{id}'.");
    }

    private sealed class PendingStatus : QueuedEventStatus
    {
        public PendingStatus() : base(0, "PENDING", "Pending", "Event is waiting to be processed")
        {
        }
    }

    private sealed class ProcessingStatus : QueuedEventStatus
    {
        public ProcessingStatus() : base(1, "PROCESSING", "Processing", "Event is currently being processed")
        {
        }
    }

    private sealed class ProcessedStatus : QueuedEventStatus
    {
        public ProcessedStatus() : base(2, "PROCESSED", "Processed", "Event was processed successfully")
        {
        }
    }

    private sealed class FailedStatus : QueuedEventStatus
    {
        public FailedStatus() : base(3, "FAILED", "Failed", "Event processing failed")
        {
        }
    }

    private sealed class CancelledStatus : QueuedEventStatus
    {
        public CancelledStatus() : base(4, "CANCELLED", "Cancelled", "Event was cancelled")
        {
        }
    }
}
