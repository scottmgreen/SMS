using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PDXSMS_Presentation.Pages.System;

/// <summary>
/// System Monitoring - Monitor system performance, health, and operational metrics
/// </summary>
public class SystemMonitoringModel : PageModel
{
    public SystemHealthMetrics Metrics { get; set; } = new();

    public void OnGet()
    {
        ViewData["Title"] = "System Monitoring - System Administration";
        LoadSystemMetrics();
    }

    private void LoadSystemMetrics()
    {
        Metrics = new SystemHealthMetrics
        {
            SystemHealth = 98,
            CpuUsage = 23,
            MemoryUsage = 67,
            DiskUsage = 45,
            NetworkLatency = 12,
            DatabaseConnections = 15,
            ActiveSessions = 8,
            LastBackup = DateTime.Now.AddHours(-6),
            SystemUptime = TimeSpan.FromDays(45),
            ErrorCount24h = 2,
            WarningCount24h = 12
        };
    }
}

public class SystemHealthMetrics
{
    public int SystemHealth { get; set; }
    public int CpuUsage { get; set; }
    public int MemoryUsage { get; set; }
    public int DiskUsage { get; set; }
    public int NetworkLatency { get; set; }
    public int DatabaseConnections { get; set; }
    public int ActiveSessions { get; set; }
    public DateTime LastBackup { get; set; }
    public TimeSpan SystemUptime { get; set; }
    public int ErrorCount24h { get; set; }
    public int WarningCount24h { get; set; }
}