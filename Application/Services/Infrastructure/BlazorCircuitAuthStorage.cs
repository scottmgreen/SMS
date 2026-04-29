//-----------------------------------------------------------------------
// <copyright file="BlazorCircuitAuthStorage.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Blazor Server Circuit-based authentication storage that persists across requests.
//                  Solves HttpContext.Items limitation where data doesn't persist between requests.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Concurrent;

namespace SMS_Application.Services;

/// <summary>
/// Blazor Server Circuit-based authentication storage
/// Stores authentication data per-circuit instead of per-request
/// This solves the HttpContext availability issues in Blazor Server
/// </summary>
public interface IBlazorCircuitAuthStorage
{
    void StoreAuthData(string circuitId, Dictionary<string, string> authData);
    Dictionary<string, string>? GetAuthData(string circuitId);
    Dictionary<string, string>? GetAuthDataByUserId(string userId);
    void ClearAuthData(string circuitId);
    void ClearAuthDataByUserId(string userId);
    Dictionary<string, Dictionary<string, string>> GetAllAuthData();
    void SetAuthData(string circuitId, Dictionary<string, string> authData);
}

public class BlazorCircuitAuthStorage : IBlazorCircuitAuthStorage
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _circuitAuthData = new();

    public void StoreAuthData(string circuitId, Dictionary<string, string> authData)
    {
        _circuitAuthData.AddOrUpdate(circuitId, authData, (key, oldData) => authData);
    }

    public Dictionary<string, string>? GetAuthData(string circuitId)
    {
        return _circuitAuthData.TryGetValue(circuitId, out var data) ? data : null;
    }

    public Dictionary<string, string>? GetAuthDataByUserId(string userId)
    {
        // Find the most recent authentication data for the given user ID
        var matchingData = _circuitAuthData.Values
            .Where(data => data.TryGetValue("SMS_UserId", out var storedUserId) && storedUserId == userId)
            .OrderByDescending(data => data.TryGetValue("SMS_LoginTime", out var loginTime) ? loginTime : string.Empty)
            .FirstOrDefault();

        return matchingData;
    }

    public void ClearAuthData(string circuitId)
    {
        _circuitAuthData.TryRemove(circuitId, out _);
    }

    public void ClearAuthDataByUserId(string userId)
    {
        var keysToRemove = _circuitAuthData
            .Where(kvp => kvp.Value.TryGetValue("SMS_UserId", out var storedUserId) && storedUserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _circuitAuthData.TryRemove(key, out _);
        }
    }

    /// <summary>
    /// Get all stored authentication data - used for fallback scenarios
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> GetAllAuthData()
    {
        return _circuitAuthData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Enhanced storage method with the same functionality as StoreAuthData
    /// </summary>
    public void SetAuthData(string circuitId, Dictionary<string, string> authData)
    {
        StoreAuthData(circuitId, authData);
    }
}