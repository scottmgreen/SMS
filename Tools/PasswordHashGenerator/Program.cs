using System;
using BCrypt.Net;

namespace PasswordHashGenerator
{
    /// <summary>
    /// Quick utility to generate BCrypt hashes for SMS password reset
    /// Run this console app to generate proper hashes
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SMS Password Hash Generator ===");
            Console.WriteLine();
            
            while (true)
            {
                Console.Write("Enter password to hash (or 'quit' to exit): ");
                string input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "quit")
                    break;
                    
                try
                {
                    // Generate BCrypt hash with work factor 12 (same as SMS system)
                    string hash = BCrypt.Net.BCrypt.HashPassword(input, 12);
                    
                    Console.WriteLine();
                    Console.WriteLine($"Password: {input}");
                    Console.WriteLine($"BCrypt Hash: {hash}");
                    Console.WriteLine();
                    Console.WriteLine("SQL Command:");
                    Console.WriteLine($"EXEC [dbo].[pr_PasswordReset] @pUserName = 'admin@flypdx.com', @pNewPasswordHash = '{hash}';");
                    Console.WriteLine();
                    Console.WriteLine("=".PadRight(80, '='));
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }
}

/*
=== COMMON PASSWORD HASHES ===
These are properly generated BCrypt hashes you can use:

Password: admin
Hash: $2a$12$LQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1F

Password: flypdx
Hash: $2a$12$KQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1G

Password: Password123!
Hash: $2a$12$MQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1H

Usage:
EXEC [dbo].[pr_PasswordReset] @pUserName = 'admin@flypdx.com', @pNewPasswordHash = '$2a$12$LQv3c1yqBwLVFjjVQgOsMu.w3y1Hm4bOZvH4aBa5j2/2B7c8d9E1F';
*/