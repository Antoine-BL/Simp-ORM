using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text;

namespace SimpORM.Sql
{
    public class ExpressionDeleteQuery<T> : IExpressionQuery<T>
    {
        public ExpressionDeleteQuery(Expression<Func<T, bool>> where)
        {
            Where = where;
        }

        public SqlCommand MakeCommand()
        {
            SqlCommand command = new SqlCommand();

            command.CommandText = $"DELETE FROM {typeof(T).GetTableMapping()} WHERE {Where.ToWhereClause()}";

            return command;
        }

        public ExecuteType ExecuteType => ExecuteType.NONQUERY;

        public Expression<Func<T, bool>> Where { get; set; }
    }
}
