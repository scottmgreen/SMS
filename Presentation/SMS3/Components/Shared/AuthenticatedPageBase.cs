using Microsoft.AspNetCore.Components;
using SMS_Application.Interfaces;

namespace SMS3.Components.Shared;

/// <summary>
/// Base class for authenticated pages with centralized user context
/// Uses Application layer ICurrentUserService directly for explicit permission checking
/// </summary>
public abstract class AuthenticatedPageBase : ComponentBase
{
    [Inject] protected ICurrentUserService CurrentUserService { get; set; } = default!;

    /// <summary>
    /// Direct access to explicit permission methods - use these in your pages
    /// </summary>
    protected bool CanRead(string module) => CurrentUserService.CanRead(module);
    protected bool CanCreate(string module) => CurrentUserService.CanCreate(module);
    protected bool CanUpdate(string module) => CurrentUserService.CanUpdate(module);
    protected bool CanDelete(string module) => CurrentUserService.CanDelete(module);
    
    /// <summary>
    /// Direct access to user properties
    /// </summary>
    protected string UserCode => CurrentUserService.UserCode;
    protected string UserDisplayName => CurrentUserService.UserDisplayName;
    protected string? UserType => CurrentUserService.UserType;
    protected bool IsAuthenticated => CurrentUserService.IsAuthenticated;
    protected List<SMSUserRolePermission> Permissions => CurrentUserService.Permissions;
    
    /// <summary>
    /// User type as Smart Enum
    /// </summary>
    protected SMSUserType? GetUserTypeEnum() => CurrentUserService.GetUserTypeEnum();
    
    /// <summary>
    /// All accessible modules for the current user
    /// </summary>
    protected List<string> GetAccessibleModules() => CurrentUserService.GetAccessibleModules();
}