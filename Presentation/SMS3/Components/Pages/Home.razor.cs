using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
namespace SMS3.Components.Pages;

public partial class Home
{

private bool IsAuthenticated => _currentUserService.IsAuthenticated;
    private string? CurrentUserName => _currentUserService.UserDisplayName;
    private string? CurrentUserType => _currentUserService.UserType;
}


