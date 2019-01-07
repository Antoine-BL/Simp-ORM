using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace SimpORM.Sql
{
    public class SqlDataContext: ISqlDataContext
    {
        private readonly SqlConnection connection;

        public SqlDataContext(string connectionString)
        {
            connection = CreateConnection(connectionString);
        }

        public IDataReader ExecuteReader(SqlCommand command)
        {
            PrepareCommandForExecute(command);

            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);

            return reader;
        }

        public int ExecuteNonQuery(SqlCommand command)
        {
            try
            {
                PrepareCommandForExecute(command);

                return command.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
            }
        }

        public object ExecuteScalar(SqlCommand command)
        {
            try
            {
                PrepareCommandForExecute(command);

                return command.ExecuteScalar();
            }
            finally
            {
                connection.Close();
            }
        }

        private void PrepareCommandForExecute(SqlCommand command)
        {
            connection.Open();

            command.CommandText = command.CommandText;

            command.Connection = connection;
        }

        private SqlConnection CreateConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            return new SqlConnection(connectionString);
        }
    }

    public interface ISqlDataContext
    {
        IDataReader ExecuteReader(SqlCommand command);
        int ExecuteNonQuery(SqlCommand command);
        object ExecuteScalar(SqlCommand command);
    }
}
