//-----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="">
//     Author: Scott Green
//     Copyright (c) . All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
//using System.Runtime.CompilerServices;


using Microsoft.FeatureManagement;

namespace CBT3_Application.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {

            services.AddScoped<IMessenger, MessengerService>();
            services.AddMediator(Assembly.GetExecutingAssembly());
           
            services.AddScoped<SystemService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<TrainingService>();
            services.AddScoped<RegistrationService>();
            services.AddScoped<CourseMachine>();
            services.AddScoped<LessonQuizService>();

           // ServiceProvider = services.BuildServiceProvider();

            return services;
        }

       // public static IServiceProvider ServiceProvider { get; private set; }


    }
}

