using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal PostgreSqlSchemaBuilderCollection() { }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Database" />
    public new PostgreSqlDatabaseBuilder Database => ReinterpretCast.To<PostgreSqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Default" />
    public new PostgreSqlSchemaBuilder Default => ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Default );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Get(string)" />
    [Pure]
    public new PostgreSqlSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.TryGet(string)" />
    [Pure]
    public new PostgreSqlSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Create(string)" />
    public new PostgreSqlSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.GetOrCreate(string)" />
    public new PostgreSqlSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<PostgreSqlSchemaBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, PostgreSqlSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<PostgreSqlSchemaBuilder>();
    }

    /// <inheritdoc />
    protected override PostgreSqlSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new PostgreSqlSchemaBuilder( Database, name );
    }
}
