using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("gamblers")]
public class Gambler
{
    [BsonId]
    public required ulong Id { get; set; }
    public required string Name { get; set; }
    public int SingleTicket { get; set; }
    public int DoubleTicket { get; set; }
    public int TripleTicket { get; set; }
    public int Total { get; set; }
}
