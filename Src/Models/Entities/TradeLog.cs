using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("tradelogs")]
public class TradeLog
{
    [BsonId]
    public required string Id { get; set; }
    public required string Channel { get; set; }
    public required string Author { get; set; }
    public DateTime Date { get; set; }
    public required string MessageUrl { get; set; }
    public required string Content { get; set; }
    public required string OriginalContent { get; set; }
    public string? Image { get; set; }
}

public class LogGroups
{
    public required string Channel { get; set; }
    public required IReadOnlyCollection<TradeLog> Messages { get; set; }
}
