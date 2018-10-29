using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IO;

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

        public string ExportNEP6Wallet(string path, string password)
        {
            try
            {
                string path_new = Path.ChangeExtension(path, ".json");
                StreamReader r = new StreamReader(path_new);

                string json = r.ReadToEnd();
                BsonDocument document = BsonDocument.Parse(json);

                BsonElement psw = new BsonElement("password", password);
                document = document.Add(psw);

                var db = conn.GetDatabase("neo");
                var collection = db.GetCollection<BsonDocument>("wallet");

                collection.InsertOne(document);

                return document.ToJson();
            }
            catch (Exception e)
            {
                return "Export Failed!";
            }
        }

        public string ExportUserWallet(string path, string password)
        {
            //not ever used
            return null;
        }
    }
}
