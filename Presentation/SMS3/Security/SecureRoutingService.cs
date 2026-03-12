using System.Security.Cryptography;
using System.Text;

namespace SMS3.Security;

/// <summary>
/// Interface for secure URL routing and parameter encryption
/// </summary>
public interface ISecureRoutingService
{
    string EncryptRouteParameter(string value);
    string DecryptRouteParameter(string encryptedValue);
    string EncryptUrl(string url);
    string DecryptUrl(string obfuscatedUrl);
}

/// <summary>
/// Simple URL encryption service for SMS routing
/// Encrypts and decrypts complete URLs for security
/// </summary>
public class SecureRoutingService : ISecureRoutingService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<SecureRoutingService> _logger;
    
    public SecureRoutingService(
        IConfiguration configuration, 
        ILogger<SecureRoutingService> logger)
    {
        _logger = logger;
        
        // Get encryption key from configuration
        var keyString = configuration["Security:RouteEncryptionKey"] 
            ?? throw new InvalidOperationException("Route encryption key not configured");
            
        _key = Convert.FromBase64String(keyString);
        _iv = new byte[16]; // Static IV for route consistency
        
        _logger.LogInformation("SecureRoutingService initialized");
    }
    
    public string EncryptRouteParameter(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
            
        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            
            var encrypted = aes.EncryptEcb(Encoding.UTF8.GetBytes(value), PaddingMode.PKCS7);
            return Convert.ToBase64String(encrypted)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt route parameter: {Value}", value);
            return value; // Fallback to original value
        }
    }
    
    public string DecryptRouteParameter(string encryptedValue)
    {
        if (string.IsNullOrEmpty(encryptedValue))
            return string.Empty;
            
        // Quick check: if it's a simple value (like version numbers), don't try to decrypt
        if (IsSimpleValue(encryptedValue))
            return encryptedValue;
            
        try
        {
            // Restore base64 format
            var base64 = encryptedValue.Replace("_", "/").Replace("-", "+");
            while (base64.Length % 4 != 0) base64 += "=";
            
            var encryptedBytes = Convert.FromBase64String(base64);
            
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            
            var decrypted = aes.DecryptEcb(encryptedBytes, PaddingMode.PKCS7);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decrypt route parameter: {EncryptedValue}", encryptedValue);
            return encryptedValue; // Return original value instead of empty string
        }
    }
    
    private static bool IsSimpleValue(string value)
    {
        // Don't try to decrypt simple values like version numbers, IDs, etc.
        if (value.Length < 8) return true; // Too short to be encrypted
        if (value.All(c => char.IsDigit(c) || c == '.')) return true; // Version numbers like "8.3.8.0"
        if (value.All(char.IsDigit)) return true; // Simple numbers like "1"
        
        // Check if it looks like a proper base64 string
        var base64Pattern = value.Replace("_", "/").Replace("-", "+");
        while (base64Pattern.Length % 4 != 0) base64Pattern += "=";
        
        try
        {
            Convert.FromBase64String(base64Pattern);
            return false; // It's a valid base64 string, might be encrypted
        }
        catch
        {
            return true; // Not valid base64, probably not encrypted
        }
    }
    
    /// <summary>
    /// Encrypt an entire URL path (including parameters)
    /// </summary>
    public string EncryptUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;
            
        try
        {
            // Encrypt the entire URL as one string
            var encryptedUrl = EncryptRouteParameter(url);
            return $"/s/{encryptedUrl}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt URL: {Url}", url);
            return url; // Fallback to original URL
        }
    }
    
    /// <summary>
    /// Decrypt an obfuscated URL back to original
    /// </summary>
    public string DecryptUrl(string obfuscatedUrl)
    {
        if (string.IsNullOrEmpty(obfuscatedUrl))
            return string.Empty;
            
        try
        {
            // Remove /s/ prefix if present
            var encryptedPart = obfuscatedUrl.StartsWith("/s/") 
                ? obfuscatedUrl.Substring(3) 
                : obfuscatedUrl;
                
            // Decrypt the URL
            return DecryptRouteParameter(encryptedPart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt URL: {ObfuscatedUrl}", obfuscatedUrl);
            return string.Empty;
        }
    }
}