using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace APIBased.ServiceA.Entities
{
    public class Person
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement(Order = 0)]
        public ObjectId Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [BsonElement(Order = 1)]
        public string Name { get; set; }
    }
}
