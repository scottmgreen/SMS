//-----------------------------------------------------------------------
// <copyright file="SecurityMonitoringService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced security monitoring service for SQL injection and XSS detection
//                  Provides detailed logging, alerting, and reporting capabilities
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using System.Collections.Concurrent;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced security monitoring service for attack detection and response
/// </summary>
public class SecurityMonitoringService
{
    private readonly ILogger<SecurityMonitoringService> _logger;
    private readonly RequestValidationConfiguration _config;
    
    // In-memory tracking for attack patterns (consider Redis for distributed scenarios)
    private readonly ConcurrentDictionary<string, AttackAttemptInfo> _attackAttempts = new();
    
    public SecurityMonitoringService(
        ILogger<SecurityMonitoringService> logger,
        RequestValidationConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    /// <summary>
    /// Record SQL injection attempt with enhanced details
    /// </summary>
    public void RecordSqlInjectionAttempt(string clientIp, string userAgent, string attackPattern, string fieldName, string requestPath)
    {
        var attemptInfo = new AttackAttemptInfo
        {
            AttackType = "SQL_INJECTION",
            ClientIp = clientIp,
            UserAgent = userAgent,
            AttackPattern = attackPattern,
            FieldName = fieldName,
            RequestPath = requestPath,
            Timestamp = DateTime.UtcNow,
            Severity = DetermineAttackSeverity(attackPattern)
        };

        // Track repeated attempts from same IP
        var key = $"SQL_{clientIp}";
        _attackAttempts.AddOrUpdate(key, attemptInfo, (k, existing) => 
        {
            existing.AttemptCount++;
            existing.LastAttempt = DateTime.UtcNow;
            return existing;
        });

        // Log with structured data
        _logger.LogApplicationWarning("SQL INJECTION ATTEMPT DETECTED | IP: {ClientIp} | Pattern: {AttackPattern} | Field: {FieldName} | Path: {RequestPath} | Severity: {Severity}",
            ApplicationEventIds.Warning,
            clientIp, attackPattern, fieldName, requestPath, attemptInfo.Severity);

        // Check for attack escalation
        CheckForAttackEscalation(clientIp, attemptInfo.Severity);
    }

    /// <summary>
    /// Record XSS attempt with enhanced details
    /// </summary>
    public void RecordXssAttempt(string clientIp, string userAgent, string attackPattern, string fieldName, string requestPath)
    {
        var attemptInfo = new AttackAttemptInfo
        {
            AttackType = "XSS",
            ClientIp = clientIp,
            UserAgent = userAgent,
            AttackPattern = attackPattern,
            FieldName = fieldName,
            RequestPath = requestPath,
            Timestamp = DateTime.UtcNow,
            Severity = DetermineXssSeverity(attackPattern)
        };

        var key = $"XSS_{clientIp}";
        _attackAttempts.AddOrUpdate(key, attemptInfo, (k, existing) => 
        {
            existing.AttemptCount++;
            existing.LastAttempt = DateTime.UtcNow;
            return existing;
        });

        _logger.LogApplicationWarning("XSS ATTEMPT DETECTED | IP: {ClientIp} | Pattern: {AttackPattern} | Field: {FieldName} | Path: {RequestPath} | Severity: {Severity}",
            ApplicationEventIds.Warning,
            clientIp, attackPattern, fieldName, requestPath, attemptInfo.Severity);

        CheckForAttackEscalation(clientIp, attemptInfo.Severity);
    }

    /// <summary>
    /// Get attack statistics for monitoring dashboard
    /// </summary>
    public SecurityStats GetSecurityStats()
    {
        var now = DateTime.UtcNow;
        var last24Hours = now.AddHours(-24);
        var lastWeek = now.AddDays(-7);

        var recent24h = _attackAttempts.Values.Where(a => a.LastAttempt >= last24Hours).ToList();
        var recentWeek = _attackAttempts.Values.Where(a => a.LastAttempt >= lastWeek).ToList();

        return new SecurityStats
        {
            SqlInjectionAttempts24h = recent24h.Count(a => a.AttackType == "SQL_INJECTION"),
            XssAttempts24h = recent24h.Count(a => a.AttackType == "XSS"),
            SqlInjectionAttemptsWeek = recentWeek.Count(a => a.AttackType == "SQL_INJECTION"),
            XssAttemptsWeek = recentWeek.Count(a => a.AttackType == "XSS"),
            UniqueAttackersWeek = recentWeek.Select(a => a.ClientIp).Distinct().Count(),
            HighSeverityAttacks24h = recent24h.Count(a => a.Severity == "HIGH"),
            TopAttackPatterns = recent24h
                .GroupBy(a => a.AttackPattern)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    private void CheckForAttackEscalation(string clientIp, string severity)
    {
        var sqlKey = $"SQL_{clientIp}";
        var xssKey = $"XSS_{clientIp}";
        
        var totalAttempts = 
            (_attackAttempts.TryGetValue(sqlKey, out var sqlInfo) ? sqlInfo.AttemptCount : 0) +
            (_attackAttempts.TryGetValue(xssKey, out var xssInfo) ? xssInfo.AttemptCount : 0);

        // Escalate if multiple attempts or high severity
        if (totalAttempts >= 5 || severity == "HIGH")
        {
            _logger.LogApplicationCritical("ATTACK ESCALATION | IP: {ClientIp} | Total Attempts: {TotalAttempts} | Severity: {Severity} | CONSIDER IP BLOCKING",
                ApplicationEventIds.Critical,
                null,
                clientIp, totalAttempts, severity);
        }
    }

    private string DetermineAttackSeverity(string pattern)
    {
        var highRiskPatterns = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "EXEC", "UNION" };
        var mediumRiskPatterns = new[] { "SELECT", "INSERT", "UPDATE" };

        if (highRiskPatterns.Any(p => pattern.ToUpperInvariant().Contains(p)))
            return "HIGH";
        if (mediumRiskPatterns.Any(p => pattern.ToUpperInvariant().Contains(p)))
            return "MEDIUM";
        
        return "LOW";
    }

    private string DetermineXssSeverity(string pattern)
    {
        var highRiskPatterns = new[] { "<script", "javascript:", "onload=", "onerror=" };
        var mediumRiskPatterns = new[] { "<iframe", "<object", "<embed" };

        if (highRiskPatterns.Any(p => pattern.ToLowerInvariant().Contains(p)))
            return "HIGH";
        if (mediumRiskPatterns.Any(p => pattern.ToLowerInvariant().Contains(p)))
            return "MEDIUM";
        
        return "LOW";
    }
}

/// <summary>
/// Attack attempt tracking information
/// </summary>
public class AttackAttemptInfo
{
    public string AttackType { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string AttackPattern { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public DateTime LastAttempt { get; set; }
    public int AttemptCount { get; set; } = 1;
    public string Severity { get; set; } = "LOW";
}

/// <summary>
/// Security statistics for monitoring
/// </summary>
public class SecurityStats
{
    public int SqlInjectionAttempts24h { get; set; }
    public int XssAttempts24h { get; set; }
    public int SqlInjectionAttemptsWeek { get; set; }
    public int XssAttemptsWeek { get; set; }
    public int UniqueAttackersWeek { get; set; }
    public int HighSeverityAttacks24h { get; set; }
    public Dictionary<string, int> TopAttackPatterns { get; set; } = new();
}