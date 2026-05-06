//-----------------------------------------------------------------------
// <copyright file="PDXSMSApiEndpointsV2.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: API endpoints for PDXSMS API version 2.
//-----------------------------------------------------------------------

using Asp.Versioning;
using Asp.Versioning.Builder;

using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;

using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;

using SMS_Infrastructure.Security;

using SMS3.Api.Models;
using SMS3.Api.Services;
using SMS3.Components;




//app.UseSwaggerUI(options =>
//{
//    options.SwaggerEndpoint("/swagger/v1/swagger.json", "V1");
//    options.SwaggerEndpoint("/swagger/v2/swagger.json", "V2");
//});


//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
//    c.SwaggerDoc("v2", new OpenApiInfo { Title = "My API", Version = "v2" });
//});

//http://localhost:5115/api-docs/index.html?urls.primaryName=V1
//http://localhost:5115/api-docs/index.html?urls.primaryName=V2

//No operations defined in spec!



namespace SMS3.Api.Endpoints
{
    /// <summary>
    /// Extension methods for registering PDXSMS API v2 endpoints
    /// </summary>
    public static class PDXSMSApiEndpointsV2
    {
        /// <summary>
        /// Maps PDXSMS API v2 endpoints
        /// </summary>
        public static WebApplication MapPDXSMSApiEndpointsV2(this WebApplication app)
        {
            var group = app.MapGroup("/api/v2/pdxsms")
                .WithGroupName("v2")
                .WithTags("PDXSMSApiV2");

            // Main report submission endpoint
            group.MapPost("", SubmitPDXSMSReport)
                .AddEndpointFilter<ApiKeyAuthenticationFilter>()
                .WithName("SubmitPDXSMSReportV2")
                .WithSummary("Submit a confidential safety report from external systems (v2)")
                .WithDescription("Allows external systems to submit confidential safety reports (API v2)")
                .Produces<PDXSMSReportApiResponse>(StatusCodes.Status200OK)
                .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
                .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);
                //.WithApiVersionSet(app.GetApiVersionSet("v2"));

            // Get hazard categories
            //group.MapGet("/hazard-categories", GetHazardCategories)
            //    .AddEndpointFilter<ApiKeyAuthenticationFilter>()
            //    .WithName("GetHazardCategoriesV2")
            //    .WithSummary("Get all available hazard categories (v2)")
            //    .WithDescription("Returns all valid hazard category values for API submissions (API v2)")
            //    .Produces<object>(StatusCodes.Status200OK);

            //// Get hazard types
            //group.MapGet("/hazard-types", GetHazardTypes)
            //    .AddEndpointFilter<ApiKeyAuthenticationFilter>()
            //    .WithName("GetHazardTypesV2")
            //    .WithSummary("Get available hazard types (v2)")
            //    .WithDescription("Get hazard types. Use ?category=INCIDENT to filter by category (API v2)")
            //    .Produces<object>(StatusCodes.Status200OK);

            //// Get complete reference data
            //group.MapGet("/reference-data", GetReferenceData)
            //    .AddEndpointFilter<ApiKeyAuthenticationFilter>()
            //    .WithName("GetReferenceDataV2")
            //    .WithSummary("Get complete reference data (v2)")
            //    .WithDescription("Returns all categories and hazard types in hierarchical structure (API v2)")
            //    .Produces<object>(StatusCodes.Status200OK);

            return app;
        }

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
