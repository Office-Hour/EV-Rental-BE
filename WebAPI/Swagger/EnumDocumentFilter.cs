using System;
using System.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Swagger;

/// <summary>
/// Harmonises enum schemas across the generated document so OpenAPI lists string names instead of numeric values.
/// </summary>
public sealed class EnumDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var enumTypes = typeof(Domain.Enums.BookingStatus).Assembly
            .GetTypes()
            .Where(t => t.IsEnum)
            .ToArray();

        foreach (var enumType in enumTypes)
        {
            if (context.SchemaRepository.TryLookupByType(enumType, out var schema))
            {
                ApplyEnumMetadata(schema, enumType);
            }

            var nullableEnum = typeof(Nullable<>).MakeGenericType(enumType);
            if (context.SchemaRepository.TryLookupByType(nullableEnum, out var nullableSchema))
            {
                ApplyEnumMetadata(nullableSchema, enumType);
            }
        }
    }

    private static void ApplyEnumMetadata(OpenApiSchema schema, Type enumType)
    {
        var enumNames = Enum.GetNames(enumType);

        schema.Type = "string";
        schema.Format = null;
        schema.Enum = enumNames
            .Select(name => (IOpenApiAny)new OpenApiString(name))
            .ToList();

        var suffix = $" Possible values: {string.Join(", ", enumNames)}.";
        schema.Description = string.IsNullOrWhiteSpace(schema.Description)
            ? suffix.TrimStart()
            : string.Concat(schema.Description!.TrimEnd(), suffix);
    }
}
