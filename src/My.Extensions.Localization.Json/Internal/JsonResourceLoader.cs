using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace My.Extensions.Localization.Json.Internal;

public static class JsonResourceLoader
{
    private static readonly JsonDocumentOptions _jsonDocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };


    public static IDictionary<string, string> Load(string filePath)
    {
        var resources = new Dictionary<string, string>();
        if (File.Exists(filePath))
        {
            using var reader = new StreamReader(filePath);

            using var document = JsonDocument.Parse(reader.BaseStream, _jsonDocumentOptions);

            var rootELement = document.RootElement.Clone();

            JsonElementToDictionary(rootELement, resources);
        }

        return resources;
    }

    private static void JsonElementToDictionary(JsonElement element, Dictionary<string, string> result)
    {
        foreach (var item in element.EnumerateObject())
        {
            JsonElementToObject(item.Value, result, item.Name);
        }
    }

    private static void JsonElementToObject(JsonElement element, Dictionary<string, string> result, string path)
    {
        const char period = '.';
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var item in element.EnumerateObject())
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = item.Name;
                }
                else
                {
                    path += period + item.Name;
                }

                JsonElementToObject(item.Value, result, path);

                if (path.Contains(period))
                {
                    path = path[..path.LastIndexOf(period)];
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            JsonElementToArray(element, result, path);
        }
        else
        {
            JsonElementToValue(element, result, path);
        }
    }

    private static void JsonElementToArray(JsonElement element, Dictionary<string, string> result, string path)
    {
        const char openBracket = '[';
        var index = 0;
        foreach (var item in element.EnumerateArray())
        {
            path += $"[{index}]";

            JsonElementToObject(item, result, path);

            if (path.Contains(openBracket))
            {
                path = path[..path.LastIndexOf(openBracket)];
            }

            ++index;
        }
    }

    private static void JsonElementToValue(JsonElement element, Dictionary<string, string> result, string path)
        => result.Add(path, element.ToString());
}