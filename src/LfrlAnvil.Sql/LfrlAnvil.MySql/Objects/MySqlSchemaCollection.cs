using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlSchemaCollection : SqlSchemaCollection
{
    internal MySqlSchemaCollection(MySqlSchemaBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlSchemaCollection.Default" />
    public new MySqlSchema Default => ReinterpretCast.To<MySqlSchema>( base.Default );

    /// <inheritdoc cref="SqlSchemaCollection.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );

    /// <inheritdoc cref="SqlSchemaCollection.Get(string)" />
    [Pure]
    public new MySqlSchema Get(string name)
    {
        return ReinterpretCast.To<MySqlSchema>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaCollection.TryGet(string)" />
    [Pure]
    public new MySqlSchema? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlSchema>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlSchema, MySqlSchema> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlSchema>();
    }

    /// <inheritdoc />
    protected override MySqlSchema CreateSchema(SqlSchemaBuilder builder)
    {
        return new MySqlSchema( Database, ReinterpretCast.To<MySqlSchemaBuilder>( builder ) );
    }
}
