using MySql.Data.MySqlClient;
using System.Data;

namespace MASA.Utils.GenerateDBModel.Helper
{
    public class DapperHelper
    {
        private static string _connectionString = string.Empty;

        private static string ConnectionString
        {
            get { return _connectionString; }
        }
     
        private static IDbConnection dbConnection = null;

        private static DapperHelper uniqueInstance;
       
        private static readonly object locker = new();


        public static DapperHelper GetInstance(string connStr)
        {
            _connectionString = connStr;
            if (uniqueInstance == null)
            {
                lock (locker)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new DapperHelper();
                    }
                }
            }
            return uniqueInstance;
        }

        public static IDbConnection OpenCurrentDbConnection()
        {
            if (dbConnection == null)
            {
                dbConnection = new MySqlConnection(ConnectionString);
            }

            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            return dbConnection;
        }
    }
}
