using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Serialization;

internal static class EnumSerialization
{
    public static JsonNamingPolicy NamingPolicy => JsonNamingPolicy.CamelCase;

    public static JsonStringEnumConverter CreateConverter() =>
        new JsonStringEnumConverter(NamingPolicy, allowIntegerValues: false);

    public static void Configure(JsonSerializerOptions options)
    {
        if (options.Converters.OfType<JsonStringEnumConverter>().Any())
        {
            return;
        }

        options.Converters.Add(CreateConverter());
    }
}
