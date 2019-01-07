using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text;

namespace SimpORM.Sql
{
    public class ExpressionReadQuery<TEntity>: IExpressionQuery<TEntity>
    {
        public Expression<Func<TEntity, bool>> WhereExpression { get; set; }
        public IList<OrderByExpression<TEntity, object>> OrderByList { get; } = new List<OrderByExpression<TEntity, object>>();

        public SqlCommand MakeCommand()
        {
            SqlCommand command = new SqlCommand();
            string commandText = $"SELECT * FROM {typeof(TEntity).GetTableMapping()}";

            commandText += MakeWhereClause();
            commandText += MakeOrderByClause();

            return new SqlCommand(commandText);
        }

        public ExpressionReadQuery(bool isScalar = false)
        {
            ExecuteType = isScalar ? ExecuteType.SCALAR : ExecuteType.READER;
        }

        private string MakeWhereClause()
        {
            if (WhereExpression != null)
            {
                return $" WHERE {WhereExpression.ToWhereClause()}";
            }

            return "";
        }

        private string MakeOrderByClause()
        {
            string orderBy = "";

            if (OrderByList.Count > 0)
            {
                orderBy += " ORDER BY ";
                for (int i = 0; i < OrderByList.Count; i++)
                {
                    OrderByExpression<TEntity, object> OrderByExpr = OrderByList[i];
                    orderBy += OrderByExpr.ToOrderBy();
                    if (i < OrderByList.Count - 1) orderBy += ", ";
                }
            }

            return orderBy;
        }

        public ExecuteType ExecuteType { get; }
    }
}
