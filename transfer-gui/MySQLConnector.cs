using MySql.Data.MySqlClient;
using System.IO;
using System;
using Neo.Implementations.Wallets.NEP6;
using System.Collections.Generic;
using Neo.IO.Json;
using System.Linq;

namespace transfer_gui
{
    class MySQLConnector
    {
        private MySqlConnection conn;
        private int count = 0;
        public int Count
        {
            get { return count; }
        }

        public MySQLConnector(string connectionString)
        {
            conn = new MySqlConnection(connectionString);

            try
            {
                conn.Open();

                string cmdStr = "CREATE TABLE IF NOT EXISTS neo.scrypt ( uid INT AUTO_INCREMENT, n INT, r INT, p INT, PRIMARY KEY(uid))engine = innodb;";
                MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.contract ( script VARCHAR(1000), deployed TINYINT(1), PRIMARY KEY(script))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.parameter ( name VARCHAR(45), type VARCHAR(45), PRIMARY KEY(name))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.wallet ( uid INT AUTO_INCREMENT, name VARCHAR(45), version VARCHAR(10), scrypt_id INT, password VARCHAR(45), extra VARCHAR(45), PRIMARY KEY(uid)," +
                    " CONSTRAINT `scrypt_id1` FOREIGN KEY (`scrypt_id`) REFERENCES `scrypt` (`uid`))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.account ( address VARCHAR(45), label VARCHAR(45), isDefault TINYINT(1), locked TINYINT(1), account_key VARCHAR(100), contract_script VARCHAR(1000), wallet_id INT, extra VARCHAR(45), PRIMARY KEY(address)," +
                    " CONSTRAINT `wallet_id1` FOREIGN KEY (`wallet_id`) REFERENCES `wallet` (`uid`)," +
                    " CONSTRAINT `contract_script1` FOREIGN KEY (`contract_script`) REFERENCES `contract` (`script`))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                cmdStr = "CREATE TABLE IF NOT EXISTS neo.contract2parameter ( uid INT AUTO_INCREMENT, contract_script VARCHAR(1000), parameter_name VARCHAR(45), PRIMARY KEY(uid)," +
                    " CONSTRAINT `contract_script2` FOREIGN KEY (`contract_script`) REFERENCES `contract` (`script`)," +
                    " CONSTRAINT `parameter_name1` FOREIGN KEY (`parameter_name`) REFERENCES `parameter` (`name`))engine = innodb;";
                cmd = new MySqlCommand(cmdStr, conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        private MySQLConnector() { }

        public string ExportNEP6Wallet(NEP6Wallet wallet, string password)
        {
            int wallet_count = 0;
            int account_count = 0;
            int contract_count = 0;
            int scrypt_count = 0;
            int parameter_count = 0;

            try
            {
                conn.Open();

                JObject jWallet = wallet.ToJson();

                int scrypt_id = 0;
                bool check = true;
                try
                {
                    // try get scrypt uid
                    string cmdStr = "SELECT uid FROM neo.scrypt WHERE n=@n and r=@r and p=@p;";
                    MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                    cmd.Parameters.AddWithValue("@n", ReplaceNull(jWallet["scrypt"]["n"]));
                    cmd.Parameters.AddWithValue("@r", ReplaceNull(jWallet["scrypt"]["r"]));
                    cmd.Parameters.AddWithValue("@p", ReplaceNull(jWallet["scrypt"]["n"]));
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    if (rdr.Read())
                        if (rdr.HasRows)
                        {
                            check = false;
                            scrypt_id = rdr.GetInt32("uid");
                        }
                    rdr.Close();

                    scrypt_count++;
                }
                catch (MySqlException e)
                {
                    Console.WriteLine(e.StackTrace);
                    return "Query Failed!";
                }

                if (check)
                {
                    try
                    {
                        // table scrypt
                        string cmdStr = "INSERT INTO neo.scrypt VALUES(NULL, @n, @r, @p);";
                        MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                        cmd.Parameters.AddWithValue("@n", ReplaceNull(jWallet["scrypt"]["n"]));
                        cmd.Parameters.AddWithValue("@r", ReplaceNull(jWallet["scrypt"]["r"]));
                        cmd.Parameters.AddWithValue("@p", ReplaceNull(jWallet["scrypt"]["n"]));
                        int result = cmd.ExecuteNonQuery();

                        scrypt_id = (int)cmd.LastInsertedId;
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.StackTrace);
                        return "Export Failed!";
                    }
                }

                int wallet_id = 0;
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

                    wallet_id = (int)cmd.LastInsertedId;
                    wallet_count++;
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

                        contract_count++;
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.StackTrace);
                        return "Export Failed!";
                    }

                    try
                    {
                        // table account
                        string cmdStr = "INSERT INTO neo.account VALUES(@address, @label, @isDefault, @lock, @key, @script, @wallet_id, @extra);";
                        MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                        cmd.Parameters.AddWithValue("@address", ReplaceNull(jAccount["address"]));
                        cmd.Parameters.AddWithValue("@label", ReplaceNull(jAccount["label"]));
                        cmd.Parameters.AddWithValue("@isDefault", ReplaceBoolean(jAccount["isDefault"].AsBoolean()));
                        cmd.Parameters.AddWithValue("@lock", ReplaceBoolean(jAccount["lock"].AsBoolean()));
                        cmd.Parameters.AddWithValue("@key", ReplaceNull(jAccount["key"]));
                        cmd.Parameters.AddWithValue("@script", ReplaceNull(jAccount["contract"]["script"]));
                        cmd.Parameters.AddWithValue("@wallet_id", ReplaceNull(wallet_id));
                        cmd.Parameters.AddWithValue("@extra", ReplaceNull(jAccount["extra"]));
                        int result = cmd.ExecuteNonQuery();

                        account_count++;
                        count = account_count;
                    }
                    catch (MySqlException e)
                    {
                        Console.WriteLine(e.StackTrace);
                        return "Export Failed!";
                    }

                    foreach (JObject parameter in account.Contract.ParameterList.Zip((account.Contract as NEP6Contract).ParameterNames, (type, name) =>
                    {
                        JObject parameter = new JObject();
                        parameter["name"] = name;
                        parameter["type"] = type;
                        return parameter;
                    }))
                    {
                        check = true;
                        try
                        {
                            // try get parameter
                            string cmdStr = "SELECT name FROM neo.parameter where name=@name and type=@type;";
                            MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                            cmd.Parameters.AddWithValue("@name", ReplaceNull(parameter["name"].AsString()));
                            cmd.Parameters.AddWithValue("@type", ReplaceNull(parameter["name"].AsString()));
                            
                            MySqlDataReader rdr = cmd.ExecuteReader();

                            if (rdr.Read())
                                if (rdr.HasRows)
                                    check = false;
                            rdr.Close();
                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine(e.StackTrace);
                            return "Query Failed!";
                        }

                        if (check)
                        {
                            try
                            {
                                // table parameter
                                string cmdStr = "INSERT INTO neo.parameter VALUES(@name, @type);";
                                MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                                cmd.Parameters.AddWithValue("@name", ReplaceNull(parameter["name"].AsString()));
                                cmd.Parameters.AddWithValue("@type", ReplaceNull(parameter["type"].AsString()));
                                int result = cmd.ExecuteNonQuery();
                            }
                            catch (MySqlException e)
                            {
                                Console.WriteLine(e.StackTrace);
                                return "Export Failed!";
                            }
                        }

                        try
                        {
                            // table contract2parameter
                            string cmdStr = "INSERT INTO neo.contract2parameter VALUES(NULL, @script, @parameter_name);";
                            MySqlCommand cmd = new MySqlCommand(cmdStr, conn);
                            cmd.Parameters.AddWithValue("@script", ReplaceNull(jAccount["contract"]["script"]));
                            cmd.Parameters.AddWithValue("@parameter_name", ReplaceNull(parameter["name"].AsString()));
                            int result = cmd.ExecuteNonQuery();

                            parameter_count++;
                        }
                        catch (MySqlException e)
                        {
                            Console.WriteLine(e.StackTrace);
                            return "Export Failed!";
                        }
                    }
                }

                conn.Close();

                return $"Export Success! Total: {wallet_count} wallet(s), {scrypt_count} scrypt(s), {account_count} account(s), {contract_count} contract(s), {parameter_count} parameter(s).";
            }
            catch (MySqlException e)
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
