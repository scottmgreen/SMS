//-----------------------------------------------------------------------
// <copyright file="PDXSMSApiEndpointsV1.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: External API endpoints for SMS confidential reporting (API v1).
//-----------------------------------------------------------------------


using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using SMS3.Api.Models;
using SMS3.Api.Services;
using SMS_Infrastructure.Security;
using SMS_Application.Commands;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS3.Api.Endpoints
{
    /// <summary>
    /// Extension methods for registering PDXSMS API v1 endpoints
    /// </summary>
    public static class PDXSMSApiEndpointsV1
    {
        /// <summary>
        /// Maps PDXSMS API v1 endpoints
        /// </summary>
        public static WebApplication MapPDXSMSApiEndpointsV1(this WebApplication app, ApiVersionSet versionSet)
        {
            var group = app.MapGroup("/api/v1/pdxsms")
                //.WithGroupName("v1")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(1.0)
                .WithTags("PDXSMSApiV1");

            // Main report submission endpoint
            group.MapPost("", SubmitPDXSMSReport)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("SubmitPDXSMSReportV1")
                .WithSummary("Submit a SMS report from external systems (v1)")
                .WithDescription("Allows external systems to submit SMS reports (API v1)")
                .Produces<PDXSMSReportApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);
                
                //.WithApiVersionSet(app.GetApiVersionSet("v1"));

            // Get hazard categories
            group.MapGet("/hazard-categories", GetHazardCategories)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("GetHazardCategoriesV1")
                .WithSummary("Get all available hazard categories (v1)")
                .WithDescription("Returns all valid hazard category values for API submissions (API v1)")
                .Produces<object>(StatusCodes.Status200OK);

            // Get hazard types
            group.MapGet("/hazard-types", GetHazardTypes)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("GetHazardTypesV1")
                .WithSummary("Get available hazard types (v1)")
                .WithDescription("Get hazard types. Use ?category=INCIDENT to filter by category (API v1)")
                .Produces<object>(StatusCodes.Status200OK);

            // Get complete reference data
            group.MapGet("/reference-data", GetReferenceData)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("GetReferenceDataV1")
                .WithSummary("Get complete reference data (v1)")
                .WithDescription("Returns all categories and hazard types in hierarchical structure (API v1)")
                .Produces<object>(StatusCodes.Status200OK);

            return app;
        }

        /// <summary>
        /// Main report submission handler
        /// </summary>
        private static async Task<IResult> SubmitPDXSMSReport(
            [FromBody] PDXSMSReportApiRequestV1 request,
            IBaseMediator mediator,
            ILogger<Program> logger,
            HttpContext httpContext,
            IPDXSMSApiService apiService)
        {
            try
            {
                // Use the dedicated API service for business logic
                var result = await apiService.ProcessReportSubmissionAsyncV1(request, httpContext);
                
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