namespace CBT3_Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity, IAuditableEntity
{
    protected BaseAuditableEntity(BaseID<string> id, string createdBy, DateTime createdDate) : base(id)
    {
        CreatedBy = createdBy;
        CreatedDate = createdDate;
    }

    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
