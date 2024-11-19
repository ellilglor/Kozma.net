using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

public class Mute
{
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("tag")]
    public required string Name { get; set; }

    [BsonElement("expires")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
}

[Collection("WTB-mutes")]
public class BuyMute : Mute
{

}

[Collection("WTS-mutes")]
public class SellMute : Mute
{

}
