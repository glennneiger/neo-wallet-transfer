using Microsoft.Win32;
using System;
using System.Windows;

namespace transfer_gui
{
    /// <summary>
    /// WalletFile.xaml 的交互逻辑
    /// </summary>
    public partial class Config : Window
    {
        public string walletPath { get; set; }
        public string walletPassword { get; set; }
        public int dbType { get; set; }
        public string connectionString { get; set; }

        public Config()
        {
            InitializeComponent();
        }

        private void btnFileA_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "请选择钱包文件";
            dlg.DefaultExt = ".db3";
            dlg.Filter = "全部文件类型|*.*|(*.db3)|*.db3|(*.json)|*.json";

            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                this.txtWalletPath.Text = dlg.FileName;
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            walletPath = this.txtWalletPath.Text;
            IntPtr p = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.txtbxPassword.SecurePassword);
            walletPassword = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(p);
            dbType = this.cbbDBType.SelectedIndex;

            string host = this.txtHost.Text;
            string userName = this.txtUserName.Text;
            IntPtr dbP = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(this.txtdbPassword.SecurePassword);
            string dbPassword = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(dbP);
            string dbName = this.txtDatabase.Text;

            switch (dbType)
            {
                case (int)DBType.Mongo:
                    if (userName.Length < 1)
                        connectionString = "mongodb://" + host;
                    else
                        connectionString = "mongodb://" + userName + ":" + dbPassword + "@" + host;
                    break;
                case (int)DBType.MySQL:
                    connectionString = "Host=" + host + "; UserName=" + userName + "; Password=" + dbPassword + "; Database=" + dbName + ";";
                    break;
                default:
                    break;
            }

            if (walletPassword == "" || walletPath == "")
            {
                MessageBox.Show("请输入钱包完整信息！");
            }
            else if (host == "")
            {
                MessageBox.Show("请输入数据库地址！");
            }
            else if (dbType == (int)DBType.MySQL && (userName == "" || dbPassword == ""))
            {
                MessageBox.Show("MySQL连接信息不足！");
            }
            else if (dbType == (int)DBType.Mongo && userName == "" && dbPassword != "")
            {
                MessageBox.Show("MongoDB缺少相应的用户名！");
            }
            else
            {
                this.DialogResult = true;
            }
        }
    }
}
