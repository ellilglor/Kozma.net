using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Models;

[Collection("findlogs")]
public class SearchedLog
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("item")]
    public required string Item { get; set; }

    [BsonElement("amount")]
    public int Count { get; set; }
}
