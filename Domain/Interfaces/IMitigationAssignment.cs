namespace SMS_Domain.Interfaces;

public interface IMitigationAssignment
{
    MitigationAssignmentID Id { get; set; }
    string? Code { get; set; }
    string? MitigationCode { get; set; }
    string? DepartmentCode { get; set; }
}
