using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Creates instances of <see cref="SqlObjectBuilderReferenceSource{T}"/> type.
/// </summary>
public static class SqlObjectBuilderReferenceSource
{
    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.
    /// </summary>
    /// <param name="object">Referencing object.</param>
    /// <param name="property">
    /// Optional name of the referencing property of <see cref="SqlObjectBuilderReferenceSource{T}.Object"/>. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="SqlObjectBuilderReferenceSource{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderReferenceSource<SqlObjectBuilder> Create(SqlObjectBuilder @object, string? property = null)
    {
        return new SqlObjectBuilderReferenceSource<SqlObjectBuilder>( @object, property );
    }
}
