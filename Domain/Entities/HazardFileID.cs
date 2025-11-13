namespace SMS_Domain.Entities;

/// <summary>
/// HazardFile ID Value Object
/// </summary>
public sealed class HazardFileID : BaseID<string>
{
    public HazardFileID(string id) : base(id) { }
}