//-----------------------------------------------------------------------
// <copyright file="DataAccess.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data access utility providing database operation helpers, connection management, and query execution support.
//                  Infrastructure utility providing shared functionality
//                  for data access and external system integration.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Infrastructure.Common;

public static class DataAccess
{
    public static IDbDataParameter Parameter(string parmName, object parmValue, Type datatype = null)
    {
        IDbDataParameter parm;
        if (datatype is null)
        {
            parm = new SqlParameter
            {
                ParameterName = parmName,
                Value = parmValue
            };
        }
        else
        {
            parm = new SqlParameter(parmName, SqlDbType.Xml)
            {
                Value = new System.Data.SqlTypes.SqlXml(XmlReader.Create(new StringReader(parmValue.ToString())))
            };
        }

        return parm;
    }
}

