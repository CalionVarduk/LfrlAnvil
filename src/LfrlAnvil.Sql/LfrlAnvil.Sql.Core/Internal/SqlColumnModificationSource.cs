using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a source of <see cref="ISqlColumnBuilder"/> modifications.
/// </summary>
/// <param name="Column">Modified column.</param>
/// <param name="Source">Source of column modifications.</param>
/// <typeparam name="T">SQL column builder type.</typeparam>
public readonly record struct SqlColumnModificationSource<T>(T Column, T Source)
    where T : ISqlColumnBuilder
{
    /// <summary>
    /// Creates a new <see cref="SqlColumnModificationSource{T}"/> instance with the same <see cref="Column"/> and <see cref="Source"/>.
    /// </summary>
    /// <param name="column">Modified column.</param>
    /// <returns>New <see cref="SqlColumnModificationSource{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlColumnModificationSource<T> Self(T column)
    {
        return new SqlColumnModificationSource<T>( column, column );
    }
}
