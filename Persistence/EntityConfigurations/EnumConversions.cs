using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

internal static class EnumConversions
{
    public static PropertyBuilder<TEnum> AsStringEnum<TEnum>(this PropertyBuilder<TEnum> prop)
        where TEnum : struct, Enum
        => prop.HasConversion<string>().HasMaxLength(32);
}
