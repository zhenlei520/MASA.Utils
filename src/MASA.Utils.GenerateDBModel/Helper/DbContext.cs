using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MASA.Utils.GenerateDBModel.Helper
{
    public static class DbContext
    {
        private static IDbConnection _dbConnection;

        public static void OpenDbConnection(string connStr)
        {
            if (_dbConnection == null)
            {
                _dbConnection = new MySqlConnection(connStr);
            }

            if (_dbConnection.State == ConnectionState.Closed)
            {
                _dbConnection.Open();
            }
        }

        public static IEnumerable<T> Query<T>( string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _dbConnection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
        }
    }
}
