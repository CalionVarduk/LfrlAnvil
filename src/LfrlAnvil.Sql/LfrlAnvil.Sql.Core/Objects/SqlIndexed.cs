using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

/// <summary>
/// Represents an indexed SQL expression.
/// </summary>
/// <param name="Column">Optional <see cref="ISqlColumn"/> instance.</param>
/// <param name="Ordering">Ordering of this indexed expression.</param>
/// <typeparam name="T">SQL column type.</typeparam>
public readonly record struct SqlIndexed<T>(T? Column, OrderBy Ordering)
    where T : class, ISqlColumn
{
    /// <summary>
    /// Creates a new <see cref="SqlIndexed{T}"/> instance with base <see cref="ISqlColumn"/> type.
    /// </summary>
    /// <param name="source">Source to convert.</param>
    /// <returns>New <see cref="SqlIndexed{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator SqlIndexed<ISqlColumn>(SqlIndexed<T> source)
    {
        return new SqlIndexed<ISqlColumn>( source.Column, source.Ordering );
    }
}
