using MongoCluster.Messages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoMongoClusterReader
{
    class Program
    {
        static ConcurrentQueue<string> queue = new();
        private static readonly Random rnd = new Random();
        
        private static int GetRandomNumber(int minIncludeValue, int maxExcludeValue)
        {
            return rnd.Next(minIncludeValue, maxExcludeValue);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCD";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[GetRandomNumber(0, s.Length)]).ToArray());
        }


        static void Main(string[] args)
        {
            var ran = new Random();

            var dbName = "MyDatabase";
            var client = new MongoClient($"mongodb://127.0.0.1:27117,127.0.0.1:27118/{dbName}");

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<MyDocument>("MyCollection", new MongoCollectionSettings { ReadPreference = ReadPreference.SecondaryPreferred }); // Always read from a secondary, read from the primary if no secondary is available (https://severalnines.com/blog/become-mongodb-dba-how-scale-reads)
                       

            Task.Run(() =>
            {
                long counter = 0;
                while (true)
                {
                    if (queue.TryDequeue(out string msg))
                    {
                        counter++;
                        Console.WriteLine($"{counter}. {msg}");
                    }
                    else
                    {
                        Thread.Sleep(10);
                    }
                }
            });

            Console.WriteLine($"Start {DateTime.Now}");
            while (true)
            {
                var maxIterations = 10;

                var parallelGroups = Enumerable.Range(0, maxIterations);

                var oldOem = string.Empty;

                var parallelTasks = parallelGroups.Select(groups =>
                {
                    return Task.Run(async () =>
                    {
                        var oemNumber = string.Empty;

                        while (true)
                        {
                            oemNumber = RandomString(4);
                            if (oldOem != oemNumber)
                            {
                                oldOem = oemNumber;
                                break;
                            }
                        }
                        
                        await FindSomethingAsync(collection, oemNumber);
                    });
                });

                Task.WhenAll(parallelTasks).GetAwaiter().GetResult();
                Console.WriteLine("====================================\n");
                
            }

            Console.WriteLine($"Done {DateTime.Now}");
            Console.ReadKey();
        }

        private static async Task FindSomethingAsync(IMongoCollection<MyDocument> collection, string oemNumber)
        {
            var filterBuilder = Builders<MyDocument>.Filter;
            var filter = filterBuilder.Eq(x => x.OemNumber, oemNumber);

            var xx = GetRandomNumber(0, 3);
            var canFilterZip = xx % 2 == 0;

            string zipCode = string.Empty;

            if (canFilterZip)
            {
                zipCode = GetRandomNumber(1, 1000).ToString().PadLeft(4, '0');
                filter = filterBuilder.Eq(x => x.ZipCode, zipCode);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            var data = await collection.Find(filter).Skip(0).Limit(50).ToListAsync();
            var nbrDocs = data.Count();

            //var nbrDocs = await collection.CountDocumentsAsync(filter);
            stopwatch.Stop();

            _ = Task.Run(() =>
            {
                queue.Enqueue($"Found {nbrDocs}\trecords in {Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2)}ms\t[ OEM {oemNumber} {(string.IsNullOrWhiteSpace(zipCode) ? "]" : $"- Zip {zipCode} ]")}");
            });
        }
    }
}
