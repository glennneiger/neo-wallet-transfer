using Neo.Implementations.Wallets.EntityFramework;
using Neo.Implementations.Wallets.NEP6;
using Neo.Wallets;
using System;
using System.IO;

namespace transfer_gui
{

    public class Account
    {
        public string Address { get; set; }
        public string Walletpath { get; set; }
        public string Indexerpath { get; set; }
        public string Password { get; set; }

        private Wallet wallet;

        public Wallet Wallet
        {
            get { return wallet; }
            set { wallet = value; }
        }

        public Account()
        {
        }

        public Account(string walletpath, string password, string wif)
        {
            Walletpath = walletpath;
            switch (Path.GetExtension(walletpath))
            {
                case ".db3":
                    {
                        Wallet = UserWallet.Create(walletpath, password);
                        WalletAccount account = Wallet.CreateAccount(Wallet.GetPrivateKeyFromWIF(wif));
                    }
                    break;
                case ".json":
                    {
                        NEP6Wallet wallet = new NEP6Wallet(walletpath);
                        wallet.Unlock(password);
                        WalletAccount account = wallet.CreateAccount(Wallet.GetPrivateKeyFromWIF(wif));
                        wallet.Save();
                        Wallet = wallet;
                    }
                    break;
                default:
                    Console.WriteLine("未知的钱包文件类型");
                    break;
            }

            foreach (WalletAccount account in wallet.GetAccounts())
            {
                Address = account.Address;
            }
            Password = password;
        }

        public void Init(string walletpath, string password, string wif)
        {
            Walletpath = walletpath;
            switch (Path.GetExtension(walletpath))
            {
                case ".db3":
                    {
                        Wallet = UserWallet.Create(walletpath, password);
                        WalletAccount account = Wallet.CreateAccount(Wallet.GetPrivateKeyFromWIF(wif));
                    }
                    break;
                case ".json":
                    {
                        NEP6Wallet wallet = new NEP6Wallet(walletpath);
                        wallet.Unlock(password);
                        WalletAccount account = wallet.CreateAccount(Wallet.GetPrivateKeyFromWIF(wif));
                        wallet.Save();
                        Wallet = wallet;
                    }
                    break;
                default:
                    Console.WriteLine("未知的钱包文件类型");
                    break;
            }
            
            foreach (WalletAccount account in wallet.GetAccounts())
            {
                Address = account.Address;
            }
            Password = password;
        }
    }


}
