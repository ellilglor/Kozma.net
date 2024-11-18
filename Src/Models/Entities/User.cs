using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("users")]
public class User
{
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("tag")]
    public required string Name { get; set; }

    [BsonElement("amount")]
    public int Count { get; set; }

    [BsonElement("unboxed")]
    public int Unboxed { get; set; }

    [BsonElement("punched")]
    public int Punched { get; set; }
}
