using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GenStatsW.Utils
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "vps.stevenbarbe.fr";
            int port = 3306;
            string database = "genstats";
            string username = "genstats";
            string password = "QA9pgCbIH7sIWIYB";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }

    }
}
