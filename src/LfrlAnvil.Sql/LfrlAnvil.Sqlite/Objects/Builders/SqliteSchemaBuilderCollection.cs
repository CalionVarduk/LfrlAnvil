using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal SqliteSchemaBuilderCollection() { }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Default" />
    public new SqliteSchemaBuilder Default => ReinterpretCast.To<SqliteSchemaBuilder>( base.Default );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Get(string)" />
    [Pure]
    public new SqliteSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.TryGet(string)" />
    [Pure]
    public new SqliteSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Create(string)" />
    public new SqliteSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.GetOrCreate(string)" />
    public new SqliteSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqliteSchemaBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, SqliteSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteSchemaBuilder>();
    }

    /// <inheritdoc />
    protected override SqliteSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new SqliteSchemaBuilder( Database, name );
    }
}
