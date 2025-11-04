namespace SMS_Domain.Interfaces;

public interface IMitigation
{
    MitigationID Id { get; set; }
    string? Code { get; set; }
    string? HazardCode { get; set; }
}
