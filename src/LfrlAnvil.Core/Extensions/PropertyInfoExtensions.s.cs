using System.Diagnostics.Contracts;
using System.Reflection;

namespace LfrlAnvil.Extensions;

public static class PropertyInfoExtensions
{
    [Pure]
    public static FieldInfo? GetBackingField(this PropertyInfo source)
    {
        var backingFieldName = $"<{source.Name}>k__BackingField";

        var result = source.DeclaringType?
            .GetField( backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic );

        return result;
    }
}
