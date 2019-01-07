using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SimpORM.Sql
{
    public class SqlDataAdapter : IDbAdapter
    {
        private readonly ISqlDataContext _dataContext;

        public SqlDataAdapter(ISqlDataContext dataContext)
        {
            this._dataContext = dataContext;
        }

        public IList<T> SelectAllInTable<T>()
        {
            SqlCommand command = GenerateSelectForTable<T>();

            return ExecuteReader<T>(command);
        }

        public IList<T> SelectWhere<T>(Expression<Func<T, bool>> predicate)
        {
            SqlCommand command = GenerateSelectForTable<T>();
            command.CommandText += $" WHERE {predicate.ToWhereClause()}";

            return ExecuteReader<T>(command);
        }

        private SqlCommand GenerateSelectForTable<T>()
        {
            string tableName = typeof(T).GetTableMapping();
            string selectQuery = $"SELECT * FROM {tableName}";

            return new SqlCommand(selectQuery);
        }

        public T InsertRow<T>(T entity)
        {
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                object propVal = prop.InvokeGetOn(entity);
                if (prop.IsIdProp() && (propVal == null || propVal.Equals(prop.PropertyType.GetDefault())))
                {
                    int nextVal = CalcNextColumnValue<T>(prop.GetColMapping());
                    prop.InvokeSetOn(entity, nextVal);
                }
            }

            ExpressionInsertQuery<T> insertQuery = new ExpressionInsertQuery<T>(entity);

            SqlCommand insertCommand = insertQuery.MakeCommand();

            _dataContext.ExecuteNonQuery(insertCommand);

            return entity;
        }

        public int CalcNextColumnValue<T>(string column)
        {
            SqlCommand getIdCommand = new SqlCommand($"SELECT MAX({column}) FROM {typeof(T).GetTableMapping()}");
            int nextId = (int)_dataContext.ExecuteScalar(getIdCommand) + 1;

            return nextId;
        }

        public T UpdateRow<T>(T entity)
        {
            Expression whereExpression = null;
            List<Expression<Func<T, bool>>> setClauses = new List<Expression<Func<T, bool>>>();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.IsIdProp())
                {
                    if (whereExpression == null)
                    {
                        whereExpression = prop.ToPropComparisonExpression<T>(prop.InvokeGetOn(entity));
                    }
                    else
                    {
                        whereExpression = Expression.AndAlso(whereExpression, prop.ToPropComparisonExpression<T>(prop.InvokeGetOn(entity)));
                    }
                }
                else
                {
                    setClauses.Add(EntityUtils.ToPropComparisonPredicate<T>(prop, prop.InvokeGetOn(entity)));
                }
            }

            ParameterExpression inputParam = Expression.Parameter(typeof(T), typeof(T).Name);
            Expression<Func<T, bool>> whereClause = 
                whereExpression == null ?
                    ent => 1 == 1 : Expression.Lambda<Func<T, bool>>(whereExpression, inputParam);

            ExpressionUpdateQuery<T> query = new ExpressionUpdateQuery<T>(whereClause, setClauses);

            SqlCommand updateCommand = query.MakeCommand();

            _dataContext.ExecuteNonQuery(updateCommand);

            return entity;
        }

        public bool DeleteWhere<T>(Expression<Func<T, bool>> where)
        {
            ExpressionDeleteQuery<T> query = new ExpressionDeleteQuery<T>(where);

            SqlCommand deleteCommand = query.MakeCommand();

            return _dataContext.ExecuteNonQuery(deleteCommand) > 0;
        }

        public object ExecuteExpressionQuery<T>(IExpressionQuery<T> query)
        {
            SqlCommand command = query.MakeCommand();

            switch (query.ExecuteType)
            {
                case ExecuteType.NONQUERY:
                    return _dataContext.ExecuteNonQuery(command);
                case ExecuteType.READER:
                    return ExecuteReader<T>(command);
                case ExecuteType.SCALAR:
                    return _dataContext.ExecuteScalar(command);
                default:
                    throw new NotImplementedException("Unsupported ExecuteType");
            }
        }

        private IList<T> ExecuteReader<T>(SqlCommand command)
        {
            List<T> allFromSelect;

            using (IDataReader dataReader = _dataContext.ExecuteReader(command))
            {
                allFromSelect = dataReader.ToList<T>();
            }

            return allFromSelect;
        }
    }
}
