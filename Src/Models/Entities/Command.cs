using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("commands")]
public class Command
{
    [BsonId]
    public ObjectId Id { get; set; }
    public required string Name { get; set; }
    public int Count { get; set; }
    public bool IsGame { get; set; }
}
