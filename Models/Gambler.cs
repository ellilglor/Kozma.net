using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Models;

[Collection("gamblers")]
public class Gambler
{
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("tag")]
    public required string Name { get; set; }

    [BsonElement("single")]
    public int Single { get; set; }

    [BsonElement("double")]
    public int Double { get; set; }

    [BsonElement("triple")]
    public int Triple { get; set; }

    [BsonElement("total")]
    public int Total { get; set; }
}

public record GamblerStats(Gambler Gambler, double Percentage);
