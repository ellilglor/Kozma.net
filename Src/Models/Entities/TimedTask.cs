using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("timed_events")]
public class TimedTask
{
    [BsonId]
    public ObjectId Id { get; set; }
    public required string Name { get; set; }
    public int Executed { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
}
