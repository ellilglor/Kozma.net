﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("tradelogs")]
public class TradeLog
{
    [BsonId]
    public required string Id { get; set; }

    [BsonElement("channel")]
    public required string Channel { get; set; }

    [BsonElement("author")]
    public required string Author { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("messageUrl")]
    public required string MessageUrl { get; set; }

    [BsonElement("content")]
    public required string Content { get; set; }

    [BsonElement("original")]
    public required string OriginalContent { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }
}

public class LogGroups
{
    public required string Channel { get; set; }
    public required IReadOnlyCollection<TradeLog> Messages { get; set; }
}
