using Neo.Core;
using Neo.Implementations.Blockchains.LevelDB;
using Neo.Network;
using System;
using transfer_gui.Properties;
using Neo.Wallets;

namespace transfer_gui
{
    static class Program
    {
        public static LocalNode LocalNode;
        public static Wallet CurrentWallet;
        public static MainWindow MainWindow;

        ///<summary>
        /// 应用程序的主入口点。
        ///</summary>
        [STAThread]
        static void Main(string[] args)
        {
            using (Blockchain.RegisterBlockchain(new LevelDBBlockchain(Settings.Default.DataDirectoryPath)))
            using (LocalNode = new LocalNode())
            {
                LocalNode.UpnpEnabled = true;
                App app = new App();
                app.InitializeComponent();
                app.MainWindow = MainWindow ;
                app.Run();
            }
        }
    }
}
