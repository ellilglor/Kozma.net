﻿using Kozma.net.Src.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("boxes")]
public class Unbox
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("box")]
    public required string Name { get; set; }

    [BsonElement("amount")]
    public int Count { get; set; }
}

public record UnboxStat(Box Name, int Count, double Percentage);
