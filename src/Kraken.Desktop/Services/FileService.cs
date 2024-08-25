using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kraken.Desktop.Services;

public class FileService
{
    private FileService()
    {
        _jsonDeserializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonDeserializerOptions;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private static readonly Lazy<FileService> Lazy = new(() => new FileService());

    public static FileService Instance => Lazy.Value;

    public T? Read<T>(string filePath, string fileName)
    {
        var path = Path.Combine(filePath, fileName);

        if (!File.Exists(path))
            return default;

        string json;
        lock (_lock)
        {
            json = File.ReadAllText(path);
        }

        return JsonSerializer.Deserialize<T>(json, _jsonDeserializerOptions);
    }

    public void Save(string folderPath, string filePath, string fileContent)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        lock (_lock)
        {
            File.WriteAllText(Path.Combine(folderPath, filePath), fileContent, Encoding.UTF8);
        }
    }

    public void Save<T>(string folderPath, string filePath, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonSerializer.Serialize(content, _jsonSerializerOptions);
        lock (_lock)
        {
            File.WriteAllText(Path.Combine(folderPath, filePath), fileContent, Encoding.UTF8);
        }
    }
}
