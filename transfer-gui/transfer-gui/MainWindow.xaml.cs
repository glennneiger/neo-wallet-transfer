using Neo.Implementations.Wallets.EntityFramework;
using Neo.Implementations.Wallets.NEP6;
using Neo.Wallets;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace transfer_gui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Wallet wallet;
        private String connectionString;
        private DBType dBType;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (dBType < 0)
            {
                MessageBox.Show("数据库类型读取错误！");
                return;
            }
            if (wallet == null)
            {
                MessageBox.Show("钱包数据读取错误！");
                return;
            }
            if (connectionString == null)
            {
                MessageBox.Show("数据库连接字符串错误！");
                return;
            }

            switch (dBType)
            {
                case DBType.MySQL:
                    ExportMySQL(wallet, connectionString);
                    break;
                case DBType.Mongo:
                    ExportMongo(wallet, connectionString);
                    break;
                default:
                    MessageBox.Show("未知的数据库类型");
                    break;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            Config config = new Config();
            if (config.ShowDialog() == true)
            {
                try
                {
                    wallet = OpenWallet(config.walletPath, config.walletPassword);
                }
                catch (CryptographicException)
                {
                    MessageBox.Show("密码错误！");
                    return;
                }

                dBType = (DBType)Enum.ToObject(typeof(DBType), config.dbType);
                connectionString = config.connectionString;
            }
        }

        private static Wallet OpenWallet(string path, string password)
        {
            if (Path.GetExtension(path) == ".db3")
            {
                return UserWallet.Open(path, password);
            }
            else //.json
            {
                NEP6Wallet nep6wallet = new NEP6Wallet(path);
                nep6wallet.Unlock(password);
                return nep6wallet;
            }
        }

        private static void ExportMongo(Wallet wallet, string connectionString)
        {
            MongoConnector mongo = new MongoConnector(connectionString);
            if (wallet is NEP6Wallet)
            {
                mongo.ExportNEP6Wallet(wallet as NEP6Wallet);
            }
            else
            {
                mongo.ExportUserWallet(wallet as UserWallet);
            }
        }

        private static void ExportMySQL(Wallet wallet, string connectionString)
        {
            MySQLConnector mySQL = new MySQLConnector(connectionString);
            if (wallet is NEP6Wallet)
            {
                mySQL.ExportNEP6Wallet(wallet as NEP6Wallet);
            }
            else
            {
                mySQL.ExportUserWallet(wallet as UserWallet);
            }
        }
    }
}
