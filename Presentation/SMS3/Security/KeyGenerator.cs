using System.Security.Cryptography;

namespace SMS3.Security;

/// <summary>
/// Utility class to generate secure encryption keys for route encryption
/// Run this once to generate your production key
/// </summary>
public static class KeyGenerator
{
    /// <summary>
    /// Generate a secure 256-bit AES key for route encryption
    /// </summary>
    public static string GenerateSecureKey()
    {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.GenerateKey();
        
        return Convert.ToBase64String(aes.Key);
    }
    
        
}

