//-----------------------------------------------------------------------
// <copyright file="BaseCommandBundle.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base class for command bundles in the CQRS architecture.
//                  Provides common functionality for command aggregation and processing
//                  across the Application layer.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Application.Common;

/// <summary>
/// Base Command Bundle for CQRS Architecture
/// Provides foundational functionality for command aggregation and processing.
/// Serves as a base class for command bundles that need to group related commands.
/// 
/// Purpose:
/// - Command aggregation and batching
/// - Common command processing infrastructure
/// - Audit and logging support for command bundles
/// - Transaction boundary management for related commands
/// </summary>
public class BaseCommandBundle
{
}
