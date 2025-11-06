using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Swagger;

/// <summary>
/// Ensures enums are represented as strings with named values in generated OpenAPI schemas.
/// </summary>
public sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        ApplyForEnumSchema(schema, context.Type);

        if (schema.Properties is null || schema.Properties.Count == 0)
        {
            return;
        }

        var propertyInfos = context.Type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in schema.Properties)
        {
            if (!propertyInfos.TryGetValue(kvp.Key, out var propertyInfo))
            {
                continue;
            }

            ApplyForEnumSchema(kvp.Value, propertyInfo.PropertyType);
        }
    }

    private static void ApplyForEnumSchema(OpenApiSchema schema, Type declaredType)
    {
        var enumType = Nullable.GetUnderlyingType(declaredType) ?? declaredType;

        if (!enumType.IsEnum)
        {
            return;
        }

        var enumNames = Enum.GetNames(enumType);

        schema.Type = "string";
        schema.Enum = enumNames
            .Select(name => (IOpenApiAny)new OpenApiString(name))
            .ToList();

        AppendEnumDescription(schema, enumNames);
    }

    private static void AppendEnumDescription(OpenApiSchema schema, IReadOnlyList<string> enumNames)
    {
        if (enumNames.Count == 0)
        {
            return;
        }

        var suffix = $" Possible values: {string.Join(", ", enumNames)}.";

        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? suffix.TrimStart()
            : string.Concat(schema.Description.TrimEnd(), suffix);
    }
}
