using Microsoft.Extensions.Configuration;
using System.IO;

namespace CBT_Browser;
public class SettingsHelper
{
    private readonly IConfigurationRoot _configuration;

    public SettingsHelper()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Set the base path to the current directory
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true); // Load the appsettings.json file

        _configuration = builder.Build();
    }

    public string GetAppSetting(string key)
    {
        return _configuration[key];
    }
}