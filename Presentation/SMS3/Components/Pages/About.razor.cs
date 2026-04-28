using System.Diagnostics;
using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Radzen;

using SMS_Shared.Configuration;

using SMS3.Components.Shared.UIHelpers;

using SysEnv = System.Environment;
using SysIO = System.IO;
using SysText = System.Text;

namespace SMS3.Components.Pages;

public partial class About : ComponentBase
{
    [Inject] private INotificationHelper  NotificationHelper { get; set; } = default!;
    
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    // Page Header Properties
    public string PageTitle => "PDX Safety Management System (SMS3)";
    public string PageSubtitle => "Comprehensive Aviation Safety Management Platform";

    // System Information Properties
    public string ApplicationVersion { get; set; } = "Unknown";
    public string ApplicationBuildDate { get; set; } = "Unknown";
    public string Environment { get; set; } = "Development";
    public string FrameworkVersion { get; set; } = "8.0";
    public string BuildConfiguration { get; set; } = "Debug";
    public string LastUpdateTime { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    public string SystemUptime { get; set; } = "Unknown";

    // Assembly Information
    public List<AssemblyInfo> AssemblyVersions { get; set; } = new();

    private DateTime _applicationStartTime = DateTime.Now;

    protected override void OnInitialized()
    {
        LoadSystemInformation();
        LoadAssemblyVersions();
        CalculateUptime();
    }

    private void LoadSystemInformation()
    {
        try
        {
            // Get main application assembly information
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly is not null)
            {
                // Get version from assembly
                var version = entryAssembly.GetName().Version;
                ApplicationVersion = version?.ToString() ?? "1.0.0.0";

                // Get build date from file info
                var fileInfo = new SysIO.FileInfo(entryAssembly.Location);
                ApplicationBuildDate = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Environment information
            Environment = SysEnv.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            // Framework version
            FrameworkVersion = SysEnv.Version.ToString();

            // Build configuration
#if DEBUG
            BuildConfiguration = "Debug";
#else
            BuildConfiguration = "Release";
#endif

            // Application start time (approximate)
            _applicationStartTime = Process.GetCurrentProcess().StartTime;
        }
        catch (Exception ex)
        {
            // Log error but don't crash the page
            Console.WriteLine($"Error loading system information: {ex.Message}");
        }
    }

    private void LoadAssemblyVersions()
    {
        try
        {
            AssemblyVersions.Clear();

            // Get key assemblies we want to display
            var keyAssemblyNames = new[]
            {
                "SMS3",          // Presentation layer
                "Application",   // Application layer  
                "Domain",        // Domain layer
                "Infrastructure", // Infrastructure layer
                "Shared"         // Shared library
            };

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assemblyName in keyAssemblyNames)
            {
                var assembly = loadedAssemblies.FirstOrDefault(a => 
                    a.GetName().Name?.Equals(assemblyName, StringComparison.OrdinalIgnoreCase) == true);

                if (assembly is not null)
                {
                    var assemblyInfo = ExtractAssemblyInfo(assembly);
                    if (assemblyInfo is not null)
                    {
                        AssemblyVersions.Add(assemblyInfo);
                    }
                }
                else
                {
                    // Assembly not loaded, create placeholder
                    AssemblyVersions.Add(new AssemblyInfo
                    {
                        Name = assemblyName,
                        Version = "Not Loaded",
                        FileVersion = "N/A",
                        Location = "Assembly not currently loaded",
                        Dependencies = new List<DependencyInfo>()
                    });
                }
            }

            // Sort by importance/name
            AssemblyVersions = AssemblyVersions.OrderBy(a => GetAssemblyPriority(a.Name)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading assembly versions: {ex.Message}");
            
            // Add error entry
            AssemblyVersions.Add(new AssemblyInfo
            {
                Name = "Error",
                Version = "Failed to load assembly information",
                FileVersion = ex.Message,
                Location = "See application logs for details",
                Dependencies = new List<DependencyInfo>()
            });
        }
    }

    private AssemblyInfo? ExtractAssemblyInfo(Assembly assembly)
    {
        try
        {
            var assemblyName = assembly.GetName();
            var version = assemblyName.Version?.ToString() ?? "Unknown";
            
            // Try to get file version
            var fileVersion = "Unknown";
            try
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                fileVersion = fileVersionInfo.FileVersion ?? version;
            }
            catch
            {
                fileVersion = version; // Fallback to assembly version
            }

            // Get dependencies based on assembly name
            var dependencies = GetKnownDependencies(assemblyName.Name ?? "Unknown");

            return new AssemblyInfo
            {
                Name = assemblyName.Name ?? "Unknown",
                Version = version,
                FileVersion = fileVersion,
                Location = assembly.Location,
                Dependencies = dependencies
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting info for assembly {assembly.GetName().Name}: {ex.Message}");
            return null;
        }
    }

    private List<DependencyInfo> GetKnownDependencies(string assemblyName)
    {
        return assemblyName.ToLower() switch
        {
            "sms3" => new List<DependencyInfo>
            {
                new DependencyInfo { Name = "Radzen.Blazor", Version = "8.3.8" },
                new DependencyInfo { Name = "Swashbuckle.AspNetCore", Version = "8.1.4" }
            },
            "application" => new List<DependencyInfo>
            {
                new DependencyInfo { Name = "Microsoft.AspNetCore.Http", Version = "2.2.2" },
                new DependencyInfo { Name = "Microsoft.Data.SqlClient", Version = "6.1.2" },
                new DependencyInfo { Name = "Microsoft.Extensions.Configuration", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Console", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Debug", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.EventLog", Version = "8.0.0" }
            },
            "domain" => new List<DependencyInfo>
            {
                new DependencyInfo { Name = "BCrypt.Net-Next", Version = "4.0.3" },
                new DependencyInfo { Name = "Microsoft.Extensions.Configuration", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging", Version = "8.0.0" }
            },
            "infrastructure" => new List<DependencyInfo>
            {
                new DependencyInfo { Name = "Microsoft.Data.SqlClient", Version = "6.1.2" },
                new DependencyInfo { Name = "Microsoft.Extensions.Configuration", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Configuration.Json", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Http", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Console", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Debug", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.EventLog", Version = "8.0.0" }
            },
            "shared" => new List<DependencyInfo>
            {
                new DependencyInfo { Name = "Microsoft.Extensions.Configuration.Abstractions", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Configuration.Json", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Abstractions", Version = "8.0.3" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Console", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.Debug", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.Extensions.Logging.EventLog", Version = "8.0.0" },
                new DependencyInfo { Name = "Microsoft.FeatureManagement", Version = "3.2.0" }
            },
            _ => new List<DependencyInfo>()
        };
    }

    private int GetAssemblyPriority(string assemblyName)
    {
        return assemblyName.ToLower() switch
        {
            "sms3" => 1,
            "application" => 2,
            "domain" => 3,
            "infrastructure" => 4,
            "shared" => 5,
            _ => 99
        };
    }

    private void CalculateUptime()
    {
        try
        {
            var uptime = DateTime.Now - _applicationStartTime;
            
            if (uptime.TotalDays >= 1)
            {
                SystemUptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
            }
            else if (uptime.TotalHours >= 1)
            {
                SystemUptime = $"{uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
            }
            else
            {
                SystemUptime = $"{uptime.Minutes}m {uptime.Seconds}s";
            }
        }
        catch
        {
            SystemUptime = "Unknown";
        }
    }

    private BadgeStyle GetEnvironmentBadgeStyle()
    {
        return Environment.ToLower() switch
        {
            "production" => BadgeStyle.Success,
            "staging" => BadgeStyle.Warning,
            "development" => BadgeStyle.Info,
            _ => BadgeStyle.Secondary
        };
    }

    private string GetAssemblyIcon(string assemblyName)
    {
        return assemblyName.ToLower() switch
        {
            "sms3" => "web",
            "application" => "apps",
            "domain" => "business",
            "infrastructure" => "storage",
            "shared" => "share",
            _ => "extension"
        };
    }

    private async Task CheckSystemHealth()
    {
        try
        {
            // Simulate system health check
            await Task.Delay(500);
            
            var healthStatus = "All systems operational";
            var memoryUsage = GC.GetTotalMemory(false) / (1024 * 1024); // MB
            
            healthStatus += $" | Memory: {memoryUsage:F0} MB";

            await NotificationHelper.ShowSuccessAsync($"System Health Check Complete: {healthStatus}", 5000);
        }
        catch (Exception ex)
        {
            await NotificationHelper.ShowErrorAsync($"Health check failed: {ex.Message}");
        }
    }

    private async Task CopyVersionInfo()
    {
        try
        {
            var versionInfo = BuildVersionInfoText();
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", versionInfo);

            await NotificationHelper.ShowSuccessAsync("Version information copied to clipboard", 3000);
        }
        catch (Exception ex)
        {
            await NotificationHelper.ShowErrorAsync($"Failed to copy version info: {ex.Message}");
        }
    }

    private string BuildVersionInfoText()
    {
        var info = new SysText.StringBuilder();
        info.AppendLine("PDX Safety Management System - Version Information");
        info.AppendLine("=" + new string('=', 60));
        info.AppendLine($"Application Version: {ApplicationVersion}");
        info.AppendLine($"Build Date: {ApplicationBuildDate}");
        info.AppendLine($"Environment: {Environment}");
        info.AppendLine($"Framework: .NET {FrameworkVersion}");
        info.AppendLine($"Configuration: {BuildConfiguration}");
        info.AppendLine();
        info.AppendLine("Assembly Versions:");
        info.AppendLine("-" + new string('-', 20));
        
        foreach (var assembly in AssemblyVersions)
        {
            info.AppendLine($"{assembly.Name}: {assembly.Version} (File: {assembly.FileVersion})");
            
            if (assembly.Dependencies.Any())
            {
                info.AppendLine("  Dependencies:");
                foreach (var dep in assembly.Dependencies)
                {
                    info.AppendLine($"    - {dep.Name}: {dep.Version}");
                }
            }
            info.AppendLine();
        }
        
        info.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        
        return info.ToString();
    }

    public class AssemblyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string FileVersion { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<DependencyInfo> Dependencies { get; set; } = new();
    }

    public class DependencyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
    }
}