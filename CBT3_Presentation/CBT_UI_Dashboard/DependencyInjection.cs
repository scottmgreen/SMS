//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Net;

using CBT3_Shared.Configuration;
using CBT3_Application.Configuration;
using CBT3_Infrastructure.Configuration;
using Radzen;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CBT3_UI_Dashboard;

public static class DependencyInjection
{
    
public static IServiceCollection Initialize(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedServices(configuration);//services.BuildServiceProvider().GetRequiredService<IConfiguration>());
        services.AddInfrastructureServices(configuration); //services.BuildServiceProvider().GetRequiredService<IConfiguration>());
        services.AddApplicationServices();

        return services;
    }
    
    


}
