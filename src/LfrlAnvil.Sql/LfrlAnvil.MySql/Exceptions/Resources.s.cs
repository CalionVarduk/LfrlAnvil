using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.MySql.Exceptions;

internal static class Resources
{
    internal const string TemporaryViewsAreForbidden = "temporary views are forbidden";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string IndexFiltersAreForbidden(MySqlIndexBuilder index, SqlConditionNode condition)
    {
        return $"Cannot set '{condition}' as '{index}' filter because index filters are forbidden.";
    }
}
