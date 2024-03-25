using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace My.Extensions.Localization.Json.Internal;

internal static class JsonResourceLoader
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

            resources = document.RootElement.EnumerateObject().ToDictionary(e => e.Name, e => e.Value.ToString());
        }

        return resources;
    }
}