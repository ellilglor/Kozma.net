using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

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
