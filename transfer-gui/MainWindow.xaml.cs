using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Implementations.Wallets.NEP6;
using Neo.Network;
using Neo.Wallets;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using transfer_gui.Properties;

namespace transfer_gui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public static LocalNode LocalNode = new LocalNode();

        private Wallet wallet;
        private string connectionString;
        private string path;
        private string password;
        private DBType dBType;

        ObservableCollection<RecordInfo> recordInfoList = new ObservableCollection<RecordInfo>();
        internal ObservableCollection<RecordInfo> RecordInfoList
        {
            get { return recordInfoList; }
            set { recordInfoList = value; }
        }
        public MainWindow()
        {
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
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

            string result = null;
            switch (dBType)
            {
                case DBType.MySQL:
                    result = ExportMySQL(wallet, path, password, connectionString);
                    break;
                case DBType.Mongo:
                    result = ExportMongo(wallet, path, password, connectionString);
                    break;
                default:
                    MessageBox.Show("未知的数据库类型");
                    break;
            }

            this.Dispatcher.Invoke(DispatcherPriority.Send, new Action(() =>
            {
                this.RecordInfoList.Add(new RecordInfo(result));
                this.listView.ScrollIntoView(this.listView.Items[this.listView.Items.Count - 1]);
                GridView gv = listView.View as GridView;
                if (gv != null)
                {
                    foreach (GridViewColumn gvc in gv.Columns)
                    {
                        gvc.Width = gvc.ActualWidth;
                        gvc.Width = Double.NaN;
                    }
                }
            }));
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LocalNode.UpnpEnabled = true;
            LocalNode.Start(Settings.Default.NodePort);
            listView.ItemsSource = RecordInfoList;
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
                path = config.walletPath;
                password = config.walletPassword;
                connectionString = config.connectionString;
            }
        }

        private static Wallet OpenWallet(string path, string password)
        {
            if (Path.GetExtension(path) == ".db3")
            {
                string path_new = Path.ChangeExtension(path, ".json");
                NEP6Wallet nep6wallet = NEP6Wallet.Migrate(path_new, path, password);
                nep6wallet.Save();
                return nep6wallet;
            }
            else //.json
            {
                NEP6Wallet nep6wallet = new NEP6Wallet(path);
                nep6wallet.Unlock(password);
                return nep6wallet;
            }
        }

        private static string ExportMongo(Wallet wallet, string path, string password, string connectionString)
        {
            MongoConnector mongo = new MongoConnector(connectionString);
            if (wallet is NEP6Wallet)
            {
                return mongo.ExportNEP6Wallet(path, password);
            }
            else
            {
                //never reach here
            }

            return null;
        }

        private static string ExportMySQL(Wallet wallet, string path, string password, string connectionString)
        {
            MySQLConnector mySQL = new MySQLConnector(connectionString);
            if (wallet is NEP6Wallet)
            {
                return mySQL.ExportNEP6Wallet(wallet as NEP6Wallet, path, password);
            }
            else
            {
                //never reach here
            }
            return null;
        }
    }
}
