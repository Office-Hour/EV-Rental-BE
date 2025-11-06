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

        var enumValues = EnumSchemaMetadata.GetWireValues(enumType);
        schema.Type = "string";
        schema.Enum = EnumSchemaMetadata.ToOpenApiEnum(enumValues);
        EnumSchemaMetadata.EnsureDescription(schema, enumValues);
    }
}
