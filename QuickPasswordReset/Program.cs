using SMS_Domain.ValueObjects;
using System;

namespace PasswordResetHelper
{
    /// <summary>
    /// Quick console app to generate password hash using SMS Domain
    /// Uses the same Password.Create method that the app uses
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SMS Domain Password Generator ===");
            Console.WriteLine("Uses the SAME Domain Password entity your app uses!");
            Console.WriteLine();

            // Generate hashes for common passwords
            string[] passwords = { "admin", "flypdx", "Password123!", "SMS123!" };

            foreach (string plainPassword in passwords)
            {
                try
                {
                    var passwordResult = Password.Create(plainPassword);
                    if (passwordResult.IsSuccess)
                    {
                        var passwordObj = passwordResult.Value;
                        
                        Console.WriteLine($"Password: {plainPassword}");
                        Console.WriteLine($"Hash: {passwordObj.HashedValue}");
                        Console.WriteLine($"SQL: UPDATE [dbo].[tbld_SMSApplicationUsers] SET [fldv_Password] = '{passwordObj.HashedValue}' WHERE [fldv_UserName] = 'admin@flypdx.com';");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($"? Password '{plainPassword}' failed validation: {passwordResult.Error.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Error with '{plainPassword}': {ex.Message}");
                }
            }

            // Interactive mode
            Console.WriteLine("=== INTERACTIVE MODE ===");
            while (true)
            {
                Console.Write("Enter password to hash (or 'quit'): ");
                string input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "quit")
                    break;

                try
                {
                    var result = Password.Create(input);
                    if (result.IsSuccess)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"? SUCCESS!");
                        Console.WriteLine($"Password: {input}");
                        Console.WriteLine($"Hash: {result.Value.HashedValue}");
                        Console.WriteLine();
                        Console.WriteLine("SQL to reset admin:");
                        Console.WriteLine($"UPDATE [dbo].[tbld_SMSApplicationUsers]");
                        Console.WriteLine($"SET [fldv_Password] = '{result.Value.HashedValue}',");
                        Console.WriteLine($"    [fldv_UpdatedBy] = 'ADMIN-RESET',");
                        Console.WriteLine($"    [fldd_UpdatedDate] = GETUTCDATE(),");
                        Console.WriteLine($"    [fldd_PasswordLastChanged] = GETUTCDATE()");
                        Console.WriteLine($"WHERE [fldv_UserName] = 'admin@flypdx.com';");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine($"? Password validation failed: {result.Error.Message}");
                        Console.WriteLine("Requirements: 8+ chars, upper+lower+digit+special");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"? Error: {ex.Message}");
                }
                Console.WriteLine();
            }
        }
    }
}