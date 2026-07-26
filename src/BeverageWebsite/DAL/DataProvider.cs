using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace BeverageWebsite.DAL
{
    /// <summary>
    /// Provides reusable ADO.NET methods for executing SQL commands against the application's SQL Server database.
    /// </summary>
    public class DataProvider
    {
        private readonly string _connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProvider"/> class.
        /// </summary>
        public DataProvider()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["BeverageWebsiteDbConnection"]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new ConfigurationErrorsException("Connection string 'BeverageWebsiteDbConnection' was not found in the application configuration.");
            }
        }

        /// <summary>
        /// Executes a SQL command that does not return a result set.
        /// </summary>
        /// <param name="commandText">The SQL command text to execute.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="parameters">The SQL parameters to include in the command.</param>
        /// <returns>The number of rows affected.</returns>
        public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute non-query command. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executes a SQL command and returns the first column of the first row.
        /// </summary>
        /// <param name="commandText">The SQL command text to execute.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="parameters">The SQL parameters to include in the command.</param>
        /// <returns>The first column of the first row, or null if no result is returned.</returns>
        public object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandType = commandType;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute scalar command. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executes a SQL command and returns a data reader.
        /// </summary>
        /// <param name="commandText">The SQL command text to execute.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="parameters">The SQL parameters to include in the command.</param>
        /// <returns>A <see cref="SqlDataReader"/> instance.</returns>
        public SqlDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var connection = new SqlConnection(_connectionString);

            try
            {
                var command = new SqlCommand(commandText, connection);
                command.CommandType = commandType;

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                connection.Open();
                return command.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                throw new InvalidOperationException($"Failed to execute reader command. Details: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executes a SQL command and returns the result as a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="commandText">The SQL command text to execute.</param>
        /// <param name="commandType">The type of command to execute.</param>
        /// <param name="parameters">The SQL parameters to include in the command.</param>
        /// <returns>A populated <see cref="DataTable"/>.</returns>
        public DataTable ExecuteDataTable(string commandText, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                using (var command = new SqlCommand(commandText, connection))
                using (var adapter = new SqlDataAdapter(command))
                {
                    command.CommandType = commandType;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    adapter.Fill(dataTable);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute data table command. Details: {ex.Message}", ex);
            }

            return dataTable;
        }
    }
}
