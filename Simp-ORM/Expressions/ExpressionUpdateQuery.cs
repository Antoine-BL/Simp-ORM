using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SimpORM.Sql
{
    public class ExpressionUpdateQuery<T> : IExpressionQuery<T>
    {
        public ExpressionUpdateQuery(Expression<Func<T, bool>> Where, IList<Expression<Func<T, bool>>> SetClauses)
        {
            this.Where = Where;
            this.SetClauses = SetClauses;
        }

        public Expression<Func<T, bool>> Where { get; }
        public IList<Expression<Func<T, bool>>> SetClauses { get; }

        public SqlCommand MakeCommand()
        {
            SqlCommand command = new SqlCommand();
            PropertyInfo[] properties = typeof(T).GetProperties();

            string commandText = $"UPDATE {typeof(T).GetTableMapping()} ";
            if (SetClauses.Count > 0) commandText += "SET ";
            for (var i = 0; i < SetClauses.Count; i++)
            {
                commandText += $"{SetClauses[i].ToWhereClause()}".Replace(")", "").Replace("(", "");

                if (i < SetClauses.Count - 1)
                    commandText += ", ";
            }

            commandText += $" WHERE {Where.ToWhereClause()}";

            command.CommandText = commandText;
            return command;
        }

        public ExecuteType ExecuteType => ExecuteType.NONQUERY;
    }
}
