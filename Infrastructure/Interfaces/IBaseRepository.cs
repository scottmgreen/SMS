//-----------------------------------------------------------------------
// <copyright file="IBaseRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base repository class providing common data access functionality with connection management and stored procedure execution.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Interfaces;

public interface IBaseRepository<TEntity, TModel>
where TEntity : class
where TModel : class
{
    public ILogger<TEntity> Logger { get; }
    //public UserDetails UserDetails { get; }
    public IConfiguration Configuration { get; }

    public string ConnectionString { get; }
    public string LogHeader { get; }

    //Task<List<TModel>> GetAllAsync();
    //Task<TModel> GetByCodeAsync(string id);
    //Task<TModel> AddAsync(TModel model);
    //Task<bool> UpdateAsync(TModel model);
    //Task<bool> DeleteAsync(TModel model);
}

