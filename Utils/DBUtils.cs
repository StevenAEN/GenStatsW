using System.Configuration;
using MySql.Data.MySqlClient;

namespace GenStatsW.Utils
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MySqlConnection"].ConnectionString;
            return new MySqlConnection(connectionString);
        }
    }
}
