using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Creates instances of <see cref="SqlObjectBuilderReference{T}"/> type.
/// </summary>
public static class SqlObjectBuilderReference
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderReference{T}"/> instance.
    /// </summary>
    /// <param name="source">Underlying reference source.</param>
    /// <param name="target">Target object builder.</param>
    /// <typeparam name="T">SQL object builder type.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderReference{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderReference<T> Create<T>(SqlObjectBuilderReferenceSource<T> source, T target)
        where T : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderReference<T>( source, target );
    }
}
