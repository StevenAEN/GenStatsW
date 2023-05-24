
using MySql.Data.MySqlClient;

namespace GenStatsW.Utils
{
    public class DBMySQLUtils
    {
        
        private static MySqlConnection? connection;

        public static MySqlConnection GetDBConnection(string host, int port, string database, string username, string password)
        {
            if (connection == null)
            {
                string connString = $"Server={host};Database={database};port={port};User Id={username};password={password}";
                connection = new MySqlConnection(connString);
            }

            return connection;
        }
    }

}
