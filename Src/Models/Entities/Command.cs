using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("commands")]
public class Command
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("command")]
    public required string Name { get; set; }

    [BsonElement("amount")]
    public int Count { get; set; }

    [BsonElement("game")]
    public bool IsGame { get; set; }
}
