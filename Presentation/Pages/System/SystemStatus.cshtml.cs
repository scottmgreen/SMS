using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages;

/// <summary>
/// System Status - UI Implementation Overview
/// Shows current status of all SMS pages
/// </summary>
public class SystemStatusModel : PageModel
{
    public SystemStatistics Statistics { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "SMS System Status - UI Implementation Overview";
        LoadStatistics();
    }

    private void LoadStatistics()
    {
        Statistics = new SystemStatistics
        {
            Section1Pages = 6,
            Section1Complete = 6,
            Section2Pages = 3,
            Section2Complete = 3,
            Section3Pages = 3,
            Section3Complete = 0,
            Section4Pages = 4,
            Section4Complete = 0,
            Section5Pages = 4,
            Section5Complete = 1,
            TotalPages = 20,
            CompletedPages = 10
        };
    }
}

public class SystemStatistics
{
    public int Section1Pages { get; set; }
    public int Section1Complete { get; set; }
    public int Section2Pages { get; set; }
    public int Section2Complete { get; set; }
    public int Section3Pages { get; set; }
    public int Section3Complete { get; set; }
    public int Section4Pages { get; set; }
    public int Section4Complete { get; set; }
    public int Section5Pages { get; set; }
    public int Section5Complete { get; set; }
    public int TotalPages { get; set; }
    public int CompletedPages { get; set; }
    
    public double CompletionPercentage => TotalPages > 0 ? (double)CompletedPages / TotalPages * 100 : 0;
}