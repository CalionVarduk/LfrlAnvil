using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlDataTypeExtensions
{
    [Pure]
    public static ISqlDataType GetRoot(this ISqlDataType type)
    {
        var result = type;
        while ( result.ParentType is not null )
            result = result.ParentType;

        return result;
    }

    [Pure]
    public static bool IsCompatibleWith(this ISqlDataType type, ISqlDataType targetType)
    {
        return ReferenceEquals( type.GetRoot(), targetType.GetRoot() );
    }
}
