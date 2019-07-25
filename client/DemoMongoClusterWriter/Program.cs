using MongoCluster.Messages;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace DemoMongoClusterWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient("mongodb://127.0.0.1:27117");

            var database = client.GetDatabase("MyDatabase");
            var collection = database.GetCollection<User>("MyCollection");

            for (int i = 1; i < 1001; i++)
            {
                Console.WriteLine(i);
                collection.InsertOneAsync(new User {
                    Id = new ObjectId { },
                    SupplierId = Guid.NewGuid().ToString(),
                    Age = 30 + i, Name = "Jin Auto " + i,
                    Blog = $"{i} - " + @"The company needed that grimoire because it was going to try to cast a spell in the real world—to transform a popular albeit niche game, 
                                        complicated and nerdy, into a cross-media franchise. That has happened for comic books, for literature, even for toys, heaven help us. 
                                        Lots of people would agree that existing franchises can turn into games. 
                                        But can a famously intricate game turn into a story? That was Kelman’s task. Make it reasonable to produce Magic novels, 
                                        Magic comic books, even—you saw this coming—an animated series on Netflix, produced by the people who wrote and directed the last two Avengers movies, to debut next year. 
                                        And then maybe live action. Movies. Turn the universe of Magic: The Gathering into a story universe." }
                ).GetAwaiter().GetResult();
            }

            Console.WriteLine("Done...");
            Console.ReadKey();
        }
    }
}
