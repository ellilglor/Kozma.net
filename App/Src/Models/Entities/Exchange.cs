﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.EntityFrameworkCore;

namespace Kozma.net.Src.Models.Entities;

[Collection("exchange")]
public class Exchange
{
    [BsonId]
    public ObjectId Id { get; set; }
    public int Rate { get; set; }
}
