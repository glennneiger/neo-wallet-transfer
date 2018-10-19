using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            dlg.Filter = "全部文件类型|*.*|(*.db3)|*.db3";

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
            connectionString = this.txtConnectionString.Text;

            if (walletPassword == "" || walletPath == "" || connectionString == "")
            {
                MessageBox.Show("请输入完整信息！");
            }
            else
            {
                this.DialogResult = true;
            }
        }
    }
}
