using System.Text.Json;

namespace Helpers;

public class JsonHelpers
{
    public static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
    {
        WriteIndented = true,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}