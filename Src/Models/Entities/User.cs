using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("users")]
public class User
{
    [BsonId]
    public required string Id { get; set; }
    public required string Name { get; set; }
    public int Commands { get; set; }
    public int Unboxed { get; set; }
    public int Punched { get; set; }
    public int ShardSwept { get; set; }
}
