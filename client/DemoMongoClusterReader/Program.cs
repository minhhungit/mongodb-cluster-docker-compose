using MongoCluster.Messages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace DemoMongoClusterReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbName = "MyDatabase";
            var client = new MongoClient($"mongodb://127.0.0.1:27117,127.0.0.1:27118/{dbName}");

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<User>("MyCollection", new MongoCollectionSettings {ReadPreference = ReadPreference.Nearest });

            while (true)
            {
                try
                {
                    double totalDocuments = collection.CountAsync(FilterDefinition<User>.Empty).GetAwaiter().GetResult();
                    Console.WriteLine($"Total Records: {totalDocuments} - {Guid.NewGuid()}");
                }
                catch (MongoConnectionException mgConnEx)
                {
                    Console.WriteLine($"{ Guid.NewGuid()} - { mgConnEx.Message}");
                    if (mgConnEx.InnerException != null)
                    {
                        Console.WriteLine($"\tInner exception: {mgConnEx.InnerException.Message}");
                    }
                    Console.WriteLine($"\tConnectionInfo: {mgConnEx.ConnectionId}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ Guid.NewGuid()} - { ex.Message}");
                }

                System.Threading.Thread.Sleep(500);
            }


            Console.ReadKey();
        }
    }
}
