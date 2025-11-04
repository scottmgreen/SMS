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
