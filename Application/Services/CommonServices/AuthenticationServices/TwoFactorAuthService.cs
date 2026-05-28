//-----------------------------------------------------------------------
// <copyright file="TwoFactorAuthService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Two-Factor Authentication service with TOTP and Microsoft Authenticator integration.
//                  Provides secure 2FA implementation with backup codes and recovery options.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace SMS_Application.Services;

/// <summary>
/// Two-Factor Authentication service providing TOTP-based 2FA
/// Compatible with Microsoft Authenticator and other TOTP apps
/// </summary>
public class TwoFactorAuthService
{
    private readonly ILogger<TwoFactorAuthService> _logger;
    private readonly TwoFactorAuthConfiguration _config;
    
    // Base32 encoding characters (RFC 4648)
    private const string Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public TwoFactorAuthService(
        ILogger<TwoFactorAuthService> logger,
        TwoFactorAuthConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Generate a new secret key for TOTP
    /// </summary>
    public string GenerateSecretKey()
    {
        var keyBytes = new byte[20]; // 160-bit key (recommended)
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(keyBytes);
        
        return ToBase32String(keyBytes);
    }

    /// <summary>
    /// Generate QR code URI for Microsoft Authenticator
    /// Format: otpauth://totp/issuer:account?secret=key&issuer=issuer
    /// </summary>
    public string GenerateQrCodeUri(string userEmail, string secretKey)
    {
        var account = Uri.EscapeDataString($"{_config.IssuerName}:{userEmail}");
        var issuer = Uri.EscapeDataString(_config.IssuerName);
        var secret = Uri.EscapeDataString(secretKey);
        
        var uri = $"otpauth://totp/{account}?" +
                  $"secret={secret}&" +
                  $"issuer={issuer}&" +
                  $"algorithm=SHA1&" +
                  $"digits={_config.TotpDigits}&" +
                  $"period={_config.TimeWindowSeconds}";

        _logger.LogApplicationDebug("Generated 2FA QR code URI for user: {UserEmail}",
            ApplicationEventIds.Debug,
            userEmail);
        return uri;
    }

    /// <summary>
    /// Validate TOTP code against secret key
    /// </summary>
    public bool ValidateTotpCode(string secretKey, string userCode)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(userCode))
            return false;

        var cleanedCode = userCode.Replace(" ", "").Replace("-", "");
        
        if (cleanedCode.Length != _config.TotpDigits)
            return false;

        if (!long.TryParse(cleanedCode, out var providedCode))
            return false;

        try
        {
            var keyBytes = FromBase32String(secretKey);
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeWindow = currentTime / _config.TimeWindowSeconds;

            // Check current time window and tolerance windows for clock drift
            for (int i = -_config.TimeTolerance; i <= _config.TimeTolerance; i++)
            {
                var testTimeWindow = timeWindow + i;
                var expectedCode = GenerateTotpCode(keyBytes, testTimeWindow);
                
                if (expectedCode == providedCode)
                {
                    _logger.LogApplicationInformation("TOTP code validated successfully (time window offset: {Offset})",
                        ApplicationEventIds.Information,
                        i);
                    return true;
                }
            }

            _logger.LogApplicationWarning("TOTP code validation failed - invalid code provided", ApplicationEventIds.Warning);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error validating TOTP code", ApplicationEventIds.Error, ex);
            return false;
        }
    }

    /// <summary>
    /// Generate backup codes for account recovery
    /// </summary>
    public List<string> GenerateBackupCodes()
    {
        var backupCodes = new List<string>();
        
        using var rng = RandomNumberGenerator.Create();
        
        for (int i = 0; i < _config.BackupCodeCount; i++)
        {
            var codeBytes = new byte[4];
            rng.GetBytes(codeBytes);
            
            // Generate 8-digit backup codes
            var code = Math.Abs(BitConverter.ToInt32(codeBytes, 0) % 100000000).ToString("D8");
            
            // Format as XXXX-XXXX for better readability
            var formattedCode = $"{code.Substring(0, 4)}-{code.Substring(4, 4)}";
            backupCodes.Add(formattedCode);
        }

        _logger.LogApplicationInformation("Generated {BackupCodeCount} backup codes",
            ApplicationEventIds.Information,
            _config.BackupCodeCount);
        return backupCodes;
    }

    /// <summary>
    /// Validate backup code (one-time use)
    /// </summary>
    public bool ValidateBackupCode(List<string> userBackupCodes, string providedCode, out List<string> remainingCodes)
    {
        remainingCodes = new List<string>(userBackupCodes);
        
        if (string.IsNullOrWhiteSpace(providedCode))
            return false;

        var cleanedCode = providedCode.Replace(" ", "").Replace("-", "").ToUpperInvariant();
        
        // Check if code exists in user's backup codes
        var matchingCode = remainingCodes.FirstOrDefault(code => 
            code.Replace("-", "").ToUpperInvariant() == cleanedCode);

        if (matchingCode != null)
        {
            // Remove used backup code
            remainingCodes.Remove(matchingCode);
            _logger.LogApplicationInformation("Backup code validated and consumed. {RemainingCount} codes remaining",
                ApplicationEventIds.Information,
                remainingCodes.Count);
            return true;
        }

        _logger.LogApplicationWarning("Invalid backup code provided", ApplicationEventIds.Warning);
        return false;
    }

    /// <summary>
    /// Generate current TOTP code (for testing/display purposes)
    /// </summary>
    public string GetCurrentTotpCode(string secretKey)
    {
        if (string.IsNullOrWhiteSpace(secretKey))
            return "000000";
            
        try
        {
            var keyBytes = FromBase32String(secretKey);
            var timeWindow = GetCurrentTimeWindow();
            var code = GenerateTotpCode(keyBytes, timeWindow);
            
            return code.ToString(_config.TotpDigits == 6 ? "D6" : $"D{_config.TotpDigits}");
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error generating current TOTP code for debugging", ApplicationEventIds.Error, ex);
            return "000000";
        }
    }

    /// <summary>
    /// Get time remaining in current TOTP window
    /// </summary>
    public int GetTimeRemainingInWindow()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timeInWindow = currentTime % _config.TimeWindowSeconds;
        return _config.TimeWindowSeconds - (int)timeInWindow;
    }

    #region Private Helper Methods

    /// <summary>
    /// Generate TOTP code for specific time window using HMAC-SHA1
    /// </summary>
    private long GenerateTotpCode(byte[] keyBytes, long timeWindow)
    {
        var timeBytes = BitConverter.GetBytes(timeWindow);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeBytes);
        }

        using var hmac = new HMACSHA1(keyBytes);
        var hash = hmac.ComputeHash(timeBytes);
        
        var offset = hash[hash.Length - 1] & 0xF;
        var code = ((hash[offset] & 0x7F) << 24) |
                   ((hash[offset + 1] & 0xFF) << 16) |
                   ((hash[offset + 2] & 0xFF) << 8) |
                   (hash[offset + 3] & 0xFF);

        var digits = (int)Math.Pow(10, _config.TotpDigits);
        return code % digits;
    }

    /// <summary>
    /// Convert byte array to Base32 string
    /// </summary>
    private string ToBase32String(byte[] bytes)
    {
        if (bytes.Length == 0) return string.Empty;

        var sb = new StringBuilder();
        int buffer = bytes[0];
        int bitsLeft = 8;
        int index = 1;

        while (bitsLeft > 0 || index < bytes.Length)
        {
            if (bitsLeft < 5)
            {
                if (index < bytes.Length)
                {
                    buffer = (buffer << 8) | bytes[index++];
                    bitsLeft += 8;
                }
                else
                {
                    int pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }

            int val = (buffer >> (bitsLeft - 5)) & 0x1F;
            sb.Append(Base32Chars[val]);
            bitsLeft -= 5;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Convert Base32 string to byte array
    /// </summary>
    private byte[] FromBase32String(string base32)
    {
        if (string.IsNullOrEmpty(base32)) return Array.Empty<byte>();

        var cleanInput = base32.ToUpperInvariant().Replace(" ", "").Replace("-", "");
        var bytes = new List<byte>();
        
        int buffer = 0;
        int bitsLeft = 0;

        foreach (char c in cleanInput)
        {
            int val = Base32Chars.IndexOf(c);
            if (val < 0) throw new ArgumentException($"Invalid Base32 character: {c}");

            buffer = (buffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bytes.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// Get the current time window for TOTP (number of time windows since Unix epoch)
    /// </summary>
    private long GetCurrentTimeWindow()
    {
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return currentTime / _config.TimeWindowSeconds;
    }

    #endregion
}
