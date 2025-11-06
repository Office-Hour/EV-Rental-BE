using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebAPI.Swagger;

/// <summary>
/// Harmonises enum schemas across the generated document so OpenAPI lists string names instead of numeric values.
/// </summary>
public sealed class EnumDocumentFilter : IDocumentFilter
{
    private static readonly Type[] EnumTypes = DiscoverProjectEnumTypes();

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var enumType in EnumTypes)
        {
            ApplyIfPresent(context, enumType);
            ApplyIfPresent(context, typeof(Nullable<>).MakeGenericType(enumType));
        }
    }

    private static void ApplyIfPresent(DocumentFilterContext context, Type enumType)
    {
        if (!context.SchemaRepository.TryLookupByType(enumType, out var schema))
        {
            return;
        }

        ApplyEnumMetadata(schema, enumType);
    }

    private static void ApplyEnumMetadata(OpenApiSchema schema, Type enumType)
    {
        var enumValues = EnumSchemaMetadata.GetWireValues(enumType);

        if (!EnumSchemaMetadata.MatchesExisting(schema, enumValues))
        {
            schema.Type = "string";
            schema.Format = null;
            schema.Enum = EnumSchemaMetadata.ToOpenApiEnum(enumValues);
        }

        EnumSchemaMetadata.EnsureDescription(schema, enumValues);
    }

    private static Type[] DiscoverProjectEnumTypes()
    {
        var prefixes = new[]
        {
            "Domain",
            "Application",
            "Persistence",
            "WebAPI"
        };

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && HasProjectPrefix(assembly.GetName().Name, prefixes))
            .SelectMany(GetTypesSafe)
            .Where(type => type?.IsEnum == true)
            .Cast<Type>()
            .Distinct()
            .ToArray();
    }

    private static bool HasProjectPrefix(string? assemblyName, IReadOnlyList<string> prefixes)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return false;
        }

        return prefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.Ordinal));
    }

    private static IEnumerable<Type?> GetTypesSafe(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types;
        }
    }
}
