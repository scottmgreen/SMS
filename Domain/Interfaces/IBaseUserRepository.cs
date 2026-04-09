//-----------------------------------------------------------------------
// <copyright file="IBaseUserRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository contract defining data access operations for SMS baseuser entities.
//                  Domain service contract defining business operations
//                  and ensuring clean architecture boundaries.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;

namespace SMS_Domain.Interfaces;

/// <summary>
/// Defines the contract for base user repository operations
/// </summary>
public interface IBaseUserRepository<T> where T : BaseUser
{
    /// <summary>
    /// Gets all users of type T
    /// </summary>
    Task<Result<IEnumerable<T>>> GetAllAsync();

    ///// <summary>
    ///// Gets a user by their unique ID (accepts any BaseUserID-derived type)
    ///// </summary>
    //Task<Result<T>> GetByIdAsync(BaseUserID id);

    ///// <summary>
    ///// Gets a user by their unique ID (string overload for convenience)
    ///// </summary>
    //Task<Result<T>> GetByIdAsync(string id);

    /// <summary>
    /// Gets a user by their code
    /// </summary>
    Task<Result<T>> GetByCodeAsync(string code);

    /// <summary>
    /// Gets a user by their username
    /// </summary>
    Task<Result<T>> GetByUserNameAsync(string userName);

    /// <summary>
    /// Gets all active users of type T
    /// </summary>
    Task<Result<IEnumerable<T>>> GetActiveUsersAsync();

    /// <summary>
    /// Adds a new user
    /// </summary>
    Task<Result<T>> AddAsync(T user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task<Result<bool>> UpdateAsync(T user);

    /// <summary>
    /// Updates a user's password (accepts any BaseUserID-derived type)
    /// </summary>
    Task<Result<bool>> UpdatePasswordAsync(BaseUserID userId, string hashedPassword);

    /// <summary>
    /// Records a user login (accepts any BaseUserID-derived type)
    /// </summary>
    Task<Result<bool>> RecordLoginAsync(BaseUserID userId, DateTime loginDate);

    /// <summary>
    /// Soft deletes a user (accepts any BaseUserID-derived type)
    /// </summary>
    Task<Result<bool>> DeleteAsync(BaseUserID userId);

    /// <summary>
    /// Checks if a username already exists
    /// </summary>
    Task<Result<bool>> UserNameExistsAsync(string userName);

    /// <summary>
    /// Gets user statistics
    /// </summary>
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
}

/// <summary>
/// Represents user statistics for reporting
/// </summary>
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int UsersRequiringPasswordChange { get; set; }
    public int StaleUsers { get; set; }
    public DateTime? LastLoginDate { get; set; }
}
