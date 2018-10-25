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

        public void ExportNEP6Wallet(NEP6Wallet wallet)
        {
            try
            {
                conn.Open();



                conn.Close();
            }
            catch(MySqlException e)
            {

            }
        }

        public void ExportUserWallet(UserWallet wallet)
        {
            try
            {
                conn.Open();



                conn.Close();
            }
            catch (MySqlException e)
            {

            }
        }
    }
}
