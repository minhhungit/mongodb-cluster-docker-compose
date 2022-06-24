using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoCluster.Messages
{
    public class MyDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("supplierId")]
        public string SupplierId { get; set; }

        [BsonElement("oemNumber")]
        public string OemNumber { get; set; }

        [BsonElement("blog")]
        public string Blog { get; set; }

        [BsonElement("zipCode")]
        public string ZipCode { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }
    }
}
