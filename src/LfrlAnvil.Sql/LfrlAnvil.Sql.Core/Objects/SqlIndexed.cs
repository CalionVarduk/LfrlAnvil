using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

public readonly record struct SqlIndexed<T>(T? Column, OrderBy Ordering)
    where T : class, ISqlColumn
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlIndexed<ISqlColumn>(SqlIndexed<T> source)
    {
        return new SqlIndexed<ISqlColumn>( source.Column, source.Ordering );
    }
}
