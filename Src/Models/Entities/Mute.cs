using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

public class Mute
{
    [BsonId]
    public required string Id { get; set; }
    public required string Name { get; set; }
    public DateTime ExpiresAt { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime UpdatedAt { get; set; }
}

[Collection("wtb_mutes")]
public class BuyMute : Mute
{

}

[Collection("wts_mutes")]
public class SellMute : Mute
{

}
