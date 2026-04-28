//-----------------------------------------------------------------------
// <copyright file="PDXSMSApiEndpoints.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: External API endpoints for SMS confidential reporting with feature flag support.
//                  Provides clean separation of API concerns from Program.cs with
//                  configurable enable/disable functionality for security.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SMS3.Api.Models;
using SMS3.Api.Services;
using SMS_Infrastructure.Security;
using SMS_Application.Messaging.Commands;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS3.Api.Endpoints
{
    /// <summary>
    /// Extension methods for registering PDXSMS API endpoints with feature flag support
    /// </summary>
    public static class PDXSMSApiEndpoints
    {
        /// <summary>
        /// Maps PDXSMS API endpoints if the feature is enabled
        /// </summary>
        /// <param name="app">The web application</param>
        /// <returns>The web application for method chaining</returns>
        public static WebApplication MapPDXSMSApiEndpoints(this WebApplication app)
        {
            // Check if external APIs are enabled via feature management
            var featureManager = app.Services.GetRequiredService<IFeatureManager>();
            
            // Only register API endpoints if the feature is enabled
            if (featureManager.IsEnabledAsync("ExternalApiEnabled").GetAwaiter().GetResult())
            {
                app.MapPDXSMSReportingEndpoints();
                app.MapPDXSMSReferenceDataEndpoints();
            }

            return app;
        }

        /// <summary>
        /// Maps the main reporting endpoints
        /// </summary>
        private static void MapPDXSMSReportingEndpoints(this WebApplication app)
        {
            // Main report submission endpoint
            app.MapPost("/api/pdxsms", SubmitPDXSMSReport)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("SubmitPDXSMSReport")
                .WithTags("PDXSMSReporting")
                .WithSummary("Submit a confidential safety report from external systems")
                .WithDescription("Allows external systems to submit confidential safety reports")
                .Produces<PDXSMSReportApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);
        }

        /// <summary>
        /// Maps the reference data endpoints
        /// </summary>
        private static void MapPDXSMSReferenceDataEndpoints(this WebApplication app)
        {
            // Get hazard categories
            app.MapGet("/api/pdxsms/hazard-categories", GetHazardCategories)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("GetHazardCategories")
                .WithTags("PDXSMSReporting")
                .WithSummary("Get all available hazard categories")
                .WithDescription("Returns all valid hazard category values for API submissions")
                .Produces<object>(StatusCodes.Status200OK);

            // Get hazard types
            app.MapGet("/api/pdxsms/hazard-types", GetHazardTypes)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("GetHazardTypes")
                .WithTags("PDXSMSReporting")
                .WithSummary("Get available hazard types")
                .WithDescription("Get hazard types. Use ?category=INCIDENT to filter by category")
                .Produces<object>(StatusCodes.Status200OK);

            // Get complete reference data
            app.MapGet("/api/pdxsms/reference-data", GetReferenceData)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("GetReferenceData")
                .WithTags("PDXSMSReporting")
                .WithSummary("Get complete reference data")
                .WithDescription("Returns all categories and hazard types in hierarchical structure")
                .Produces<object>(StatusCodes.Status200OK);
        }

        /// <summary>
        /// Main report submission handler
        /// </summary>
        private static async Task<IResult> SubmitPDXSMSReport(
            [FromBody] PDXSMSReportApiRequest request,
            IBaseMediator mediator,
            ILogger<Program> logger,
            HttpContext httpContext,
            IPDXSMSApiService apiService)
        {
            try
            {
                // Use the dedicated API service for business logic
                var result = await apiService.ProcessReportSubmissionAsync(request, httpContext);
                
                return result.IsSuccess 
                    ? Results.Ok(result.Value)
                    : Results.Problem(
                        detail: result.Error?.Message,
                        title: "Report Processing Failed",
                        statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Validation failed for external report submission");
                return Results.BadRequest(new ApiErrorResponse
                {
                    Error = "Validation failed",
                    Details = new List<string> { ex.Message },
                    RequestId = httpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing external confidential report submission");
                return Results.Problem(
                    detail: "An unexpected error occurred while processing your confidential report.",
                    title: "Internal Server Error",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get all available hazard categories
        /// </summary>
        private static IResult GetHazardCategories()
        {
            var categories = HazardCategory.GetAllValues()
                .Select(hc => new HazardCategoryResponse
                {
                    Value = hc.Value,
                    Name = hc.Name,
                    Description = hc.Description,
                    SortOrder = hc.SortOrder
                })
                .OrderBy(x => x.SortOrder)
                .ToList();

            return Results.Ok(new
            {
                message = "Available hazard categories for confidential reporting",
                categories = categories,
                totalCount = categories.Count
            });
        }

        /// <summary>
        /// Get hazard types (optionally filtered by category)
        /// </summary>
        private static IResult GetHazardTypes(string? category)
        {
            IEnumerable<HazardType> hazardTypes = string.IsNullOrEmpty(category)
                ? HazardType.GetAllValues()
                : HazardType.GetByCategory(category);

            var types = hazardTypes
                .Select(ht => new HazardTypeResponse
                {
                    Value = ht.Value,
                    Name = ht.Name,
                    Description = ht.Description,
                    Category = ht.Category,
                    GuidanceText = ht.GuidanceText,
                    RequiresRegulatoryReporting = ht.RequiresRegulatoryReporting
                })
                .ToList();

            return Results.Ok(new
            {
                message = category is not null
                    ? $"Hazard types for category: {category}"
                    : "All available hazard types",
                category = category,
                hazardTypes = types,
                totalCount = types.Count
            });
        }

        /// <summary>
        /// Get complete reference data in hierarchical format
        /// </summary>
        private static IResult GetReferenceData()
        {
            var categories = HazardCategory.GetAllValues()
                .Select(hc => new
                {
                    value = hc.Value,
                    name = hc.Name,
                    description = hc.Description,
                    sortOrder = hc.SortOrder,
                    hazardTypes = HazardType.GetByCategory(hc.Value)
                        .Select(ht => new HazardTypeResponse
                        {
                            Value = ht.Value,
                            Name = ht.Name,
                            Description = ht.Description,
                            GuidanceText = ht.GuidanceText,
                            RequiresRegulatoryReporting = ht.RequiresRegulatoryReporting
                        })
                        .ToList()
                })
                .OrderBy(x => x.sortOrder)
                .ToList();

            return Results.Ok(new
            {
                message = "Complete reference data for confidential reporting API",
                lastUpdated = DateTime.UtcNow,
                categories = categories,
                totalCategories = categories.Count,
                totalHazardTypes = categories.Sum(c => c.hazardTypes.Count),
                usage = new
                {
                    instruction = "Use the 'value' field for API submissions",
                    example = new
                    {
                        hazardCategory = "INCIDENT",
                        hazardType = "AIRCRAFT_INCIDENT"
                    }
                }
            });
        }
    }
}