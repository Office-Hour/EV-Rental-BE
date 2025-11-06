using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebAPI.Serialization;

internal static class EnumSerialization
{
    public static JsonNamingPolicy NamingPolicy => JsonNamingPolicy.CamelCase;

    public static JsonStringEnumMemberConverter CreateConverter() =>
        new JsonStringEnumMemberConverter(NamingPolicy, allowIntegerValues: false);

    public static void Configure(JsonSerializerOptions options)
    {
        if (options.Converters.OfType<JsonStringEnumMemberConverter>().Any())
        {
            return;
        }

        options.Converters.Add(CreateConverter());
    }
}
