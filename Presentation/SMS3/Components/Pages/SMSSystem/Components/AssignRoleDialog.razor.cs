using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Domain.Entities;
namespace SMS3.Components.Pages.SMSSystem.Components;

public partial class AssignRoleDialog
{

[Parameter] public string UserId { get; set; } = "";
    [Parameter] public string DisplayName { get; set; } = "";
    [Parameter] public string CurrentRoleCode { get; set; } = "";
    [Parameter] public IEnumerable<SMSUserRole> UserRoles { get; set; } = new List<SMSUserRole>();
    [CascadingParameter] public DialogService? DialogService { get; set; }

    private string SelectedRoleCode { get; set; } = "";

    protected override void OnInitialized()
    {
        SelectedRoleCode = CurrentRoleCode;
    }

    private void AssignRole()
    {
        DialogService?.Close(SelectedRoleCode);
    }

    private void Cancel()
    {
        DialogService?.Close();
    }

    private string GetCurrentRoleName()
    {
        var role = UserRoles.FirstOrDefault(r => r.Code == CurrentRoleCode);
        return role?.Name ?? CurrentRoleCode;
    }
}


