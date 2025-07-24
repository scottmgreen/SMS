using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CBT3_Infrastructure.Configuration;
using CBT3_Application.Configuration;
using CBT_Infrastructure.Services;
using CBT3_Domain.Entities;
using CBT3_Shared;
using CBT3_Application.Services;
using CBT3_Application.Interfaces;
using CBT3_Domain.Events.Notifications;
using static CBT3_Application.MediatorHandlers.NotificationHandlers.MediatorNotificationBundle;
using CBT3_Domain.Events.SystemEvents;

namespace CBT3_ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            
             // Add services to the container.
            var services = new ServiceCollection();
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

            services.AddLogging();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddTransient<UserDetails>();
            services.AddInfrastructureServices(configuration);
            services.AddApplicationServices();

            var serviceProvider = services.BuildServiceProvider();

            //var repoService = serviceProvider.GetRequiredService<DataService>();
            //List<Course> courses = repoService.GetAllCoursesAsync().Result;

            //var regservice = serviceProvider.GetRequiredService<TraineeRegistrationService>();
            //Trainee trainee = new(Guid.NewGuid().ToString());
            //bool name_status = regservice.SubmitName("Scott", "Green");


            //if (name_status)
            //{
            //    bool upid_status = regservice.SubmitUPID("1234567");
            //    if (upid_status)
            //    {
            //        upid_status = regservice.SubmitUPID("1234567");
            //    }
            //    if (upid_status)
            //    {
            //        bool yob_status = regservice.SubmitYearOfBirth("1234");
            //        if (yob_status)
            //        {
            //            yob_status = regservice.SubmitYearOfBirth("1234");
            //        }

            //        if (name_status && upid_status && yob_status)
            //        {
            //            trainee = regservice.GetTrainee();
            //        }
            //    }
            //}
            //Console.WriteLine($"Hello, {trainee.ToString()}!");

            var eventaggregator = serviceProvider.GetRequiredService<EventAggregatorService>();
            eventaggregator.Subscribe<ExecuteEvent>(HandleExecuteNotification);
            
            var mediator = serviceProvider.GetRequiredService<MediatorNotificationService>();
            mediator.Register(new TraineeRegisteredNotificationHandler());
            mediator.Publish(new TraineeRegisteredNotification { Name = "Scott Green", UPID = "1234567", YearOfBirth = "1234" });


            Console.ReadLine();


        }

        private static void HandleExecuteNotification(ExecuteEvent @event)
        {
            Console.WriteLine($"=>{@event.Text} ");
        }
    }
}
