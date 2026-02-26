//-----------------------------------------------------------------------
// <copyright file="UserRoleStatistics.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain model representing statistical data and metrics for SMS userrole reporting.
//                  Domain model representing complex data structures
//                  for business reporting and analytics.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Models;

/// <summary>
/// Statistics for user role assignments
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
