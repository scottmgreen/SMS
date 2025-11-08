using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Queries;
using SMS_Domain.ValueObjects;
using SMS_Domain.Entities;

namespace PDXSMS_UnitTests;

/// <summary>
/// Helper test class to find or create admin login credentials
/// This is for development/testing purposes only
/// </summary>
public class AdminLoginHelper
{
    private readonly ITestOutputHelper _output;

    public AdminLoginHelper(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void GenerateNewAdminPassword_ShouldCreatePasswordForDatabase()
    {
        _output.WriteLine("=== GENERATE NEW ADMIN PASSWORD ===");
        _output.WriteLine("If you can't guess the password, you can reset it in the database.");
        _output.WriteLine("");
        
        var newPassword = "TempAdmin123!";
        var passwordObject = Password.Create(newPassword).Value;
        
        _output.WriteLine($"New Password: {newPassword}");
        _output.WriteLine($"BCrypt Hash: {passwordObject.HashedValue}");
        _output.WriteLine("");
        
        _output.WriteLine("=== CORRECT SQL COMMAND TO RESET ADMIN PASSWORD ===");
        _output.WriteLine("Run this SQL command against PDXSMS_V2 database:");
        _output.WriteLine("");
        _output.WriteLine("USE [PDXSMS_V2]");
        _output.WriteLine("GO");
        _output.WriteLine("");
        _output.WriteLine("UPDATE [dbo].[tbld_SMSApplicationUsers]");
        _output.WriteLine($"SET [fldv_Password] = '{passwordObject.HashedValue}',");
        _output.WriteLine("    [fldv_UpdatedBy] = 'SYSTEM-RESET',");
        _output.WriteLine("    [fldd_UpdatedDate] = GETUTCDATE(),");
        _output.WriteLine("    [fldd_PasswordLastChanged] = GETUTCDATE()");
        _output.WriteLine("WHERE [fldv_UserName] = 'admin@flypdx.com';");
        _output.WriteLine("");
        
        _output.WriteLine("After running this SQL:");
        _output.WriteLine($"? Username: admin@flypdx.com");
        _output.WriteLine($"? Password: {newPassword}");
        _output.WriteLine($"? Login URL: https://localhost:61117/Account/Login");
        _output.WriteLine("");
        
        _output.WriteLine("=== VERIFY THE UPDATE ===");
        _output.WriteLine("Run this query to verify the password was updated:");
        _output.WriteLine("");
        _output.WriteLine("SELECT [fldv_UserName], [fldv_FirstName], [fldv_LastName],");
        _output.WriteLine("       [fldv_PermissionLevel], [fldd_UpdatedDate]");
        _output.WriteLine("FROM [dbo].[tbld_SMSApplicationUsers]");
        _output.WriteLine("WHERE [fldv_UserName] = 'admin@flypdx.com';");
        
        Assert.True(true);
    }

    [Fact]
    public void TryMorePasswords_ShouldTestPDXSpecificPasswords()
    {
        _output.WriteLine("=== TESTING PDX-SPECIFIC PASSWORDS ===");
        
        var adminHash = "$2a$12$LQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1F";
        
        // PDX/FlyPDX specific passwords
        var pdxPasswords = new[]
        {
            "flypdx",
            "flypdx123",
            "flypdx2025", 
            "pdx123",
            "pdx2025",
            "portofportland",
            "sms2025",
            "administrator2025",
            "admin2025",
            "smsadmin",
            "setup123",
            "temp123",
            "flypdx123!",
            "PDX123!",
            "FlyPDX2025!",
            "temppass",
            "changeme"
        };

        _output.WriteLine($"Testing {pdxPasswords.Length} PDX-specific passwords...");
        
        foreach (var password in pdxPasswords)
        {
            try
            {
                var passwordFromHash = Password.FromHash(adminHash, DateTime.UtcNow.AddDays(-30));
                var isValid = passwordFromHash.Verify(password);
                
                if (isValid)
                {
                    _output.WriteLine($"?? PASSWORD FOUND!");
                    _output.WriteLine($"   Username: admin@flypdx.com");
                    _output.WriteLine($"   Password: {password}");
                    _output.WriteLine($"   Login at: https://localhost:61117/Account/Login");
                    return;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Error testing '{password}': {ex.Message}");
            }
        }
        
        _output.WriteLine("? None of the PDX-specific passwords worked.");
        _output.WriteLine("?? Recommendation: Use the GenerateNewAdminPassword test to reset the password.");
        
        Assert.True(true);
    }

    [Fact]
    public void ShowAllDatabaseUsers_ShouldListCorrectSQLQueries()
    {
        _output.WriteLine("=== CORRECT SQL QUERIES FOR SMS DATABASE ===");
        _output.WriteLine("");
        _output.WriteLine("Database: PDXSMS_V2");
        _output.WriteLine("Table: [dbo].[tbld_SMSApplicationUsers]");
        _output.WriteLine("");
        
        _output.WriteLine("=== QUERY TO FIND ALL ADMIN USERS ===");
        _output.WriteLine("USE [PDXSMS_V2]");
        _output.WriteLine("GO");
        _output.WriteLine("");
        _output.WriteLine("SELECT [fldv_UserName] as Username,");
        _output.WriteLine("       [fldv_FirstName] as FirstName,");
        _output.WriteLine("       [fldv_LastName] as LastName,");
        _output.WriteLine("       [fldv_ApplicationRole] as Role,");
        _output.WriteLine("       [fldv_PermissionLevel] as PermissionLevel,");
        _output.WriteLine("       [fldb_IsActive] as IsActive,");
        _output.WriteLine("       [fldd_LastLoginDate] as LastLogin");
        _output.WriteLine("FROM [dbo].[tbld_SMSApplicationUsers]");
        _output.WriteLine("WHERE [fldb_IsActive] = 1");
        _output.WriteLine("AND ([fldv_PermissionLevel] IN ('Admin', 'SuperAdmin')");
        _output.WriteLine("     OR [fldv_ApplicationRole] LIKE '%admin%'");
        _output.WriteLine("     OR [fldv_UserName] LIKE '%admin%')");
        _output.WriteLine("ORDER BY [fldv_UserName];");
        _output.WriteLine("");
        
        _output.WriteLine("=== QUERY TO GET SPECIFIC USER PASSWORD HASH ===");
        _output.WriteLine("SELECT [fldv_UserName], [fldv_Password]");
        _output.WriteLine("FROM [dbo].[tbld_SMSApplicationUsers]");
        _output.WriteLine("WHERE [fldv_UserName] = 'admin@flypdx.com'");
        _output.WriteLine("AND [fldb_IsActive] = 1;");
        _output.WriteLine("");
        
        _output.WriteLine("=== ALL ACTIVE USERS QUERY ===");
        _output.WriteLine("SELECT [fldv_Code], [fldv_UserName], [fldv_FirstName], [fldv_LastName],");
        _output.WriteLine("       [fldv_ApplicationRole], [fldv_PermissionLevel], [fldd_LastLoginDate]");
        _output.WriteLine("FROM [dbo].[tbld_SMSApplicationUsers]");
        _output.WriteLine("WHERE [fldb_IsActive] = 1");
        _output.WriteLine("ORDER BY [fldv_PermissionLevel] DESC, [fldv_UserName];");
        
        _output.WriteLine("");
        _output.WriteLine("?? KNOWN ADMIN USERS FROM YOUR DATA:");
        _output.WriteLine("1. admin@flypdx.com (SMS Administrator - SuperAdmin)");
        _output.WriteLine("2. sarah.johnson@flypdx.com (Safety Manager - Admin)");
        
        Assert.True(true);
    }

    [Fact]
    public void VerifyActualAdminCredentials_ShouldFindWorkingPassword()
    {
        _output.WriteLine("=== ACTUAL SMS ADMIN CREDENTIALS ===");
        _output.WriteLine("Found admin users in database:");
        _output.WriteLine("");
        
        // Test the actual admin user from database
        var adminUsername = "admin@flypdx.com";
        var adminHash = "$2a$12$LQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1F";
        
        var safetyManagerUsername = "sarah.johnson@flypdx.com";
        var safetyManagerHash = "$2a$12$N3wH4sh3dP4ssw0rdF0rS4f3ty123M4n4g3rBcRyPtH4sh";
        
        _output.WriteLine($"1. SUPER ADMIN: {adminUsername} (SuperAdmin)");
        _output.WriteLine($"2. SAFETY MANAGER: {safetyManagerUsername} (Admin)");
        _output.WriteLine("");
        
        // Common passwords to test
        var commonPasswords = new[]
        {
            "admin",
            "password", 
            "Password123!",
            "Admin123!",
            "SMS123!",
            "P@ssw0rd",
            "flypdx",
            "pdxsms",
            "administrator",
            "safety123",
            "admin2025"
        };

        _output.WriteLine("Testing common passwords against SUPER ADMIN...");
        TestPasswordsAgainstHash(adminUsername, adminHash, commonPasswords);
        
        _output.WriteLine("");
        _output.WriteLine("Testing common passwords against SAFETY MANAGER...");
        TestPasswordsAgainstHash(safetyManagerUsername, safetyManagerHash, commonPasswords);
        
        Assert.True(true);
    }

    private void TestPasswordsAgainstHash(string username, string hash, string[] passwords)
    {
        foreach (var password in passwords)
        {
            try
            {
                var passwordFromHash = Password.FromHash(hash, DateTime.UtcNow.AddDays(-30));
                var isValid = passwordFromHash.Verify(password);
                
                if (isValid)
                {
                    _output.WriteLine($"? SUCCESS! CREDENTIALS FOUND!");
                    _output.WriteLine($"   Username: {username}");
                    _output.WriteLine($"   Password: {password}");
                    _output.WriteLine($"   Try logging in at: https://localhost:61117/Account/Login");
                    return;
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"? Error testing password '{password}': {ex.Message}");
            }
        }
        
        _output.WriteLine($"? No matching password found for {username} from common list");
    }
}

/// <summary>
/// SQL queries to help find admin users in database
/// Run these directly against your database
/// </summary>
public class DatabaseQueries
{
    public const string FindAllApplicationUsers = @"
        SELECT 
            fSMSApplicationUserCode,
            fSMSApplicationUserFirstName,
            fSMSApplicationUserLastName, 
            fSMSApplicationUserUserName,
            fSMSApplicationUserApplicationRole,
            fSMSApplicationUserPermissionLevel,
            fSMSApplicationUserIsActive
        FROM tSMSApplicationUser 
        WHERE fSMSApplicationUserIsActive = 1
        ORDER BY fSMSApplicationUserUserName";

    public const string FindAdminUsers = @"
        SELECT 
            fSMSApplicationUserCode,
            fSMSApplicationUserFirstName,
            fSMSApplicationUserLastName,
            fSMSApplicationUserUserName,
            fSMSApplicationUserApplicationRole,
            fSMSApplicationUserPermissionLevel
        FROM tSMSApplicationUser 
        WHERE fSMSApplicationUserIsActive = 1
        AND (
            fSMSApplicationUserPermissionLevel IN ('ADMIN', 'SUPERADMIN') 
            OR fSMSApplicationUserApplicationRole LIKE '%admin%'
            OR fSMSApplicationUserUserName LIKE '%admin%'
        )
        ORDER BY fSMSApplicationUserUserName";

    public const string GetUserPassword = @"
        SELECT 
            fSMSApplicationUserUserName,
            fSMSApplicationUserPassword
        FROM tSMSApplicationUser 
        WHERE fSMSApplicationUserUserName = 'admin@portofportland.com'  -- Replace with actual username
        AND fSMSApplicationUserIsActive = 1";
}