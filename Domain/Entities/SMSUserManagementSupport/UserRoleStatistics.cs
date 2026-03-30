//-----------------------------------------------------------------------
// <copyright file="UserRoleStatistics.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: User role statistics and analytics for user management reporting and dashboard visualization.
//                  Provides comprehensive metrics on user roles, permissions, and access patterns.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// User Role Statistics for dashboard and reporting
/// Provides comprehensive metrics on user roles, permissions, and system access
/// </summary>
public class UserRoleStatistics
{
    public int TotalAssignments { get; set; }
    public int ActiveAssignments { get; set; }
    public int ExpiredAssignments { get; set; }
    public int ExpiringAssignments { get; set; }
    public Dictionary<string, int> RoleDistribution { get; set; } = new();
    public Dictionary<string, int> DepartmentDistribution { get; set; } = new();
    public DateTime? LastAssignmentDate { get; set; }
}
