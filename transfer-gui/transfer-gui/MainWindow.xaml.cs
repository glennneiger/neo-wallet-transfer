using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace transfer_gui
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

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
                    //accountA = new Account(walletfile.walletPathA, walletfile.walletPasswordA, "KxC7fxvBgNNeiFmcp1gRzN6ZfSFXfxrTC6WDXAFjhWDqrknoZUrv");
                    //accountB = new Account(walletfile.walletPathB, walletfile.walletPasswordB, "KwPRvCPeoe2y2CvqFypAzv5nVKjziQPStHrFndZQAS5MjQbgrC5C");
                }
                catch (CryptographicException)
                {
                    MessageBox.Show("密码错误！");
                    return;
                }
            }
        }


    }
}
