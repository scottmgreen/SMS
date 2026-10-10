using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
namespace SMS3.Components.Shared.Components;

public partial class PageHeader
{

/// <summary>
    /// The main title text for the page
    /// </summary>
    [Parameter, EditorRequired]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional subtitle/description text
    /// </summary>
    [Parameter]
    public string? Subtitle { get; set; }

    /// <summary>
    /// Optional FontAwesome icon class (e.g., "fas fa-exclamation-triangle")
    /// </summary>
    [Parameter]
    public string? Icon { get; set; }

    /// <summary>
    /// Optional additional content to display on the right side of the header
    /// </summary>
    [Parameter]
    public RenderFragment? AdditionalContent { get; set; }
}


