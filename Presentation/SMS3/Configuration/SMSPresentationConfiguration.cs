using SMS_Application.Services;

namespace SMS3.Configuration;

/// <summary>
/// SMS Presentation Configuration - Static Authentication Approach
/// Clean architecture with no session or HttpContext dependencies
/// </summary>

/*
✅ SMS PRESENTATION CONFIGURATION - STATIC AUTHENTICATION APPROACH:

The SMS application now uses static authentication storage instead of sessions:

STATIC AUTHENTICATION FLOW:
1. User enters credentials in Login.razor
2. AuthenticationService validates credentials using CQRS queries
3. StaticCurrentUserService.SetAuthenticationState() stores user data in static fields
4. NavMenu and authorization checks use ICurrentUserService (StaticCurrentUserService)
5. Logout calls StaticCurrentUserService.ClearAuthenticationState() and navigates to home

BENEFITS:
✅ No HttpContext dependencies - works in any deployment environment
✅ No session configuration required - bypasses IIS session issues
✅ No cookie encryption problems - no cookies needed
✅ Direct integration with SMS Backend via Mediator/CQRS
✅ Uses actual Domain Entities without wrapper classes
✅ Clean separation between business logic and infrastructure concerns

NOTIFICATION SETTINGS:
NotificationSettings is configured in SMS_Shared.Configuration.DependencyInjection
and used throughout the presentation layer for UI notifications.

This approach eliminates deployment environment issues while maintaining 
full authentication and authorization functionality.
*/