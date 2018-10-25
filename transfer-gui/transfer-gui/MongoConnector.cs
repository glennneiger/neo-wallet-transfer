using Neo.Implementations.Wallets.EntityFramework;
using Neo.Implementations.Wallets.NEP6;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace transfer_gui
{
    class MongoConnector
    {
        private MongoClient conn;
        public MongoConnector(string connectionString)
        {
            conn = new MongoClient(connectionString);
        }
        private MongoConnector() { }

        public void ExportNEP6Wallet(NEP6Wallet wallet)
        {

        }

        public void ExportUserWallet(UserWallet wallet)
        {

        }
    }
}
