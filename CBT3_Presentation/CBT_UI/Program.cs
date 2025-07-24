using CBT_UI.Components;

using CBT3_Application.Common;
using CBT3_Application.Messaging.CircuitHandlers;
using CBT3_Application.Services;

using CBT3_Infrastructure.Configuration;


using CBT3_Shared;

using CBT3_UI;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Radzen;





namespace CBT_UI;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseWindowsService();
            


        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var configuration = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
              .Build();

        builder.Services.AddSingleton<IConfiguration>(configuration);

        builder.Services.AddRadzenComponents();

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromSeconds(10);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });


        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<CBT3_App>();
        CBT3_UI.DependencyInjection.Initialize(builder.Services, configuration);

        builder.Services.AddScoped<CircuitHandler, CBT_UI_CircuitHandler>();

        //builder.Services.BuildServiceProvider();

        var app = builder.Build();
        
        //var counter = Metrics.CreateCounter("PathCounter", "Counts requests to endpoints", new CounterConfiguration
        //{
        //    LabelNames = new[] { "method", "endpoint" }
        //});
        //app.Use((context, next) =>
        //{
        //    counter.WithLabels(context.Request.Method, context.Request.Path).Inc();
        //    return next();
        //});
        //app.UseMetricServer();
        //app.UseHttpMetrics();

        app.UseMiddleware<CircuitUserTrackingMiddleware>();
        app.UseMiddleware<LoggerMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            var dashboardservice = scope.ServiceProvider.GetRequiredService<DashboardService>();
            dashboardservice.ResetTrainingStationsAsync();
        }

        app.Urls.Add("http://localhost:7231"); // Add your custom URL/port here
        app.Urls.Add("https://localhost:7230"); // If using HTTPS, add this line

        app.UseForwardedHeaders();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseSession();
        //app.UseSessionExpiration(); // Add session expiration middleware
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        


        app.Run();
    }
}

