//-----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure layer dependency injection configuration providing service registration, repository setup, and data service coordination.
//                  Infrastructure configuration providing dependency injection,
//                  service registration, and system setup.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Configuration;

public static class DependencyInjection
{

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddHttpContextAccessor();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IUserDetailsFactory, UserDetailsFactory>();
        services.AddScoped<ILogSupport, LogSupport>();
        services.AddScoped<LogSupport>();
        services.AddSingleton<FileService>();
        services.AddDataServices(configuration);

        return services;
    }

}

