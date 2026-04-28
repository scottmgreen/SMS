//-------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: REFACTORED pipeline registration for single-responsibility pipelines.
//                  Fixed architecture to eliminate mega-pipeline and provide proper separation of concerns.
// </copyright>
//-------------------------------------------------------------------------------

using SMS_Application.Messaging.Pipelines;
using SMS_Application.Services;
using SMS_Application.Interfaces;

namespace SMS_Application.Configuration;

/// <summary>
/// Extension methods for configuring Application layer services
/// REFACTORED: Now registers single-responsibility pipelines in proper order
/// </summary>
internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the mediator with single-responsibility pipeline support to the service collection
    /// FIXED: Proper pipeline ordering with no overlapping responsibilities
    /// Pipeline Order: Validation → Audit Fields → Logging → Command Audit → Query Audit → Business Audit
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        // Core mediator service
        services.AddScoped<IBaseMediator, Mediator>();

        // Register pipelines in execution order (executed in reverse registration order)
        // Each pipeline has a single, well-defined responsibility
        
        // 6. Business audit logging (last - logs to audit tables)
        services.AddScoped(typeof(IBasePipeline<,>), typeof(AuditLogPipeline<,>));
        
        // 5. Query access auditing (for read operations)
        services.AddScoped(typeof(IBasePipeline<,>), typeof(QueryAuditPipeline<,>));
        
        // 4. Command execution auditing (for write operations)  
        services.AddScoped(typeof(IBasePipeline<,>), typeof(CommandAuditPipeline<,>));
        
        // 3. Request/response logging (for debugging and performance monitoring)
        services.AddScoped(typeof(IBasePipeline<,>), typeof(LoggingPipeline<,>));
        
        // 2. Audit field setting (sets CreatedBy, UpdatedBy, etc.)
        services.AddScoped(typeof(IBasePipeline<,>), typeof(AuditFieldsSetterPipeline<,>));
        
        // 1. Input validation (first - validates before execution)
        services.AddScoped(typeof(IBasePipeline<,>), typeof(ValidationPipeline<,>));

        // Register supporting audit services
        services.AddScoped<IQueryAccessAuditService, QueryAccessAuditService>();
        services.AddScoped<ICommandAccessAuditService, CommandAccessAuditService>();
        
        // Register all command/query handlers from the assembly
        RegisterHandlers(services, assembly);

        return services;
    }

    /// <summary>
    /// Register all request handlers from the assembly using reflection
    /// </summary>
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && 
                       t.GetInterfaces()
                        .Any(i => i.IsGenericType && 
                                 i.GetGenericTypeDefinition() == typeof(IBaseRequestHandler<,>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(IBaseRequestHandler<,>));

            services.AddScoped(interfaceType, handlerType);
        }
    }

    public enum LifeTime
    {
        Singleton,
        Transient,
        Scoped
    }
}

