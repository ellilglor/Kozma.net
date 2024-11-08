using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore;
using Kozma.net.Enums;

namespace Kozma.net.Models;

[Collection("boxes")]
public class BoxDb
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("box")]
    public Box Name { get; set; }

    [BsonElement("amount")]
    public int Count { get; set; }
}

public record BoxStats(BoxDb Box, double Percentage);