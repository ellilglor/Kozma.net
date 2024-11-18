using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Database;

[Collection("exchange")]
public class Exchange
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("rate")]
    public int Rate { get; set; }
}
