using MongoCluster.Messages;
using MongoDB.Driver;
using System;

namespace DemoMongoClusterReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://127.0.0.1:27117");

            var database = client.GetDatabase("MyDatabase");
            var collection = database.GetCollection<User>("MyCollection", new MongoCollectionSettings {ReadPreference = ReadPreference.Nearest });

            while (true)
            {
                try
                {
                    var users = collection.Find(Builders<User>.Filter.Empty).ToListAsync().GetAwaiter().GetResult();
                    Console.WriteLine($"{users.Count} - {Guid.NewGuid()}");
                }catch(Exception ex)
                {
                    Console.WriteLine($"{ Guid.NewGuid()} - { ex.Message}");
                }

                System.Threading.Thread.Sleep(100);
            }


            Console.ReadKey();
        }
    }
}
