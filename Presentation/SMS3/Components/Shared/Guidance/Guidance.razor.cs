using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
namespace SMS3.Components.Shared.Components;

public partial class Guidance
{

[Parameter] public string Title { get; set; } = "Guidance";
    [Parameter] public string Icon { get; set; } = "help";
    [Parameter] public RenderFragment? ChildContent { get; set; }
}


