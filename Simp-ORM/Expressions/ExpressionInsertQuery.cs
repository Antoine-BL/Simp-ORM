using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using SimpORM.Sql;

namespace SimpORM
{
    public class ExpressionInsertQuery<TEntity>: IExpressionQuery<TEntity>
    {
        public ExpressionInsertQuery(TEntity entity)
        {
            Entity = entity;
        }

        public SqlCommand MakeCommand()
        {
            return GenerateInsert(Entity);
        }

        public TEntity Entity { get; }

        private SqlCommand GenerateInsert<T>(T entity)
        {
            SqlCommand command = new SqlCommand();
            PropertyInfo[] properties = typeof(T).GetProperties();
            string columns = "";
            string values = "";

            for (var i = 0; i < properties.Length; i++)
            {
                PropertyInfo prop = properties[i];
                string mappedColumn = prop.GetColMapping();

                columns += mappedColumn;
                values += $"@{mappedColumn}";

                if (i < properties.Length - 1)
                {
                    columns += ", ";
                    values += ", ";
                }

                SqlParameter param = prop.ToParam(entity);
                if (prop.PropertyType == typeof(DateTime))
                {
                    param.SqlDbType = SqlDbType.DateTime;
                }

                command.Parameters.Add(param);
            }

            command.CommandText =
                $"INSERT INTO {typeof(T).GetTableMapping()} ({columns}) VALUES ({values})";
            return command;
        }

        public ExecuteType ExecuteType => ExecuteType.NONQUERY;
    }
}
