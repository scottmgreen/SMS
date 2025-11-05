using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using PDXSMS.Services;

namespace PDXSMS_Presentation.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly JsonUserRepository _userRepository;

    public LoginModel(ILogger<LoginModel> logger, JsonUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    [BindProperty]
    public LoginRequestDto LoginRequest { get; set; } = new();

    public void OnGet()
    {
        _logger.LogInformation("*** LOGIN GET: Accessing login page");
        
        // Check if user is already logged in (but not guest)
        var isGuest = HttpContext.Session.GetString("IsGuest") == "true";
        var isAuthenticated = HttpContext.Session.GetString("UserId") != null;
        var currentUserId = HttpContext.Session.GetString("UserId");
        
        _logger.LogInformation("*** LOGIN GET: Current session - IsGuest={IsGuest}, IsAuthenticated={IsAuthenticated}, UserId={UserId}", 
            isGuest, isAuthenticated, currentUserId);
        
        if (isAuthenticated && !isGuest)
        {
            _logger.LogInformation("*** LOGIN GET: User already authenticated, redirecting to dashboard");
            Response.Redirect("/");
            return;
        }

        // If this is a guest session, clear it since they're back at login
        if (isGuest)
        {
            _logger.LogInformation("*** LOGIN GET: Clearing guest session on login page access");
            HttpContext.Session.Clear();
        }

        // Auto-fill email from remember me cookie if it exists
        if (Request.Cookies.ContainsKey("RememberMe"))
        {
            LoginRequest.Email = Request.Cookies["RememberMe"] ?? "";
            _logger.LogInformation("*** LOGIN GET: Auto-filled email from RememberMe cookie: {Email}", LoginRequest.Email);
        }
        
        // Add debug info in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            TempData["DebugMessage"] = "Available test emails: admin@flypdx.com, sarah.johnson@flypdx.com, mike.chen@flypdx.com, lisa.rodriguez@flypdx.com, john.smith@flypdx.com";
            TempData["DebugPassword"] = "Passwords: admin123, safety123, ops123, audit123, user123";
        }
    }

    public async Task<IActionResult> OnPostLoginAsync()
    {
        _logger.LogInformation("*** LOGIN POST: Login attempt started for email: {Email}", LoginRequest.Email);
        
        // Check for existing session
        var existingUserId = HttpContext.Session.GetString("UserId");
        var existingIsGuest = HttpContext.Session.GetString("IsGuest") == "true";
        
        _logger.LogInformation("*** LOGIN POST: Existing session - UserId={UserId}, IsGuest={IsGuest}", 
            existingUserId, existingIsGuest);
        
        // Clear any existing guest session first
        if (existingIsGuest)
        {
            _logger.LogInformation("*** LOGIN POST: Clearing existing guest session during login attempt");
            HttpContext.Session.Clear();
        }
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("*** LOGIN POST: Login form validation failed for email: {Email}", LoginRequest.Email);
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                _logger.LogWarning("*** LOGIN POST: Validation error: {Error}", error.ErrorMessage);
            }
            TempData["ErrorMessage"] = "Please check the form for errors.";
            return Page();
        }

        try
        {
            _logger.LogInformation("*** LOGIN POST: Attempting to validate user credentials for: {Email}", LoginRequest.Email);
            
            // Use the new JSON-based user repository
            var isValidUser = await _userRepository.ValidatePasswordAsync(LoginRequest.Email, LoginRequest.Password);
            
            _logger.LogInformation("*** LOGIN POST: Password validation result for {Email}: {IsValid}", 
                LoginRequest.Email, isValidUser);
            
            if (isValidUser)
            {
                var user = await _userRepository.GetUserByUsernameAsync(LoginRequest.Email);
                
                if (user != null)
                {
                    _logger.LogInformation("*** LOGIN POST: User found: {DisplayName}, Active: {IsActive}, Locked: {IsLocked}", 
                        user.DisplayName, user.IsActive, user.IsLockedOut);
                    
                    // Check if user is locked out
                    if (user.IsLockedOut)
                    {
                        _logger.LogWarning("*** LOGIN POST: User {Email} is locked out until {LockoutEnd}", 
                            LoginRequest.Email, user.LockoutEndDate);
                        TempData["ErrorMessage"] = "Your account is temporarily locked due to multiple failed login attempts. Please try again later.";
                        return Page();
                    }

                    if (!user.IsActive)
                    {
                        _logger.LogWarning("*** LOGIN POST: User {Email} is inactive", LoginRequest.Email);
                        TempData["ErrorMessage"] = "Your account is inactive. Please contact the administrator.";
                        return Page();
                    }

                    _logger.LogInformation("*** LOGIN POST: Successful login validation for user: {Email}", LoginRequest.Email);
                    
                    // Update last login date
                    await _userRepository.UpdateLastLoginAsync(LoginRequest.Email);
                    
                    // Clear session completely first
                    HttpContext.Session.Clear();
                    
                    // Set comprehensive session data using the unified user architecture
                    HttpContext.Session.SetString("UserId", user.Id);
                    HttpContext.Session.SetString("UserName", user.DisplayName);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetString("Department", user.Department);
                    HttpContext.Session.SetString("JobTitle", user.JobTitle);
                    HttpContext.Session.SetString("ApplicationRoles", string.Join(",", user.ApplicationRoles));
                    HttpContext.Session.SetString("Permissions", string.Join(",", user.Permissions));
                    
                    // Contact and notification preferences
                    HttpContext.Session.SetString("CanReceiveEmail", user.NotificationPreferences.EmailEnabled.ToString());
                    HttpContext.Session.SetString("CanReceiveSMS", user.NotificationPreferences.SmsEnabled.ToString());
                    HttpContext.Session.SetString("CanReceiveInApp", user.NotificationPreferences.InAppEnabled.ToString());
                    
                    if (!string.IsNullOrEmpty(user.Phone))
                        HttpContext.Session.SetString("Phone", user.Phone);
                    
                    // CRITICAL: Make sure IsGuest is NOT set for regular users 
                    // (Don't set IsGuest at all for regular users)
                    
                    _logger.LogInformation("*** LOGIN POST: Session data set successfully for {Email}", LoginRequest.Email);
                    
                    // Verify session was set correctly
                    var verifyUserId = HttpContext.Session.GetString("UserId");
                    var verifyIsGuest = HttpContext.Session.GetString("IsGuest");
                    
                    _logger.LogInformation("*** LOGIN POST: Session verification - UserId={UserId}, IsGuest={IsGuest}", 
                        verifyUserId, verifyIsGuest);
                    
                    // Set remember me cookie if requested
                    if (LoginRequest.RememberMe)
                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30),
                            HttpOnly = true,
                            Secure = Request.IsHttps,
                            SameSite = SameSiteMode.Strict
                        };
                        Response.Cookies.Append("RememberMe", LoginRequest.Email, cookieOptions);
                        _logger.LogInformation("*** LOGIN POST: Remember me cookie set for {Email}", LoginRequest.Email);
                    }

                    TempData["SuccessMessage"] = $"Welcome to PDXSMS, {user.DisplayName}! You have been successfully logged in.";
                    
                    _logger.LogInformation("*** LOGIN POST: Login process completed successfully for {Email}", LoginRequest.Email);
                    
                    // Redirect to dashboard or return URL
                    var returnUrl = Request.Query["returnUrl"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        _logger.LogInformation("*** LOGIN POST: Redirecting to return URL: {ReturnUrl}", returnUrl);
                        return Redirect(returnUrl);
                    }
                    
                    _logger.LogInformation("*** LOGIN POST: Redirecting to dashboard");
                    return RedirectToPage("/Index");
                }
                else
                {
                    _logger.LogError("*** LOGIN POST: User validation returned true but GetUserByUsernameAsync returned null for {Email}", LoginRequest.Email);
                }
            }
            
            // Authentication failed
            _logger.LogWarning("*** LOGIN POST: Authentication failed for email: {Email}", LoginRequest.Email);
            
            // Record failed attempt
            await _userRepository.RecordFailedLoginAsync(LoginRequest.Email);
            
            TempData["ErrorMessage"] = "Invalid email address or password. Please try again.";
            
            // Add debugging info in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                var allUsers = await _userRepository.GetAllUsersAsync();
                var availableEmails = allUsers.Where(u => u.IsActive).Select(u => u.Email).ToList();
                TempData["DebugMessage"] = $"Available test emails: {string.Join(", ", availableEmails)}";
                TempData["DebugPassword"] = "Check AppData/users.json for hashed passwords. Expected passwords: admin123, safety123, ops123, audit123, user123";
            }
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "*** LOGIN POST: Exception during login process for email: {Email}", LoginRequest.Email);
            TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
            
            // Add exception details in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                TempData["DebugMessage"] = $"Exception: {ex.Message}";
            }
            
            return Page();
        }
    }

    public IActionResult OnPostGuestLogin()
    {
        _logger.LogInformation("*** LOGIN POST: Guest login initiated for confidential reporting");

        try
        {
            // Clear any existing session first
            HttpContext.Session.Clear();

            // Set guest session data with enhanced security
            HttpContext.Session.SetString("IsGuest", "true");
            HttpContext.Session.SetString("UserId", "GUEST_" + Guid.NewGuid().ToString("N")[..8]);
            HttpContext.Session.SetString("UserName", "Anonymous Guest");
            HttpContext.Session.SetString("ApplicationRoles", "Guest");
            HttpContext.Session.SetString("Permissions", "SubmitConfidentialReport");
            HttpContext.Session.SetString("Department", "Anonymous");
            HttpContext.Session.SetString("GuestStartTime", DateTime.UtcNow.ToString());

            _logger.LogInformation("*** LOGIN POST: Guest session created with UserId={UserId}", 
                HttpContext.Session.GetString("UserId"));

            // Redirect directly to confidential reporting page
            return RedirectToPage("/SafetyRiskManagement/ConfidentialReporting");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "*** LOGIN POST: Error during guest login process");
            TempData["ErrorMessage"] = "An error occurred accessing the confidential reporting system. Please try again.";
            return Page();
        }
    }

    public IActionResult OnPostClearGuestSession()
    {
        _logger.LogInformation("*** LOGIN POST: Clearing guest session via explicit request");
        HttpContext.Session.Clear();
        return new JsonResult(new { success = true });
    }

    public IActionResult OnGetForgotPassword()
    {
        TempData["InfoMessage"] = "Password reset functionality uses the unified user architecture. Check AppData/users.json for development passwords.";
        return Page();
    }

    public async Task<IActionResult> OnGetUserInfoAsync()
    {
        // Development endpoint to see user data
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
        {
            return NotFound();
        }

        var users = await _userRepository.GetAllUsersAsync();
        var userInfo = users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.DisplayName,
            u.Department,
            u.JobTitle,
            u.ApplicationRoles,
            u.Permissions,
            u.IsActive,
            u.ContactSummary,
            NotificationChannels = new
            {
                u.NotificationPreferences.EmailEnabled,
                u.NotificationPreferences.SmsEnabled,
                u.NotificationPreferences.InAppEnabled
            },
            u.NotificationPreferences.TopicSubscriptions,
            u.LastLoginDate,
            u.FailedLoginAttempts,
            u.IsLockedOut,
            PasswordHint = u.Username switch
            {
                "admin@flypdx.com" => "admin123",
                "sarah.johnson@flypdx.com" => "safety123",
                "mike.chen@flypdx.com" => "ops123",
                "lisa.rodriguez@flypdx.com" => "audit123",
                "john.smith@flypdx.com" => "user123",
                _ => "unknown"
            }
        }).ToList();

        return new JsonResult(userInfo);
    }
}

public class LoginRequestDto
{
    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}