//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Net;

namespace CBT3_ConsoleApp;

public static class CBT3_ServiceProvider
{
    
public static void Initialize()
    {
        ServiceCollection services = new ServiceCollection();
        IConfigurationRoot configuration = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
       
        services.AddSingleton<IConfiguration>(configuration);
        services.AddTransient<UserDetails>();
        services.AddSharedServices(configuration);
        services.AddInfrastructureServices(configuration);
        services.AddApplicationServices();


        ServiceProvider = services.BuildServiceProvider();

    }
    
    public static IServiceProvider ServiceProvider { get; private set; }


}
