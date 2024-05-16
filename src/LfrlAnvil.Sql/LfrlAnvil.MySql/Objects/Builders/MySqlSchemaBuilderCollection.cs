using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal MySqlSchemaBuilderCollection() { }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Default" />
    public new MySqlSchemaBuilder Default => ReinterpretCast.To<MySqlSchemaBuilder>( base.Default );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Get(string)" />
    [Pure]
    public new MySqlSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.TryGet(string)" />
    [Pure]
    public new MySqlSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Create(string)" />
    public new MySqlSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.GetOrCreate(string)" />
    public new MySqlSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, MySqlSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlSchemaBuilder>();
    }

    /// <inheritdoc />
    protected override MySqlSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new MySqlSchemaBuilder( Database, name );
    }
}
