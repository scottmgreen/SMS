using SMS_Domain.Entities;
using SMS_Domain.Models;
using SMS_Domain.Enums;
using SMS_Application.Common;
using SMS_Application.Interfaces;
using SMS_Shared.Common;

namespace SMS_Application.Messaging.Commands;

// =============================================
// HAZARD FILE COMMANDS - Following Exact SMS Pattern
// =============================================

public class CreateHazardFileCommand : BaseCommandBundle, IRequest<Result<HazardFile>>
{
    public HazardFile HazardFile { get; set; }

    public CreateHazardFileCommand(HazardFile hazardFile)
    {
        HazardFile = hazardFile ?? throw new ArgumentNullException(nameof(hazardFile));
    }
}

public class UpdateHazardFileCommand : BaseCommandBundle, IRequest<Result<HazardFile>>
{
    public HazardFile HazardFile { get; set; }

    public UpdateHazardFileCommand(HazardFile hazardFile)
    {
        HazardFile = hazardFile ?? throw new ArgumentNullException(nameof(hazardFile));
    }
}

public class DeactivateHazardFileCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public int FileId { get; set; }
    public string Reason { get; set; }
    public string DeactivatedBy { get; set; }

    public DeactivateHazardFileCommand(int fileId, string reason, string deactivatedBy)
    {
        FileId = fileId;
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        DeactivatedBy = deactivatedBy ?? throw new ArgumentNullException(nameof(deactivatedBy));
    }
}

public class ReactivateHazardFileCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public int FileId { get; set; }
    public string ReactivatedBy { get; set; }

    public ReactivateHazardFileCommand(int fileId, string reactivatedBy)
    {
        FileId = fileId;
        ReactivatedBy = reactivatedBy ?? throw new ArgumentNullException(nameof(reactivatedBy));
    }
}

public class SetHazardFileConfidentialityCommand : BaseCommandBundle, IRequest<Result<bool>>
{
    public string FileCode { get; set; }
    public bool IsConfidential { get; set; }
    public string UpdatedBy { get; set; }

    public SetHazardFileConfidentialityCommand(string fileCode, bool isConfidential, string updatedBy)
    {
        FileCode = fileCode ?? throw new ArgumentNullException(nameof(fileCode));
        IsConfidential = isConfidential;
        UpdatedBy = updatedBy ?? throw new ArgumentNullException(nameof(updatedBy));
    }
}