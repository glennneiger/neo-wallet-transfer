using MySql.Data.MySqlClient;
using Neo.Implementations.Wallets.EntityFramework;
using Neo.Implementations.Wallets.NEP6;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace transfer_gui
{
    class MySQLConnector
    {
        private MySqlConnection conn;

        public MySQLConnector(string connectionString)
        {
            conn = new MySqlConnection(connectionString);

            conn.Open();

            string cmdStr = "";
            MySqlCommand cmd = new MySqlCommand(cmdStr, conn);

            conn.Close();
        }
        private MySQLConnector() { }

        public string ExportNEP6Wallet(string path, string password)
        {
            try
            {
                conn.Open();

                //in the future

                conn.Close();
            }
            catch(MySqlException e)
            {

            }

            return null;
        }

        public string ExportUserWallet(string path, string password)
        {
            try
            {
                conn.Open();

                //not ever used

                conn.Close();
            }
            catch (MySqlException e)
            {

            }

            return null;
        }
    }
}
