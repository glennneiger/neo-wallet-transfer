using MySql.Data.MySqlClient;
using System.IO;
using Newtonsoft.Json.Linq;
using System;

namespace transfer_gui
{
    class MySQLConnector
    {
        private MySqlConnection conn;

        public MySQLConnector(string connectionString)
        {
            conn = new MySqlConnection(connectionString);

            conn.Open();

            string cmdStr = "CREATE TABLE IF NOT EXISTS wallet ( uid int AUTO_INCREMENT, data JSON, PRIMARY KEY(uid))engine = innodb; ";
            MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
            int result = cmd.ExecuteNonQuery();

            conn.Close();
        }
        private MySQLConnector() { }

        public string ExportNEP6Wallet(string path, string password)
        {
            string path_new = Path.ChangeExtension(path, ".json");
            StreamReader r = new StreamReader(path_new);

            string json = r.ReadToEnd();

            try
            {
                conn.Open();                

                JObject jo = JObject.Parse(json);
                jo.Add("password", password);
                string cmdStr = "INSERT INTO wallet VALUES(NULL, \'" + jo + "\');";
                Console.WriteLine(cmdStr);

                MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                int result = cmd.ExecuteNonQuery();

                conn.Close();

                return jo.ToString().Replace("\n", "").Replace("\r", "");
            }
            catch(MySqlException e)
            {
                return "Export Failed!";
            }
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
