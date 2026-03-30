//-----------------------------------------------------------------------
// <copyright file="ValidationPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Validation pipeline for comprehensive input validation and business rule enforcement.
//                  Implements cross-cutting concerns in the request/response pipeline.
//                  Handles logging, auditing, validation, and other aspects.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace SMS_Application.Messaging.Pipelines;

/// <summary>
/// Validation pipeline for comprehensive input validation and business rule enforcement
/// Validates commands before execution and provides detailed validation feedback
/// </summary>
public class ValidationPipeline<TRequest, TResult> : IPipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    private readonly ILogger<ValidationPipeline<TRequest, TResult>> _logger;

    public ValidationPipeline(ILogger<ValidationPipeline<TRequest, TResult>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellation = default)
    {
        cancellation.ThrowIfCancellationRequested();

        var commandType = request.GetType().Name;
        _logger.LogInformation("? Clean Architecture: Validation pipeline processing {CommandType}", commandType);

        // Perform validation before command execution
        var validationResult = ValidateRequest(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("?? Validation failed for {CommandType}: {ValidationErrors}", 
                commandType, string.Join(", ", validationResult.Errors));

            // Return validation failure result
            var failureResult = CreateValidationFailureResult<TResult>(validationResult.Errors);
            return failureResult;
        }

        _logger.LogInformation("? Clean Architecture: Validation passed for {CommandType}", commandType);

        // Execute the command handler if validation passes
        var result = await next().ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Comprehensive request validation using multiple validation strategies
    /// </summary>
    private ValidationResult ValidateRequest(TRequest request)
    {
        var errors = new List<string>();

        try
        {
            // 1. Data Annotations Validation
            var dataAnnotationErrors = ValidateDataAnnotations(request);
            errors.AddRange(dataAnnotationErrors);

            // 2. Custom Business Rule Validation
            var businessRuleErrors = ValidateBusinessRules(request);
            errors.AddRange(businessRuleErrors);

            // 3. Command-Specific Validation
            var commandSpecificErrors = ValidateCommandSpecific(request);
            errors.AddRange(commandSpecificErrors);

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error during validation of {CommandType}", request.GetType().Name);
            errors.Add($"Validation error: {ex.Message}");
            
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors
            };
        }
    }

    /// <summary>
    /// Validate using Data Annotations attributes
    /// </summary>
    private List<string> ValidateDataAnnotations(TRequest request)
    {
        var errors = new List<string>();
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(request);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
        {
            errors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? "Validation error"));
        }

        return errors;
    }

    /// <summary>
    /// Validate common business rules across all commands
    /// </summary>
    private List<string> ValidateBusinessRules(TRequest request)
    {
        var errors = new List<string>();
        var commandType = request.GetType().Name;

        // Null/empty validation for critical properties
        var properties = request.GetType().GetProperties();
        
        foreach (var prop in properties)
        {
            var value = prop.GetValue(request);
            
            // Check for null required IDs
            if (prop.Name.EndsWith("Id") && value == null)
            {
                errors.Add($"{prop.Name} is required for {commandType}");
            }
            
            // Check for empty required strings
            if (prop.PropertyType == typeof(string) && prop.Name.Contains("Code") && string.IsNullOrWhiteSpace(value as string))
            {
                errors.Add($"{prop.Name} cannot be empty for {commandType}");
            }
        }

        return errors;
    }

    /// <summary>
    /// Validate command-specific business rules
    /// </summary>
    private List<string> ValidateCommandSpecific(TRequest request)
    {
        var errors = new List<string>();
        var commandType = request.GetType().Name;

        // User management command validation
        if (commandType.Contains("User"))
        {
            errors.AddRange(ValidateUserCommands(request));
        }

        // Hazard management command validation
        if (commandType.Contains("Hazard"))
        {
            errors.AddRange(ValidateHazardCommands(request));
        }

        // Risk assessment command validation
        if (commandType.Contains("RiskAssessment"))
        {
            errors.AddRange(ValidateRiskAssessmentCommands(request));
        }

        return errors;
    }

    /// <summary>
    /// Validate user management specific rules
    /// </summary>
    private List<string> ValidateUserCommands(TRequest request)
    {
        var errors = new List<string>();
        
        // Add user-specific validation logic here
        // Example: Username format, password complexity, etc.
        
        return errors;
    }

    /// <summary>
    /// Validate hazard management specific rules
    /// </summary>
    private List<string> ValidateHazardCommands(TRequest request)
    {
        var errors = new List<string>();
        
        // Add hazard-specific validation logic here
        // Example: Hazard category validation, severity checks, etc.
        
        return errors;
    }

    /// <summary>
    /// Validate risk assessment specific rules
    /// </summary>
    private List<string> ValidateRiskAssessmentCommands(TRequest request)
    {
        var errors = new List<string>();
        
        // Add risk assessment-specific validation logic here
        // Example: Score ranges, step completion validation, etc.
        
        return errors;
    }

    /// <summary>
    /// Create a validation failure result
    /// </summary>
    private static TResult CreateValidationFailureResult<T>(List<string> errors) where T : Result
    {
        var errorMessage = $"Validation failed: {string.Join("; ", errors)}";
        var error = new Error("VALIDATION_FAILED", errorMessage);
        
        // Use reflection to create the appropriate Result type
        var resultType = typeof(T);
        
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            // Handle Result<T>
            var genericArg = resultType.GetGenericArguments()[0];
            var method = typeof(Result<>).MakeGenericType(genericArg).GetMethod("Failure", new[] { typeof(Error) });
            return (TResult)method!.Invoke(null, new object[] { error })!;
        }
        else if (resultType == typeof(Result))
        {
            // Handle Result
            return (TResult)(Result)Result.Failure(error);
        }
        
        throw new InvalidOperationException($"Unsupported result type: {resultType}");
    }

    /// <summary>
    /// Validation result container
    /// </summary>
    private class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}