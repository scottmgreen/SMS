using Radzen;

using SMS_Application.Configuration;

using SMS_Infrastructure.Configuration;

using SMS_Shared.Configuration;
using SMS3.Components.Layout;

using SMS3.Components;
using SMS3.Configuration;

namespace SMS3;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddRadzenComponents();

        // Add session services for SMS session management
        //builder.Services.AddDistributedMemoryCache();
        //builder.Services.AddSession(options =>
        //{
        //    options.IdleTimeout = TimeSpan.FromMinutes(30);
        //    options.Cookie.HttpOnly = true;
        //    options.Cookie.IsEssential = true;
        //    options.Cookie.Name = "SMS3_Session";
        //    options.Cookie.SameSite = SameSiteMode.Lax;
        //});
        
        // **NEW**: Register authentication service as singleton
        builder.Services.AddSingleton<AuthenticationService>();

        builder.Services.AddSharedServices(builder.Configuration);
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();
        
        // SMS Session Management
        builder.Services.ConfigureSMSSession();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        // Add session middleware
        app.UseSession();

        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
