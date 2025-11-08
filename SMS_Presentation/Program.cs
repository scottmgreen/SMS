using SMS_Infrastructure.Configuration;
using SMS_Application.Configuration;
using SMS_Shared.Configuration;
using SMS.Presentation.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// ? DIRECT SMS BACKEND INTEGRATION - NO HELPER SERVICES
builder.Services.AddSharedServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(); // This includes IMediator registration

// ? SMS Session Management (no helper services)
builder.Services.ConfigureSMSSession();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ? SMS Session and Authentication (direct session management)
app.UseSession();
app.UseSMSAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

/*
? DIRECT SMS BACKEND INTEGRATION:

REMOVED:
? SMSAuthorizationService helper class
? LoginModel/IndexModel helper classes
? Any wrapper services

USES ONLY:
? SMS Backend via IMediator/CQRS
? SMS Domain Entities directly
? SMS Repository layer through queries
? Direct session management with Domain Entity data

AUTHENTICATION FLOW:
1. Login page uses IMediator directly
2. Calls GetSMSApplicationUserByUserNameQuery etc.
3. Gets actual Domain Entities from repositories
4. Creates session with Domain Entity properties
5. No intermediate helper classes!

This is exactly what you wanted - pure SMS Backend integration!
*/