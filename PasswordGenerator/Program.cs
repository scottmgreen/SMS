using System;
using BCrypt.Net;

namespace PasswordHashGenerator
{
    /// <summary>
    /// SMS Password Hash Generator - FIXED VERSION
    /// Generates REAL BCrypt hashes for SMS password reset
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SMS PASSWORD HASH GENERATOR - FIXED ===");
            Console.WriteLine("Generates REAL BCrypt hashes for SMS system");
            Console.WriteLine();
            
            // First, generate common passwords immediately
            GenerateCommonPasswords();
            
            // Interactive mode for custom passwords
            Console.WriteLine("=== INTERACTIVE MODE ===");
            while (true)
            {
                Console.Write("Enter password to hash (or 'quit' to exit): ");
                string input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "quit")
                    break;
                    
                GenerateHashForPassword(input);
            }
            
            Console.WriteLine("Goodbye!");
        }
        
        static void GenerateCommonPasswords()
        {
            Console.WriteLine("=== COMMON PASSWORDS WITH REAL HASHES ===");
            Console.WriteLine();
            
            string[] commonPasswords = { 
                "admin", 
                "password", 
                "Password123!", 
                "flypdx", 
                "TempAdmin123!",
                "SMS123!",
                "pdx2025"
            };
            
            foreach (string password in commonPasswords)
            {
                try
                {
                    string hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
                    Console.WriteLine($"Password: {password}");
                    Console.WriteLine($"Hash: {hash}");
                    Console.WriteLine($"SQL: EXEC [dbo].[pr_PasswordReset] @pUserName = 'admin@flypdx.com', @pNewPasswordHash = '{hash}';");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error hashing '{password}': {ex.Message}");
                }
            }
            
            Console.WriteLine("=".PadRight(100, '='));
            Console.WriteLine();
        }
        
        static void GenerateHashForPassword(string password)
        {
            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(password, 12);
                
                Console.WriteLine();
                Console.WriteLine($"✅ SUCCESS!");
                Console.WriteLine($"Password: {password}");
                Console.WriteLine($"BCrypt Hash: {hash}");
                Console.WriteLine();
                Console.WriteLine("📋 SQL Command to reset admin@flypdx.com:");
                Console.WriteLine($"EXEC [dbo].[pr_PasswordReset] @pUserName = 'admin@flypdx.com', @pNewPasswordHash = '{hash}';");
                Console.WriteLine();
                Console.WriteLine("🧪 Test the hash:");
                
                // Test the hash immediately
                bool isValid = BCrypt.Net.BCrypt.Verify(password, hash);
                Console.WriteLine($"Hash verification: {(isValid ? "✅ VALID" : "❌ INVALID")}");
                Console.WriteLine();
                Console.WriteLine("=".PadRight(80, '='));
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating hash: {ex.Message}");
                Console.WriteLine();
            }
        }
    }
}

/*
=== WHAT IS BCRYPT? ===

BCrypt is a secure password hashing algorithm that:

1. 🧂 SALT: Adds random data to prevent rainbow table attacks
2. 🔄 WORK FACTOR: Uses configurable "rounds" to make hashing slow (cost factor 12 = 2^12 iterations)  
3. 🔒 ONE-WAY: Cannot be "decrypted" - only verified by hashing input and comparing
4. 🛡️ ADAPTIVE: Can increase work factor over time as computers get faster

Example BCrypt hash: $2a$12$randomsalt22charactersxxxcryptedpassword30characters
                     |  |  |                    |
                     |  |  Salt (22 chars)     Hash (31 chars)
                     |  Work factor (12)
                     Algorithm version (2a)

WHY THE "INVALID SALT" ERROR?
- The stored procedure was using FAKE example hashes
- BCrypt requires properly formatted hashes with valid salts
- This tool generates REAL hashes that will work

SECURITY BEST PRACTICES:
- Work factor 12 is good for 2024 (recommended 10-12)
- Never store plain text passwords
- Always use BCrypt.Verify() to check passwords
- Generated hashes are unique even for same password (due to random salt)
*/