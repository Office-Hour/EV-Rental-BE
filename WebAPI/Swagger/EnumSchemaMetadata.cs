using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using WebAPI.Serialization;

namespace WebAPI.Swagger;

internal static class EnumSchemaMetadata
{
    public static IReadOnlyList<string> GetWireValues(Type enumType)
    {
        var fields = enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static);

        return fields
            .Select(GetWireName)
            .ToArray();
    }

    public static IList<IOpenApiAny> ToOpenApiEnum(IReadOnlyList<string> values) =>
        values
            .Select(value => (IOpenApiAny)new OpenApiString(value))
            .ToList();

    public static bool MatchesExisting(OpenApiSchema schema, IReadOnlyList<string> values)
    {
        if (!string.Equals(schema.Type, "string", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (schema.Enum is null || schema.Enum.Count != values.Count)
        {
            return false;
        }

        for (var i = 0; i < values.Count; i++)
        {
            if (schema.Enum[i] is not OpenApiString openApiString ||
                !string.Equals(openApiString.Value, values[i], StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    public static void EnsureDescription(OpenApiSchema schema, IReadOnlyList<string> values)
    {
        if (values.Count == 0)
        {
            return;
        }

        var suffix = $" Possible values: {string.Join(", ", values)}.";
        if (schema.Description?.Contains(suffix, StringComparison.Ordinal) == true)
        {
            return;
        }

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? suffix.TrimStart()
            : string.Concat(schema.Description.TrimEnd(), suffix);
    }

    private static string GetWireName(FieldInfo fieldInfo)
    {
        var enumMember = fieldInfo.GetCustomAttribute<EnumMemberAttribute>();
        if (!string.IsNullOrWhiteSpace(enumMember?.Value))
        {
            return enumMember!.Value!;
        }

        return EnumSerialization.NamingPolicy?.ConvertName(fieldInfo.Name) ?? fieldInfo.Name;
    }
}
