using System.Reflection;

namespace LfrlSoft.NET.Common.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static FieldInfo? GetBackingField(this PropertyInfo source)
        {
            var backingFieldName = $"<{source.Name}>k__BackingField";

            var result = source.DeclaringType?
                .GetField( backingFieldName, BindingFlags.Instance | BindingFlags.NonPublic );

            return result;
        }
    }
}
