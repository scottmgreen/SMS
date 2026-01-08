namespace SMS_Domain.Entities;

/// <summary>
/// SPI Threshold configuration for alerts
/// </summary>
public class SPIThreshold
{
    public string ThresholdType { get; set; } = string.Empty; // Warning, Critical, Target
    public decimal Value { get; set; }
    public string ComparisonOperator { get; set; } = string.Empty; // >=, <=, =, !=
    public string AlertMessage { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? AlertRecipients { get; set; }
}