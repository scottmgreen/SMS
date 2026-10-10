using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
namespace SMS3.Components.Shared.Components;

public partial class UserProfileDialog
{

[Parameter] public ICurrentUserService? _currentUserService { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private string _moduleToCheck = "";
    private string _permissionCheckResult = "";
    private List<SMSUserRolePermission> _checkedModulePermissions = new();

    // Get unique modules from user permissions
    private string[] SMSModules
    {
        get
        {
            if (_currentUserService?.Permissions?.Any() != true)
                return Array.Empty<string>();

            return _currentUserService.Permissions
                .Select(p => p.SMSModule)
                .Where(module => !string.IsNullOrWhiteSpace(module))
                .Cast<string>() // Ensure non-nullable after null filtering
                .Distinct()
                .OrderBy(module => module)
                .ToArray();
        }
    }

    private async Task CloseModal()
    {
        await OnClose.InvokeAsync();
    }

    private bool IsPermissionGranted(string module, string action)
    {
        if (_currentUserService?.Permissions == null) return false;

        var permission = _currentUserService.Permissions.FirstOrDefault(p => p.SMSModule == module);
        return action switch
        {
            "Create" => permission?.Create == true,
            "Read" => permission?.Read == true,
            "Update" => permission?.Update == true,
            "Delete" => permission?.Delete == true,
            _ => false
        };
    }

    private int GetPermissionCount(string action)
    {
        if (_currentUserService?.Permissions == null) return 0;

        return action switch
        {
            "Create" => _currentUserService.Permissions.Count(p => p.Create),
            "Read" => _currentUserService.Permissions.Count(p => p.Read),
            "Update" => _currentUserService.Permissions.Count(p => p.Update),
            "Delete" => _currentUserService.Permissions.Count(p => p.Delete),
            _ => 0
        };
    }

    private void CheckModulePermissions()
    {
        if (string.IsNullOrEmpty(_moduleToCheck) || _currentUserService?.Permissions == null)
        {
            _permissionCheckResult = "Please enter a module name.";
            _checkedModulePermissions = new();
            return;
        }

        _checkedModulePermissions = _currentUserService.Permissions
            .Where(p => p.SMSModule?.Contains(_moduleToCheck, StringComparison.OrdinalIgnoreCase) == true)
            .GroupBy(p => p.SMSModule) // Group by module name to eliminate duplicates
            .Select(g => g.First()) // Take the first permission from each group
            .ToList();

        if (_checkedModulePermissions.Any())
        {
            _permissionCheckResult = $"Found {_checkedModulePermissions.Count} module(s) matching '{_moduleToCheck}'";
        }
        else
        {
            _permissionCheckResult = $"No permissions found for module containing '{_moduleToCheck}'";
        }
    }
}


