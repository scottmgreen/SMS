using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Pages.SMSRiskManagement.Models;
namespace SMS3.Components.Pages.SMSRiskManagement.Components;

public partial class UploadFileSection
{

[Parameter] public List<AttachedFile> AttachedFiles { get; set; } = new();
    [Parameter] public bool HasMissingFileDescriptions { get; set; }
    [Parameter] public string MissingDescriptionMessage { get; set; } = "Each attached file requires a description.";
    [Parameter] public string AcceptedFileExtensions { get; set; } = ".jpg,.jpeg,.png,.pdf,.doc,.docx,.txt";
    [Parameter] public int MaxFileSize { get; set; } = 10485760;
    [Parameter] public EventCallback<UploadChangeEventArgs> OnInputFileChange { get; set; }
    [Parameter] public EventCallback<int> OnRemoveFile { get; set; }
    [Parameter] public EventCallback OnClearAllFiles { get; set; }
    [Parameter] public EventCallback OnFilesChanged { get; set; }
}


