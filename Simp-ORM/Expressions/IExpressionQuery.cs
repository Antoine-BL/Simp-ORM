using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace SimpORM.Sql
{
    public interface IExpressionQuery<T>
    {
        SqlCommand MakeCommand();
        ExecuteType ExecuteType { get; }
    }

    public enum ExecuteType
    {
        READER,
        NONQUERY,
        SCALAR
    }
}
