namespace SMS_Domain.Entities;

public sealed class Report : BaseAuditableEntity
{
    public Report(ReportID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    
    public DateTime IncidentDateTime { get; set; } = DateTime.Now;


    public string?  SubmittedBy { get; set; } = string.Empty;
    public DateTime SubmittedDate { get; set; } = DateTime.Now;
    public string?  SubmittingDepartment { get; set; }
    public string?  SubmittingDepartmentJobFunction { get; set; }
    public string?  ReportContactName { get; set; }
    public string?  ReportContactCell { get; set; }
    public string?  ReportContactEmail { get; set; }

    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Stage { get; set; }

    public bool IsAnonymous { get; set; }
}
