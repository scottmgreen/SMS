//-----------------------------------------------------------------------
// <copyright file="AuditFieldsPipeline.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: DEPRECATED MEGA-PIPELINE - DO NOT USE
//                  This pipeline violates Single Responsibility Principle.
//                  Use individual pipelines instead: AuditFieldsSetterPipeline, CommandAuditPipeline, etc.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Pipelines;

/// <summary>
/// ? DEPRECATED MEGA-PIPELINE - DO NOT USE ?
/// 
/// This pipeline was refactored into single-responsibility pipelines:
/// - AuditFieldsSetterPipeline: Sets audit fields only
/// - CommandAuditPipeline: Audits command execution only  
/// - QueryAuditPipeline: Audits query access only
/// - LoggingPipeline: Request/response logging only
/// - AuditLogPipeline: Business audit logging only
/// - ValidationPipeline: Input validation only
/// 
/// This class is kept for reference but should not be registered or used.
/// </summary>
[Obsolete("This mega-pipeline violates SRP. Use individual single-responsibility pipelines instead.")]
public class AuditFieldsPipeline<TRequest, TResult> : IBasePipeline<TRequest, TResult>
    where TRequest : IRequest<TResult>
    where TResult : Result
{
    public AuditFieldsPipeline()
    {
        throw new InvalidOperationException(
            "? DEPRECATED MEGA-PIPELINE - This pipeline violates Single Responsibility Principle. " +
            "Use individual pipelines instead: AuditFieldsSetterPipeline, CommandAuditPipeline, QueryAuditPipeline, etc.");
    }

    public Task<TResult> HandleAsync(TRequest request, RequestPipelineDelegate<TResult> next, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "? DEPRECATED MEGA-PIPELINE - This pipeline should not be executed. " +
            "Check ServiceCollectionExtensions.cs to ensure proper single-responsibility pipelines are registered.");
    }
}

