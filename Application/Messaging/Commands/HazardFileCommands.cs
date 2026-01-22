namespace SMS_Application.Messaging.Commands;

// =============================================
// HAZARD FILE COMMANDS - Following Exact SMS Pattern
// =============================================

public class CreateHazardFileCommand : BaseCommandBundle, IRequest<Result<HazardFile>>, ICreateCommand
{
    public HazardFile HazardFile { get; set; }

    public CreateHazardFileCommand(HazardFile hazardFile)
    {
        HazardFile = hazardFile ?? throw new ArgumentNullException(nameof(hazardFile));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        HazardFile.UploadedBy = userId;
        HazardFile.UploadedDate = timestamp;
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // For create commands, we typically don't set UpdatedBy
    }
}

public class UpdateHazardFileCommand : BaseCommandBundle, IRequest<Result<HazardFile>>, IUpdateCommand
{
    public HazardFile HazardFile { get; set; }

    public UpdateHazardFileCommand(HazardFile hazardFile)
    {
        HazardFile = hazardFile ?? throw new ArgumentNullException(nameof(hazardFile));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // For update commands, we typically don't modify CreatedBy
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        // HazardFile doesn't have standard UpdatedBy/UpdatedDate fields
        // The audit is tracked through UploadedBy and activity logs
    }
}

public class DeactivateHazardFileCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    public int FileId { get; set; }
    public string Reason { get; set; }
    public string DeactivatedBy { get; set; }

    public DeactivateHazardFileCommand(int fileId, string reason)
    {
        FileId = fileId;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        DeactivatedBy = userId;
    }
}

public class ReactivateHazardFileCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    public int FileId { get; set; }
    public string ReactivatedBy { get; set; }

    public ReactivateHazardFileCommand(int fileId)
    {
        FileId = fileId;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        ReactivatedBy = userId;
    }
}

public class SetHazardFileConfidentialityCommand : BaseCommandBundle, IRequest<Result<bool>>, IHasAuditFields
{
    public string FileCode { get; set; }
    public bool IsConfidential { get; set; }
    public string UpdatedBy { get; set; }

    public SetHazardFileConfidentialityCommand(string fileCode, bool isConfidential)
    {
        FileCode = fileCode ?? throw new ArgumentNullException(nameof(fileCode));
        IsConfidential = isConfidential;
    }

    public void SetCreatedBy(string userId, DateTime timestamp)
    {
        // This is not a create operation
    }

    public void SetUpdatedBy(string userId, DateTime timestamp)
    {
        UpdatedBy = userId;
    }
}