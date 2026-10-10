using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Pages.SMSSystem.UserGroups;

public partial class AllowedCompaniesDialog
{

[Parameter] public List<AllowedCompanyDisplayItem> AllowedCompanies { get; set; } = new();
    [Inject] private DialogService _dialogService { get; set; } = default!;

    private void CloseDialog()
    {
        _dialogService.Close();
    }
}


