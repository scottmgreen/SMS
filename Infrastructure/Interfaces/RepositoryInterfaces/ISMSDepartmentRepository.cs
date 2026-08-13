//-----------------------------------------------------------------------
// <copyright file="ISMSDepartmentRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure repository contract for SQL-backed SMS department lookup values.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Enums;
using SMS_Domain.Interfaces;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// SMS Department repository interface.
/// </summary>
public interface ISMSDepartmentRepository
{
    Task<Result<IEnumerable<SMSDepartment>>> GetAllAsync();
}
