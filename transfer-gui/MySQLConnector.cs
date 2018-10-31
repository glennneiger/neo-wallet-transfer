using MySql.Data.MySqlClient;
using System.IO;
using System;
using Neo.Implementations.Wallets.NEP6;
using System.Collections.Generic;
using Neo.IO.Json;

namespace transfer_gui
{
    class MySQLConnector
    {
        private MySqlConnection conn;

        public MySQLConnector(string connectionString)
        {
            conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();

                string cmdStr = "CREATE TABLE IF NOT EXISTS neo.scrypt ( uid INT AUTO_INCREMENT, n INT, r INT, p INT, PRIMARY KEY(uid))engine = innodb;";
                MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.contract ( script VARCHAR(100), deployed TINYINT(1), PRIMARY KEY(script))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.parameter ( name VARCHAR(45), type VARCHAR(45), PRIMARY KEY(name))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.wallet ( uid INT AUTO_INCREMENT, name VARCHAR(45), version VARCHAR(10), scrypt_id INT, password VARCHAR(45), extra VARCHAR(45), PRIMARY KEY(uid), CONSTRAINT `scrypt_id1` FOREIGN KEY (`scrypt_id`) REFERENCES `scrypt` (`uid`))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.account ( address VARCHAR(45), label VARCHAR(45), isDefault TINYINT(1), locked TINYINT(1), account_key VARCHAR(100), contract_script VARCHAR(100), extra VARCHAR(45), PRIMARY KEY(address), CONSTRAINT `contract_script1` FOREIGN KEY (`contract_script`) REFERENCES `contract` (`script`))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.contract2parameter ( uid INT AUTO_INCREMENT, contract_script VARCHAR(100), parameter_name VARCHAR(45), PRIMARY KEY(uid), CONSTRAINT `contract_script2` FOREIGN KEY (`contract_script`) REFERENCES `contract` (`script`), CONSTRAINT `parameter_name1` FOREIGN KEY (`parameter_name`) REFERENCES `parameter` (`name`))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        private MySQLConnector() { }

        public string ExportNEP6Wallet(NEP6Wallet wallet, string path, string password)
        {
            string path_new = Path.ChangeExtension(path, ".json");
            StreamReader r = new StreamReader(path_new);

            string json = r.ReadToEnd();

            try
            {
                conn.Open();

                JObject jWallet = wallet.ToJson();

                try
                {
                    // table scrypt
                    string cmdStr = "INSERT INTO neo.scrypt VALUES(NULL, @n, @r, @p);";
                    MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                    cmd.Parameters.AddWithValue("@n", ReplaceNull(jWallet["scrypt"]["n"]));
                    cmd.Parameters.AddWithValue("@r", ReplaceNull(jWallet["scrypt"]["r"]));
                    cmd.Parameters.AddWithValue("@p", ReplaceNull(jWallet["scrypt"]["n"]));
                    int result = cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.StackTrace);
                    return "Export Failed!";
                }

                int scrypt_id = 0;
                try
                {
                    // get scrypt uid
                    string cmdStr = "SELECT uid FROM neo.scrypt WHERE n=@n and r=@r and p=@p;";
                    MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                    cmd.Parameters.AddWithValue("@n", ReplaceNull(jWallet["scrypt"]["n"]));
                    cmd.Parameters.AddWithValue("@r", ReplaceNull(jWallet["scrypt"]["r"]));
                    cmd.Parameters.AddWithValue("@p", ReplaceNull(jWallet["scrypt"]["n"]));
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        scrypt_id = rdr.GetInt32("uid");
                    rdr.Close();

                    if (scrypt_id == 0)
                    {
                        return "Query Failed!";
                    }
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.StackTrace);
                    return "Query Failed!";
                }

                try
                {
                    // table wallet
                    string cmdStr = "INSERT INTO neo.wallet VALUES(NULL, @name, @version, @scrypt_id, @password, @extra);";
                    MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                    cmd.Parameters.AddWithValue("@name", ReplaceNull(jWallet["name"]));
                    cmd.Parameters.AddWithValue("@version", ReplaceNull(jWallet["version"]));
                    cmd.Parameters.AddWithValue("@scrypt_id", ReplaceNull(scrypt_id));
                    cmd.Parameters.AddWithValue("@password", ReplaceNull(password));
                    cmd.Parameters.AddWithValue("@extra", ReplaceNull(jWallet["extra"]));
                    int result = cmd.ExecuteNonQuery();
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.StackTrace);
                    return "Export Failed!";
                }

                IEnumerable<NEP6Account> accounts = wallet.GetNEP6Accounts();

                foreach (NEP6Account account in accounts)
                {
                    JObject jAccount = account.ToJson();
                    try
                    {
                        // table contract
                        string cmdStr = "INSERT INTO neo.contract VALUES(@script, @deployed);";
                        MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                        cmd.Parameters.AddWithValue("@script", ReplaceNull(jAccount["contract"]["script"]));
                        cmd.Parameters.AddWithValue("@deployed", ReplaceBoolean(jAccount["contract"]["deployed"].AsBoolean()));
                        int result = cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.StackTrace);
                        return "Export Failed!";
                    }

                    try
                    {
                        // table account
                        string cmdStr = "INSERT INTO neo.account VALUES(@address, @label, @isDefault, @lock, @key, @script, @extra);";
                        MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                        cmd.Parameters.AddWithValue("@address", ReplaceNull(jAccount["address"]));
                        cmd.Parameters.AddWithValue("@label", ReplaceNull(jAccount["label"]));
                        cmd.Parameters.AddWithValue("@isDefault", ReplaceBoolean(jAccount["isDefault"].AsBoolean()));
                        cmd.Parameters.AddWithValue("@lock", ReplaceBoolean(jAccount["lock"].AsBoolean()));
                        cmd.Parameters.AddWithValue("@key", ReplaceNull(jAccount["key"]));
                        cmd.Parameters.AddWithValue("@script", ReplaceNull(jAccount["contract"]["script"]));
                        cmd.Parameters.AddWithValue("@extra", ReplaceNull(jAccount["extra"]));
                        int result = cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.StackTrace);
                        return "Export Failed!";
                    }

                    foreach (string name in ((NEP6Contract)account.Contract).ParameterNames)
                    {
                        try
                        {
                            // table parameter
                            string cmdStr = "INSERT INTO neo.parameter VALUES(@name, @type);";
                            MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                            cmd.Parameters.AddWithValue("@name", ReplaceNull(name));
                            cmd.Parameters.AddWithValue("@type", ReplaceNull(name));
                            int result = cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine(e.StackTrace);
                            return "Export Failed!";
                        }

                        try
                        {
                            // table contract2parameter
                            string cmdStr = "INSERT INTO neo.contract2parameter VALUES(NULL, @script, @parameter_name);";
                            MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                            cmd.Parameters.AddWithValue("@script", ReplaceNull(jAccount["contract"]["script"]));
                            cmd.Parameters.AddWithValue("@parameter_name", ReplaceNull(name));
                            int result = cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine(e.StackTrace);
                            return "Export Failed!";
                        }
                    }
                }

                conn.Close();

                return jWallet.ToString().Replace("\n", "").Replace("\r", "").Replace("  ", " ");
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.StackTrace);
                return "Connect Failed!";
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
        public static object ReplaceBoolean(bool boolean)
        {
            if (boolean)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public static object ReplaceNull(object obj)
        {
            if (obj == null)
            {
                return DBNull.Value;
            }
            else
            {
                return obj;
            }
        }
    }
}
