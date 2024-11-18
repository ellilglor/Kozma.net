using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using Kozma.net.Src.Enums;

namespace Kozma.net.Src.Models.Database;

[Collection("boxes")]
public class Unbox
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("box")]
    public Box Name { get; set; }

    [BsonElement("amount")]
    public int Count { get; set; }
}

public record UnboxStat(Box Name, int Count, double Percentage);
