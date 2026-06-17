using System.Text.Json;
using System.Text.Json.Serialization;

namespace LightMES.Tests.Common;

public static class TestJsonOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() },
    };
}
