using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("gamblers")]
public class Gambler
{
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("tag")]
    public required string Name { get; set; }

    [BsonElement("single")]
    public int SingleTicket { get; set; }

    [BsonElement("double")]
    public int DoubleTicket { get; set; }

    [BsonElement("triple")]
    public int TripleTicket { get; set; }

    [BsonElement("total")]
    public int Total { get; set; }
}
