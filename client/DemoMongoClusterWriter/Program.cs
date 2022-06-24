using MongoCluster.Messages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DemoMongoClusterWriter
{
    class Program
    {
        static char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

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
            var dbName = "MyDatabase";
            var client = new MongoClient($"mongodb://127.0.0.1:27117,127.0.0.1:27118/{dbName}");
            //var client = new MongoClient($"mongodb://127.0.0.1:27050/{dbName}");

            var database = client.GetDatabase(dbName);
            var collection = database.GetCollection<MyDocument>("MyCollection");

            var id = 0;
            var nbrRecordsInBatch = 10_000;
            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int times = 0; times < int.MaxValue; times++)
            {
                Stopwatch stopwatchPart = Stopwatch.StartNew();
                List<MyDocument> requests = new List<MyDocument>();

                Console.WriteLine($"{times} - start buiding records...");
                for (int j = 0; j < nbrRecordsInBatch; j++)
                {
                    var zipCode = GetRandomNumber(1, 1000).ToString().PadLeft(4, '0');

                    requests.Add(new MyDocument
                    {
                        Id = new ObjectId { },
                        SupplierId = Guid.NewGuid().ToString(),
                        OemNumber = RandomString(4),
                        ZipCode = zipCode,
                        Name = "Jin Auto " + id,
                        Blog = $"{id} - " + @"The company needed that grimoire because it was going to try to cast a spell in the real world—to transform a popular albeit niche game, 
                                         complicated and nerdy, into a cross-media franchise. That has happened for comic books, for literature, even for toys, heaven help us. 
                                         Lots of people would agree that existing franchises can turn into games. 
                                         But can a famously intricate game turn into a story? That was Kelman’s task. Make it reasonable to produce Magic novels, 
                                         Magic comic books, even—you saw this coming—an animated series on Netflix, produced by the people who wrote and directed the last two Avengers movies, to debut next year. 
                                         And then maybe live action. Movies. Turn the universe of Magic: The Gathering into a story universe."
                    });

                    id++;
                }

                Console.WriteLine($"{times} - start inserting...");

                collection.InsertManyAsync(requests, new InsertManyOptions { IsOrdered = false }).GetAwaiter().GetResult();
                stopwatchPart.Stop();

                Console.WriteLine($"{times} - {stopwatchPart.Elapsed.TotalMilliseconds}ms - {nbrRecordsInBatch} records");
            }

            stopwatch.Stop();
            Console.WriteLine($"total time taken: {stopwatch.Elapsed.TotalSeconds}");

            Console.WriteLine("Done...");
            Console.ReadKey();
        }
    }
}
