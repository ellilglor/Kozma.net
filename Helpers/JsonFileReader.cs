﻿using System.Text.Json;

namespace Kozma.net.Helpers;

public class JsonFileReader : IFileReader
{
    public async Task<T?> ReadAsync<T>(string filePath)
    {
        var projectRoot = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        if (projectRoot == null) return default;

        var fullPath = Path.Combine(projectRoot, filePath);

        using FileStream stream = File.OpenRead(fullPath);
        return await JsonSerializer.DeserializeAsync<T>(stream);
    }
}
