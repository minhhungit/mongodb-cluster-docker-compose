using MongoCluster.Messages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace DemoMongoClusterReader
{
    class Program
    {
        static List<string> supplierToTest = new List<string>
        {
            "61a8720f-c0ad-422d-a2fe-5c95b1cfbea5",
            "3b414cdb-c290-40d2-82e2-42907f08b0a1",
            "211f44bd-c81d-4f17-9c13-1319ea845826",
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),
            new Guid().ToString(), new Guid().ToString(),  new Guid().ToString(), new Guid().ToString(),           
        };
        static void Main(string[] args)
        {
            var ran = new Random();

            var dbName = "MyDatabase";
            var client = new MongoClient($"mongodb://127.0.0.1:27117,127.0.0.1:27118/{dbName}");

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<User>("MyCollection", new MongoCollectionSettings {ReadPreference = ReadPreference.Nearest });

            while (true)
            {
                try
                {
                    //double totalDocuments = collection.CountDocumentsAsync(FilterDefinition<User>.Empty).GetAwaiter().GetResult();
                    //Console.WriteLine($"Total Records: {totalDocuments} - {Guid.NewGuid()}");

                    var ranSupplierId = supplierToTest[ran.Next(0, supplierToTest.Count)];
                    var docEntry = collection.FindAsync(x => x.SupplierId == ranSupplierId).GetAwaiter().GetResult().ToList();
                    if (docEntry.Count >= 1)
                    {
                        Console.WriteLine($"\nFound {docEntry.Count} items for supplier {ranSupplierId}");
                    }
                    Console.Write("-");
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

                //System.Threading.Thread.Sleep(500);
            }


            Console.ReadKey();
        }
    }
}
