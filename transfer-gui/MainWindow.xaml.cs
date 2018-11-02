using Neo.Core;
using Neo.Implementations.Wallets.NEP6;
using Neo.Network;
using Neo.Wallets;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
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
        private int start_count = 0;
        private int end_count = 0;

        private MySQLConnector mySQL;
        private MongoConnector mongo;

        private Thread exportThread;

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

            DispatcherTimer timeTicker = new DispatcherTimer();
            timeTicker.Interval = new TimeSpan(0, 0, 1); //in Hour, Minutes, Second.
            timeTicker.Tick += time_Tick;
            timeTicker.Start();
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
            if (dBType < DBType.MySQL || dBType > DBType.Mongo)
            {
                MessageBox.Show("未知的数据库类型");
            }

            UILock();

            exportThread = new Thread(new ThreadStart(delegate
            {
                string result = null;
                try
                {
                    switch (dBType)
                    {
                        case DBType.MySQL:
                            result = ExportMySQL(wallet, path, password, connectionString);
                            break;
                        case DBType.Mongo:
                            result = ExportMongo(wallet, path, password, connectionString);
                            break;
                        default:
                            result = "Export Failed!";
                            break;
                    }
                }
                catch (ThreadAbortException)
                {
                    result = "Export Terminated!";
                }
                finally
                {
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
                        UIUnLock();
                    }));
                }
            }));
            exportThread.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LocalNode.UpnpEnabled = true;
            LocalNode.Start(Settings.Default.NodePort);
            listView.ItemsSource = RecordInfoList;
        }

        private void UILock()
        {
            this.btnStart.IsEnabled = false;
            this.btnConfig.IsEnabled = false;
        }

        private void UIUnLock()
        {
            this.btnStart.IsEnabled = true;
            this.btnConfig.IsEnabled = true;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (exportThread != null)
            {
                exportThread.Abort();
            }
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

        private string ExportMongo(Wallet wallet, string path, string password, string connectionString)
        {
            mongo = new MongoConnector(connectionString);
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

        private string ExportMySQL(Wallet wallet, string path, string password, string connectionString)
        {
            mySQL = new MySQLConnector(connectionString);
            if (wallet is NEP6Wallet)
            {
                return mySQL.ExportNEP6Wallet(wallet as NEP6Wallet, password);
            }
            else
            {
                //never reach here
            }
            return null;
        }

        private void time_Tick(object sender, EventArgs e)
        {
            lbl_height.Content = $"{Blockchain.Default.Height}/{Blockchain.Default.HeaderHeight}";
            lbl_count_node.Content = Program.LocalNode.RemoteNodeCount.ToString();

            if (mySQL != null)
            {
                end_count = mySQL.Count;
                lbl_speed.Content = end_count - start_count;
                start_count = end_count;
            }
        }
    }
}
