using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public static class SqlObjectBuilderReference
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderReference<T> Create<T>(SqlObjectBuilderReferenceSource<T> source, T target)
        where T : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReference<T>( source, target );
    }
}
