//-----------------------------------------------------------------------
// <copyright file="IUserDetailsFactory.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Factory interface defining object creation and initialization operations with dependency injection integration.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Infrastructure.Configuration;
namespace SMS_Infrastructure.Interfaces;

public interface IUserDetailsFactory
{
    UserDetails GetUserDetails();
}
